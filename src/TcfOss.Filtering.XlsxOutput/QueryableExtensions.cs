using System.Linq.Dynamic.Core;
using TcfOss.Filtering.Linq;
using TcfOss.Filtering.Linq.Queryable;

namespace TcfOss.Filtering.XlsxOutput;

public static class QueryableExtensions
{

    /// <summary>
    /// Writes filtered and sorted query results to the provided XLSX stream using the specified parsing configuration.
    /// </summary>
    /// <remarks>
    /// If your filter uses LIKE or NOT LIKE operators, pass a ParsingConfig that includes the EF-aware type provider.
    /// </remarks>
    public static void ToExcelStream<T>(this IQueryable<T> data, Stream stream, IFilter? filter, SortComponent[]? sorts, int? maxRows, IManageValues valueManager, ParsingConfig parsingConfig, SerializationOptions options)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(valueManager);
        ArgumentNullException.ThrowIfNull(parsingConfig);
        ArgumentNullException.ThrowIfNull(options);

        if (filter != null)
        {
            data = data.ApplyFiltering(filter, valueManager, parsingConfig);
        }

        data = data.ApplySorting(sorts, parsingConfig);

        if (maxRows.HasValue)
        {
            data = data.Take(maxRows.Value);
        }

        using ExcelObjectWriter<T> writer = new(stream, options);
        writer.WriteHeaders();
        foreach (T obj in data)
        {
            writer.WriteObject(obj);
        }
    }

    /// <summary>
    /// Writes the query results to the provided XLSX stream.
    /// </summary>
    public static void ToExcelStream<T>(this IQueryable<T> data, Stream stream, SerializationOptions? options)
    {
        data.ToExcelStream(stream, filter: null, sorts: null, maxRows: null, valueManager: new ValueManager(), parsingConfig: ParsingConfig.Default, options ?? new SerializationOptions());
    }

    /// <summary>
    /// Writes the query results to the provided XLSX stream using default options.
    /// </summary>
    public static void ToExcelStream<T>(this IQueryable<T> data, Stream stream)
        => data.ToExcelStream(stream, options: (SerializationOptions?)null);

    /// <summary>
    /// Writes the query results to a new XLSX memory stream.
    /// </summary>
    public static Stream ToExcelStream<T>(this IQueryable<T> data, SerializationOptions? options)
    {
        MemoryStream stream = new();
        data.ToExcelStream(stream, options);
        stream.Position = 0;
        return stream;
    }

    /// <summary>
    /// Writes the query results to a new XLSX memory stream using default options.
    /// </summary>
    public static Stream ToExcelStream<T>(this IQueryable<T> data)
        => data.ToExcelStream(options: (SerializationOptions?)null);

    /// <summary>
    /// Writes filtered and sorted query results to the provided XLSX stream.
    /// </summary>
    /// <remarks>
    /// If your filter uses LIKE or NOT LIKE operators, pass a ParsingConfig that includes the EF-aware type provider.
    /// </remarks>
    public static void ToExcelStream<T>(this IQueryable<T> data, Stream stream, IFilter? filter, SortComponent[]? sorts, int? maxRows, SerializationOptions options)
        => data.ToExcelStream(stream, filter, sorts, maxRows, new ValueManager(), ParsingConfig.Default, options);

    /// <summary>
    /// Writes filtered and sorted query results to the provided XLSX stream using default options.
    /// </summary>
    /// <remarks>
    /// If your filter uses LIKE or NOT LIKE operators, pass a ParsingConfig that includes the EF-aware type provider.
    /// </remarks>
    public static void ToExcelStream<T>(this IQueryable<T> data, Stream stream, IFilter? filter, SortComponent[]? sorts, int? maxRows)
        => data.ToExcelStream(stream, filter, sorts, maxRows, options: new SerializationOptions());

    /// <summary>
    /// Writes filtered and sorted query results to a new XLSX memory stream using the specified parsing configuration.
    /// </summary>
    /// <remarks>
    /// If your filter uses LIKE or NOT LIKE operators, pass a ParsingConfig that includes the EF-aware type provider.
    /// </remarks>
    public static Stream ToExcelStream<T>(this IQueryable<T> data, IFilter? filter, SortComponent[]? sorts, int? maxRows, ParsingConfig parsingConfig, SerializationOptions? options = null)
    {
        MemoryStream stream = new();
        data.ToExcelStream(stream, filter, sorts, maxRows, new ValueManager(), parsingConfig, options ?? new SerializationOptions());
        stream.Position = 0;
        return stream;
    }

    /// <summary>
    /// Writes filtered and sorted query results to a new XLSX memory stream using the specified parsing configuration and default options.
    /// </summary>
    /// <remarks>
    /// If your filter uses LIKE or NOT LIKE operators, pass a ParsingConfig that includes the EF-aware type provider.
    /// </remarks>
    public static Stream ToExcelStream<T>(this IQueryable<T> data, IFilter? filter, SortComponent[]? sorts, int? maxRows, ParsingConfig parsingConfig)
        => data.ToExcelStream(filter, sorts, maxRows, parsingConfig, options: null);

    /// <summary>
    /// Writes filtered and sorted query results to a new XLSX memory stream.
    /// </summary>
    /// <remarks>
    /// If your filter uses LIKE or NOT LIKE operators, pass a ParsingConfig that includes the EF-aware type provider.
    /// </remarks>
    public static Stream ToExcelStream<T>(this IQueryable<T> data, IFilter? filter, SortComponent[]? sorts, int? maxRows, SerializationOptions? options)
        => data.ToExcelStream(filter, sorts, maxRows, ParsingConfig.Default, options);

    /// <summary>
    /// Writes filtered and sorted query results to a new XLSX memory stream using default options.
    /// </summary>
    /// <remarks>
    /// If your filter uses LIKE or NOT LIKE operators, pass a ParsingConfig that includes the EF-aware type provider.
    /// </remarks>
    public static Stream ToExcelStream<T>(this IQueryable<T> data, IFilter? filter, SortComponent[]? sorts, int? maxRows)
        => data.ToExcelStream(filter, sorts, maxRows, options: null);


    /// <summary>
    /// Asynchronously streams an <see cref="IAsyncEnumerable{T}"/> to an XLSX stream.
    /// For EF Core queries, call <c>query.AsAsyncEnumerable()</c> to obtain an
    /// <see cref="IAsyncEnumerable{T}"/> that streams rows from the database without
    /// materializing the full result set in memory.
    /// </summary>
    public static async Task ToExcelStreamAsync<T>(
        this IAsyncEnumerable<T> data,
        Stream stream,
        SerializationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new SerializationOptions();

        using ExcelObjectWriter<T> writer = new(stream, options);

        writer.WriteHeaders();
        await foreach (T obj in data.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            writer.WriteObject(obj);
        }
    }

    /// <summary>
    /// Asynchronously streams an <see cref="IAsyncEnumerable{T}"/> to a new
    /// <see cref="MemoryStream"/> in XLSX format.
    /// </summary>
    public static async Task<Stream> ToExcelStreamAsync<T>(
        this IAsyncEnumerable<T> data,
        SerializationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        MemoryStream stream = new();
        await data.ToExcelStreamAsync(stream, options, cancellationToken).ConfigureAwait(false);
        stream.Position = 0;
        return stream;
    }
}
