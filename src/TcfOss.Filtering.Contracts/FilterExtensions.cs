namespace TcfOss.Filtering.Contracts;

public static class FilterExtensions
{
    public static Filter? Map(this Filter? filter, Func<Filter, Filter?> transform)
    {
        if (filter == null)
        {
            return null;
        }

        if (filter is CompositeFilter composite)
        {
            Filter[] transformedFilters = [.. composite
                .Filters
                .Select(f => f.Map(transform))
                .Where(f => f != null)!];

            return transformedFilters.Length switch
            {
                0 => null,
                1 => transformedFilters[0],
                _ => transform(composite with { Filters = transformedFilters })
            };
        }

        if (filter is QuantifiedFilter quantified)
        {
            Filter? transformedSubFilter = quantified.SubFilter.Map(transform);
            if (transformedSubFilter == null)
            {
                return null;
            }

            return transform(quantified with { SubFilter = transformedSubFilter });
        }

        return transform(filter);
    }

    public static Filter? Find(this Filter? filter, Func<Filter, bool> predicate)
    {
        if (filter == null)
        {
            return null;
        }

        if (predicate(filter))
        {
            return filter;
        }

        if (filter is CompositeFilter composite)
        {
            foreach (Filter f in composite.Filters)
            {
                Filter? found = f.Find(predicate);
                if (found != null)
                {
                    return found;
                }
            }
        }

        if (filter is QuantifiedFilter quantified)
        {
            return quantified.SubFilter.Find(predicate);
        }

        return null;
    }

    public static Filter? TryExtract<T>(this Filter? filter, Func<Filter, T?> extractor, out T? extracted) where T : class
    {
        extracted = null;

        T? result = null;
        Filter? remaining = filter.Map(f =>
        {
            if (result == null)
            {
                T? value = extractor(f);
                if (value != null)
                {
                    result = value;
                    return null;
                }
            }
            return f;
        });

        extracted = result;
        return remaining;
    }

    public static Filter? Merge(this Filter? filter, Filter? other, string op = LogicalOperators.And)
    {
        if (filter == null)
        {
            return other;
        }

        if (other == null)
        {
            return filter;
        }

        if (filter is CompositeFilter comp
                && other is CompositeFilter otherComp
                && comp.LogicalOperator == op
                && otherComp.LogicalOperator == op)
        {
            return new CompositeFilter(op, [.. comp.Filters, .. otherComp.Filters]);
        }

        if (filter is CompositeFilter compFilter && compFilter.LogicalOperator == op)
        {
            return new CompositeFilter(op, [.. compFilter.Filters, other]);
        }
        if (other is CompositeFilter compOther && compOther.LogicalOperator == op)
        {
            return new CompositeFilter(op, [filter, .. compOther.Filters]);
        }

        return new CompositeFilter(op, [filter, other]);
    }
}
