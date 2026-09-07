using System.Collections.Concurrent;
using System.Dynamic;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using TcfOss.Filtering.Contracts;

namespace TcfOss.Filtering.Linq.Queryable;

public static class LinqQueryableExtensions
{
    private static readonly ConcurrentDictionary<Type, (string Name, Func<object, object?> Getter)[]> s_propertyCache = new();
    private static readonly ConcurrentDictionary<Type, string?> s_defaultSortPropertyCache = new();

    /// <summary>
    /// Applies sorting to the query using the specified parsing configuration.
    /// </summary>
    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, SortComponent[]? sorts, ParsingConfig parsingConfig)
    {
        ArgumentNullException.ThrowIfNull(parsingConfig);

        if (sorts == null || sorts.Length == 0)
        {
            string? defaultProp = GetDefaultSortProperty(typeof(T));
            return defaultProp is null
                ? ApplyOrderBy(query, "1", parsingConfig) // No properties available (e.g. anonymous dynamic projections) — fall back to constant ordering
                : ApplyOrderBy(query, defaultProp, parsingConfig);
        }

        string ordering = string.Join(", ", sorts.Select(s => s.ToDynamicLinq()));
        return ApplyOrderBy(query, ordering, parsingConfig);
    }

    /// <summary>
    /// Applies sorting to the query using Dynamic LINQ defaults.
    /// </summary>
    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, SortComponent[]? sorts)
        => query.ApplySorting(sorts, parsingConfig: ParsingConfig.Default);


    private static IQueryable<T> ApplyOrderBy<T>(IQueryable<T> query, string ordering, ParsingConfig parsingConfig)
        => query.OrderBy(parsingConfig, ordering);


    private static string? GetDefaultSortProperty(Type type)
    {
        return s_defaultSortPropertyCache.GetOrAdd(type, static t =>
        {
            System.Reflection.PropertyInfo[] props = [.. t.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Where(p => p.GetIndexParameters().Length == 0)];

            if (props.Length == 0)
            {
                return null;
            }

            // Prefer the first property whose name ends with "Id" (case-insensitive) —
            // covers Id, CustomerId, customer_id, etc.
            System.Reflection.PropertyInfo? idProp = props.FirstOrDefault(p => p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase));
            return (idProp ?? props[0]).Name;
        });
    }

    /// <summary>
    /// Applies sorting and paging to the query using the specified parsing configuration.
    /// </summary>
    public static IQueryable<T> ApplyPagingAndSorting<T>(this IQueryable<T> query, SortComponent[] sorts, int page, int pageSize, ParsingConfig parsingConfig)
    {
        ArgumentNullException.ThrowIfNull(parsingConfig);

        query = query.ApplySorting(sorts, parsingConfig);

        int skip = (page - 1) * pageSize;
        return query.Skip(skip).Take(pageSize);
    }

    /// <summary>
    /// Applies sorting and paging to the query.
    /// </summary>
    public static IQueryable<T> ApplyPagingAndSorting<T>(this IQueryable<T> query, SortComponent[] sorts, int page, int pageSize)
        => query.ApplyPagingAndSorting(sorts, page, pageSize, parsingConfig: ParsingConfig.Default);

    /// <summary>
    /// Applies sorting and paging using the values from a data request and the specified parsing configuration.
    /// </summary>
    public static IQueryable<T> ApplyPagingAndSorting<T>(this IQueryable<T> query, DataRequest dataFilter, ParsingConfig parsingConfig)
    {
        ArgumentNullException.ThrowIfNull(dataFilter);
        ArgumentNullException.ThrowIfNull(parsingConfig);

        return query.ApplyPagingAndSorting(dataFilter.Sorts, dataFilter.Page, dataFilter.PageSize, parsingConfig);
    }

    /// <summary>
    /// Applies sorting and paging using the values from a data request.
    /// </summary>
    public static IQueryable<T> ApplyPagingAndSorting<T>(this IQueryable<T> query, DataRequest dataFilter)
        => query.ApplyPagingAndSorting(dataFilter, parsingConfig: ParsingConfig.Default);

    /// <summary>
    /// Applies a filter expression to the query using the specified parsing configuration.
    /// </summary>
    public static IQueryable<T> ApplyFiltering<T>(this IQueryable<T> query, IFilter? filter, IManageValues valueManager, ParsingConfig parsingConfig)
    {
        ArgumentNullException.ThrowIfNull(valueManager);
        ArgumentNullException.ThrowIfNull(parsingConfig);

        if (filter != null)
        {
            ValidateLikeOperators(filter, parsingConfig);

            string queryString = filter.ToDynamicLinq(valueManager);
            Expression[] values = valueManager.GetValueExpressions();

            query = query.Where(parsingConfig, queryString, values);
        }
        return query;
    }

    /// <summary>
    /// Applies a filter expression to the query.
    /// </summary>
    public static IQueryable<T> ApplyFiltering<T>(this IQueryable<T> query, IFilter? filter, IManageValues valueManager)
        => query.ApplyFiltering(filter, valueManager, parsingConfig: ParsingConfig.Default);

    /// <summary>
    /// Applies filtering and paging/sorting using a data request and the specified parsing configuration.
    /// </summary>
    public static IQueryable<T> ApplyDataFilter<T>(this IQueryable<T> query, DataRequest dataFilter, IManageValues valueManager, ParsingConfig parsingConfig)
    {
        ArgumentNullException.ThrowIfNull(dataFilter);
        ArgumentNullException.ThrowIfNull(valueManager);
        ArgumentNullException.ThrowIfNull(parsingConfig);

        query = query.ApplyFiltering(dataFilter.Filter, valueManager, parsingConfig);
        query = query.ApplyPagingAndSorting(dataFilter, parsingConfig);
        return query;
    }

    /// <summary>
    /// Applies filtering and paging/sorting using a data request.
    /// </summary>
    public static IQueryable<T> ApplyDataFilter<T>(this IQueryable<T> query, DataRequest dataFilter, IManageValues valueManager)
        => query.ApplyDataFilter(dataFilter, valueManager, parsingConfig: ParsingConfig.Default);


    /// <summary>
    /// Applies filtering and paging/sorting using a data request and a default value manager.
    /// </summary>
    public static IQueryable<T> ApplyDataFilter<T>(this IQueryable<T> query, DataRequest dataFilter)
    {
        return query.ApplyDataFilter(dataFilter, new ValueManager(), ParsingConfig.Default);
    }

    /// <summary>
    /// Materializes the query into a paged data result using the specified parsing configuration.
    /// </summary>
    public static DataResult<T> ToDataResult<T>(this IQueryable<T> query, DataRequest dataFilter, IManageValues valueManager, ParsingConfig parsingConfig)
    {
        ArgumentNullException.ThrowIfNull(dataFilter);
        ArgumentNullException.ThrowIfNull(valueManager);
        ArgumentNullException.ThrowIfNull(parsingConfig);

        IQueryable<T> filteredQuery = query.ApplyFiltering(dataFilter.Filter, valueManager, parsingConfig);

        int totalCount = filteredQuery.Count();

        var items = filteredQuery.ApplyPagingAndSorting(dataFilter, parsingConfig).ToList();

        return new DataResult<T>(items, totalCount, dataFilter.Page, dataFilter.PageSize);
    }

    /// <summary>
    /// Materializes the query into a paged data result.
    /// </summary>
    public static DataResult<T> ToDataResult<T>(this IQueryable<T> query, DataRequest dataFilter, IManageValues valueManager)
        => query.ToDataResult(dataFilter, valueManager, parsingConfig: ParsingConfig.Default);


    /// <summary>
    /// Materializes the query into a paged data result using a default value manager.
    /// </summary>
    public static DataResult<T> ToDataResult<T>(this IQueryable<T> query, DataRequest dataFilter)
    {
        return query.ToDataResult(dataFilter, new ValueManager(), ParsingConfig.Default);
    }

    /// <summary>
    /// Projects the requested fields into a dynamic result shape using the specified parsing configuration.
    /// </summary>
    public static IQueryable<dynamic> ApplyFieldSelection<T>(this IQueryable<T> query, string[] requestedFields, ParsingConfig parsingConfig)
    {
        ArgumentNullException.ThrowIfNull(requestedFields);
        ArgumentNullException.ThrowIfNull(parsingConfig);

        IEnumerable<string> terms = requestedFields.Select(f => f.Contains('.')
            ? $"{f} as {f.Replace('.', '_')}"
            : f);
        string selectClause = $"new ({string.Join(", ", terms)})";
        return query.Select(parsingConfig, selectClause).Cast<dynamic>();
    }

    /// <summary>
    /// Projects the requested fields into a dynamic result shape.
    /// </summary>
    public static IQueryable<dynamic> ApplyFieldSelection<T>(this IQueryable<T> query, string[] requestedFields)
        => query.ApplyFieldSelection(requestedFields, parsingConfig: ParsingConfig.Default);


    /// <summary>
    /// Materializes the query into a dynamic paged data result.
    /// </summary>
    public static DataResult<dynamic> ToDataResult<T>(this IQueryable<T> query, DynamicDataRequest dataFilter, IManageValues valueManager)
        => query.ToDataResult(dataFilter, valueManager, parsingConfig: ParsingConfig.Default);

    /// <summary>
    /// Materializes the query into a dynamic paged data result using the specified parsing configuration.
    /// </summary>
    public static DataResult<dynamic> ToDataResult<T>(this IQueryable<T> query, DynamicDataRequest dataFilter, IManageValues valueManager, ParsingConfig parsingConfig)
    {
        ArgumentNullException.ThrowIfNull(dataFilter);
        ArgumentNullException.ThrowIfNull(valueManager);
        ArgumentNullException.ThrowIfNull(parsingConfig);

        IQueryable<T> filteredQuery = query.ApplyFiltering(dataFilter.Filter, valueManager, parsingConfig);

        int totalCount = filteredQuery.Count();

        var rawItems = filteredQuery.ApplyPagingAndSorting(dataFilter, parsingConfig).ApplyFieldSelection(dataFilter.RequestedFields, parsingConfig).ToList();

        Dictionary<string, string> aliasToOriginal = dataFilter.RequestedFields
            .Where(f => f.Contains('.'))
            .ToDictionary(f => f.Replace('.', '_'), f => f);

        List<dynamic> items = [.. rawItems.Select(item => (dynamic)RemapFieldNames((object)item, aliasToOriginal))];

        return new DataResult<dynamic>(items, totalCount, dataFilter.Page, dataFilter.PageSize);
    }

    private static (string Name, Func<object, object?> Getter)[] GetCachedProperties(Type type)
        => s_propertyCache.GetOrAdd(type, static t =>
            [.. t.GetProperties()
             .Where(p => p.GetIndexParameters().Length == 0)
             .Select(p =>
             {
                 ParameterExpression param = Expression.Parameter(typeof(object), "obj");
                 Func<object, object?> getter = Expression.Lambda<Func<object, object?>>(
                     Expression.Convert(
                         Expression.Property(Expression.Convert(param, t), p),
                         typeof(object)),
                     param).Compile();
                 return (p.Name, getter);
             })]);

    private static ExpandoObject RemapFieldNames(object item, Dictionary<string, string> aliasToOriginal)
    {
        var expando = new ExpandoObject();
        var dict = (IDictionary<string, object?>)expando;
        foreach ((string name, Func<object, object?> getter) in GetCachedProperties(item.GetType()))
        {
            string mappedName = aliasToOriginal.GetValueOrDefault(name, name);
            dict[mappedName] = getter(item);
        }
        return expando;
    }

    /// <summary>
    /// Materializes the query into a dynamic paged data result using a default value manager.
    /// </summary>
    public static DataResult<dynamic> ToDataResult<T>(this IQueryable<T> query, DynamicDataRequest dataFilter)
    {
        return query.ToDataResult(dataFilter, new ValueManager());
    }

    private static void ValidateLikeOperators(IFilter filter, ParsingConfig parsingConfig)
    {
        if (!UsesLikeOperator(filter))
        {
            return;
        }

        if (parsingConfig.CustomTypeProvider is null ||
            !ContainsLikeTypes(parsingConfig.CustomTypeProvider.GetCustomTypes()))
        {
            throw new InvalidOperationException("Like and NotLike operators require a ParsingConfig with a custom type provider that registers EF and DbFunctionsExtensions.");
        }
    }

    private static bool ContainsLikeTypes(HashSet<Type> customTypes)
    {
        bool hasEfType = customTypes.Any(static t => t.FullName == "Microsoft.EntityFrameworkCore.EF");
        bool hasDbFunctionsExtensionsType = customTypes.Any(static t => t.FullName == "Microsoft.EntityFrameworkCore.DbFunctionsExtensions");
        return hasEfType && hasDbFunctionsExtensionsType;
    }

    private static bool UsesLikeOperator(IFilter filter)
    {
        return filter switch
        {
            SimpleFilter simple => simple.Operator == QueryOperator.Like || simple.Operator == QueryOperator.NotLike,
            CompositeFilter composite => composite.Filters.Any(UsesLikeOperator),
            QuantifiedFilter quantified => UsesLikeOperator(quantified.SubFilter),
            _ => false
        };
    }
}
