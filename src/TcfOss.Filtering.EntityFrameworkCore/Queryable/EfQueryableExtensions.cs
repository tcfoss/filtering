using System.Collections.Concurrent;
using System.Dynamic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TcfOss.Filtering.Linq;
using TcfOss.Filtering.Linq.Queryable;

namespace TcfOss.Filtering.EntityFrameworkCore.Queryable;

public static class EfQueryableExtensions
{
    private static readonly ConcurrentDictionary<Type, (string Name, Func<object, object?> Getter)[]> s_propertyCache = new();

    /// <summary>
    /// Applies sorting to the query using the EF-aware parsing configuration.
    /// </summary>
    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, SortComponent[]? sorts)
        => LinqQueryableExtensions.ApplySorting(query, sorts, CustomParsingConfig.ParsingConfig);

    /// <summary>
    /// Applies sorting and paging to the query using the EF-aware parsing configuration.
    /// </summary>
    public static IQueryable<T> ApplyPagingAndSorting<T>(this IQueryable<T> query, SortComponent[] sorts, int page, int pageSize)
        => LinqQueryableExtensions.ApplyPagingAndSorting(query, sorts, page, pageSize, CustomParsingConfig.ParsingConfig);

    /// <summary>
    /// Applies sorting and paging to the query using the EF-aware parsing configuration.
    /// </summary>
    public static IQueryable<T> ApplyPagingAndSorting<T>(this IQueryable<T> query, DataRequest dataFilter)
        => LinqQueryableExtensions.ApplyPagingAndSorting(query, dataFilter, CustomParsingConfig.ParsingConfig);

    /// <summary>
    /// Applies filtering to the query using the EF-aware parsing configuration.
    /// </summary>
    public static IQueryable<T> ApplyFiltering<T>(this IQueryable<T> query, IFilter? filter, IManageValues valueManager)
        => LinqQueryableExtensions.ApplyFiltering(query, filter, valueManager, CustomParsingConfig.ParsingConfig);

    /// <summary>
    /// Applies filtering to the query using the EF-aware parsing configuration.
    /// </summary>
    public static IQueryable<T> ApplyFiltering<T>(this IQueryable<T> query, IFilter? filter)
        => LinqQueryableExtensions.ApplyFiltering(query, filter, new ValueManager(), CustomParsingConfig.ParsingConfig);

    /// <summary>
    /// Applies filtering and paging/sorting to the query using the EF-aware parsing configuration.
    /// </summary>
    public static IQueryable<T> ApplyDataFilter<T>(this IQueryable<T> query, DataRequest dataFilter, IManageValues valueManager)
        => LinqQueryableExtensions.ApplyDataFilter(query, dataFilter, valueManager, CustomParsingConfig.ParsingConfig);

    /// <summary>
    /// Applies filtering and paging/sorting to the query using a default value manager and the EF-aware parsing configuration.
    /// </summary>
    public static IQueryable<T> ApplyDataFilter<T>(this IQueryable<T> query, DataRequest dataFilter)
        => LinqQueryableExtensions.ApplyDataFilter(query, dataFilter, new ValueManager(), CustomParsingConfig.ParsingConfig);

    /// <summary>
    /// Projects the requested fields into a dynamic result shape using the EF-aware parsing configuration.
    /// </summary>
    public static IQueryable<dynamic> ApplyFieldSelection<T>(this IQueryable<T> query, string[] requestedFields)
        => LinqQueryableExtensions.ApplyFieldSelection(query, requestedFields, CustomParsingConfig.ParsingConfig);

    /// <summary>
    /// Materializes the query into a paged data result using the EF-aware parsing configuration.
    /// </summary>
    public static Contracts.DataResult<T> ToDataResult<T>(this IQueryable<T> query, DataRequest dataFilter, IManageValues valueManager)
        => LinqQueryableExtensions.ToDataResult(query, dataFilter, valueManager, CustomParsingConfig.ParsingConfig);

    /// <summary>
    /// Materializes the query into a paged data result using a default value manager and the EF-aware parsing configuration.
    /// </summary>
    public static Contracts.DataResult<T> ToDataResult<T>(this IQueryable<T> query, DataRequest dataFilter)
        => LinqQueryableExtensions.ToDataResult(query, dataFilter, new ValueManager(), CustomParsingConfig.ParsingConfig);

    /// <summary>
    /// Materializes the query into a dynamic paged data result using the EF-aware parsing configuration.
    /// </summary>
    public static Contracts.DataResult<dynamic> ToDataResult<T>(this IQueryable<T> query, DynamicDataRequest dataFilter, IManageValues valueManager)
        => LinqQueryableExtensions.ToDataResult(query, dataFilter, valueManager, CustomParsingConfig.ParsingConfig);

    /// <summary>
    /// Materializes the query into a dynamic paged data result using a default value manager and the EF-aware parsing configuration.
    /// </summary>
    public static Contracts.DataResult<dynamic> ToDataResult<T>(this IQueryable<T> query, DynamicDataRequest dataFilter)
        => LinqQueryableExtensions.ToDataResult(query, dataFilter, new ValueManager(), CustomParsingConfig.ParsingConfig);

    /// <summary>
    /// Asynchronously materializes the query into a paged data result using EF Core async operators.
    /// </summary>
    public static async Task<Contracts.DataResult<T>> ToDataResultAsync<T>(
        this IQueryable<T> query,
        DataRequest dataFilter,
        IManageValues valueManager,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> filteredQuery = query.ApplyFiltering(dataFilter.Filter, valueManager, CustomParsingConfig.ParsingConfig);

        int totalCount = await filteredQuery.CountAsync(cancellationToken);
        List<T> items = await filteredQuery.ApplyPagingAndSorting(dataFilter).ToListAsync(cancellationToken);

        return new Contracts.DataResult<T>(items, totalCount, dataFilter.Page, dataFilter.PageSize);
    }

    /// <summary>
    /// Asynchronously materializes the query into a paged data result using a default value manager.
    /// </summary>
    public static Task<Contracts.DataResult<T>> ToDataResultAsync<T>(
        this IQueryable<T> query,
        DataRequest dataFilter,
        CancellationToken cancellationToken = default)
        => query.ToDataResultAsync(dataFilter, new ValueManager(), cancellationToken);

    /// <summary>
    /// Asynchronously materializes the query into a dynamic paged data result using EF Core async operators.
    /// </summary>
    public static async Task<Contracts.DataResult<dynamic>> ToDataResultAsync<T>(
        this IQueryable<T> query,
        DynamicDataRequest dataFilter,
        IManageValues valueManager,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> filteredQuery = query.ApplyFiltering(dataFilter.Filter, valueManager, CustomParsingConfig.ParsingConfig);

        int totalCount = await filteredQuery.CountAsync(cancellationToken);
        List<dynamic> rawItems = await filteredQuery
            .ApplyPagingAndSorting(dataFilter)
            .ApplyFieldSelection(dataFilter.RequestedFields)
            .ToListAsync(cancellationToken);

        Dictionary<string, string> aliasToOriginal = dataFilter.RequestedFields
            .Where(f => f.Contains('.'))
            .ToDictionary(f => f.Replace('.', '_'), f => f);

        List<dynamic> items = [.. rawItems.Select(item => (dynamic)RemapFieldNames((object)item, aliasToOriginal))];

        return new Contracts.DataResult<dynamic>(items, totalCount, dataFilter.Page, dataFilter.PageSize);
    }


    /// <summary>
    /// Asynchronously materializes the query into a dynamic paged data result using a default value manager.
    /// </summary>
    public static Task<Contracts.DataResult<dynamic>> ToDataResultAsync<T>(
        this IQueryable<T> query,
        DynamicDataRequest dataFilter,
        CancellationToken cancellationToken = default)
        => query.ToDataResultAsync(dataFilter, new ValueManager(), cancellationToken);


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
        foreach ((string? name, Func<object, object?>? getter) in GetCachedProperties(item.GetType()))
        {
            string mappedName = aliasToOriginal.TryGetValue(name, out string? original) ? original : name;
            dict[mappedName] = getter(item);
        }
        return expando;
    }
}

