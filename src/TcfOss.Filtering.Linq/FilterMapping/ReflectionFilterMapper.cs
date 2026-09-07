using System.Buffers;
using System.Collections.Concurrent;
using TcfOss.Filtering.Contracts;

namespace TcfOss.Filtering.Linq.FilterMapping;

public class ReflectionFilterMapper<T>(IEnumerable<string>? whitelist = null, IEnumerable<string>? blacklist = null)
    : BasicFilterMapper
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly ConcurrentDictionary<string, Func<string, object>> s_valueParsers = new(comparer: StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, Type> s_propertyTypes = new(comparer: StringComparer.OrdinalIgnoreCase);

    private readonly HashSet<string>? _whitelist = whitelist is null ? null : new HashSet<string>(whitelist, StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string>? _blacklist = blacklist is null ? null : new HashSet<string>(blacklist, StringComparer.OrdinalIgnoreCase);

    public override SimpleFilter ToFilter(Contracts.SimpleFilter dto)
    {
        var op = QueryOperator.ParseContractKey(dto.Operator);

        Func<string, object> parser = GetValueParser(dto.Field);

        if (dto.Operator is FilterOperators.IsNull or FilterOperators.IsNotNull)
        {
            return new SimpleFilter()
            {
                Field = dto.Field,
                Operator = op,
                Value = null!
            };
        }
        else if (dto.Value == null)
        {
            throw new FilterMappingException(string.Format(FilterMappingException.NullValueNotAllowedPattern, dto.Operator));
        }

        try
        {
            object value = parser(dto.Value);
            return new SimpleFilter()
            {
                Field = dto.Field,
                Operator = op,
                Value = value
            };
        }
        catch (FormatException ex)
        {
            throw new FilterMappingException(string.Format(FilterMappingException.InvalidValuePattern, dto.Field, dto.Value), ex);
        }
    }

    public override SetFilter ToFilter(Contracts.SetFilter dto)
    {
        if (dto.Values.Length == 0)
        {
            throw new FilterMappingException(string.Format(FilterMappingException.EmptyValuesPattern, dto.Field));
        }

        Func<string, object> parser = GetValueParser(dto.Field);
        object[] values = new object[dto.Values.Length];
        for (int i = 0; i < dto.Values.Length; i++)
        {
            try
            {
                values[i] = parser(dto.Values[i]);
            }
            catch (FormatException ex)
            {
                throw new FilterMappingException(string.Format(FilterMappingException.InvalidValuePattern, dto.Field, dto.Values[i]), ex);
            }
        }

        Type elementType = s_propertyTypes.GetOrAdd(dto.Field, _ =>
        {
            return typeof(T).GetPropertyTypePreserveNullability(dto.Field);
        });

        return new SetFilter(dto.Field, values, dto.Negated, elementType);
    }

    public override RangeFilter ToFilter(Contracts.RangeFilter dto)
    {
        Func<string, object> parser = GetValueParser(dto.Field);

        object valueFrom;
        object valueTo;

        try
        {
            valueFrom = parser(dto.ValueFrom);
        }
        catch (FormatException ex)
        {
            throw new FilterMappingException(string.Format(FilterMappingException.InvalidValuePattern, dto.Field, dto.ValueFrom), ex);
        }

        try
        {
            valueTo = parser(dto.ValueTo);
        }
        catch (FormatException ex)
        {
            throw new FilterMappingException(string.Format(FilterMappingException.InvalidValuePattern, dto.Field, dto.ValueTo), ex);
        }

        return new RangeFilter(dto.Field, valueFrom, valueTo, dto.Exclusive, dto.Negated);
    }

    public override SortComponent ToSortComponent(Contracts.SortComponent dto)
    {
        _ = GetValueParser(dto.Field); // Validate field exists, access control, and navigation path
        return base.ToSortComponent(dto);
    }

    public override string ToRequestedField(string field)
    {
        _ = GetValueParser(field); // Validate field exists, access control, and navigation path
        return base.ToRequestedField(field);
    }

    public override QuantifiedFilter ToFilter(Contracts.QuantifiedFilter dto)
    {
        // Validate the outer collection field against whitelist/blacklist and that it exists on T.
        // We cannot use GetValueParser here because collection types have no value parser.
        ValidateFieldAccess(dto.Field);
        _ = typeof(T).GetPropertyType(dto.Field);
        return base.ToFilter(dto);
    }


    private void ValidateFieldAccess(string fieldName)
    {
        BlacklistGuard(fieldName, _blacklist);
        WhitelistGuard(fieldName, _whitelist);
    }

    private Func<string, object> GetValueParser(string fieldName)
    {
        ValidateFieldAccess(fieldName);

        return s_valueParsers.GetOrAdd(fieldName, _ =>
        {
            Type propType = typeof(T).GetPropertyType(fieldName);
            return propType.ConstructValueParser(fieldName);
        });
    }

    private static void WhitelistGuard(string fieldName, HashSet<string>? whitelist)
    {
        if (whitelist is null)
        {
            return;
        }

        if (MatchesWildcardPattern(fieldName, whitelist))
        {
            return;
        }

        throw new FilterMappingException(string.Format(FilterMappingException.UnknownFieldPattern, fieldName));
    }

    private static void BlacklistGuard(string fieldName, HashSet<string>? blacklist)
    {
        if (blacklist is null)
        {
            return;
        }

        if (MatchesWildcardPattern(fieldName, blacklist))
        {
            throw new FilterMappingException(string.Format(FilterMappingException.UnknownFieldPattern, fieldName));
        }
    }

    /// <summary>
    /// Returns true if <paramref name="fieldName"/> matches an exact entry in
    /// <paramref name="patterns"/>, or matches any ancestor's "<c>parent.**</c>" wildcard,
    /// or its immediate parent's "<c>parent.*</c>" wildcard. Implemented with a single
    /// pooled char buffer and span-based hash lookups to avoid per-segment allocations.
    /// </summary>
    private static bool MatchesWildcardPattern(string fieldName, HashSet<string> patterns)
    {
        HashSet<string>.AlternateLookup<ReadOnlySpan<char>> lookup =
            patterns.GetAlternateLookup<ReadOnlySpan<char>>();

        if (lookup.Contains(fieldName.AsSpan()))
        {
            return true;
        }

        if (!fieldName.Contains('.'))
        {
            return false;
        }

        // Need room for fieldName + ".**"
        int bufferSize = fieldName.Length + 3;
        char[] rented = ArrayPool<char>.Shared.Rent(bufferSize);
        try
        {
            Span<char> buffer = rented.AsSpan(0, bufferSize);
            fieldName.AsSpan().CopyTo(buffer);

            ReadOnlySpan<char> nameSpan = fieldName.AsSpan();
            int searchFrom = 0;
            while (searchFrom < nameSpan.Length)
            {
                int relIdx = nameSpan[searchFrom..].IndexOf('.');
                if (relIdx < 0)
                {
                    break;
                }
                int prefixEnd = searchFrom + relIdx;

                // Save original chars we're about to overwrite so we can restore
                // them before the next iteration; otherwise a left-ancestor lookup
                // here corrupts a deeper-ancestor lookup later.
                char saved1 = buffer[prefixEnd + 1];
                char saved2 = buffer[prefixEnd + 2];

                // Write ".**" suffix at prefixEnd. buffer[prefixEnd] is already '.'.
                buffer[prefixEnd + 1] = '*';
                buffer[prefixEnd + 2] = '*';

                if (lookup.Contains(buffer[..(prefixEnd + 3)]))
                {
                    return true;
                }

                // If this dot is the last dot in fieldName, also check parent + ".*"
                bool isLastDot = nameSpan[(prefixEnd + 1)..].IndexOf('.') < 0;
                if (isLastDot && lookup.Contains(buffer[..(prefixEnd + 2)]))
                {
                    return true;
                }

                // Restore originals for the next iteration's lookup.
                buffer[prefixEnd + 1] = saved1;
                buffer[prefixEnd + 2] = saved2;

                searchFrom = prefixEnd + 1;
            }
        }
        finally
        {
            ArrayPool<char>.Shared.Return(rented);
        }

        return false;
    }
}
