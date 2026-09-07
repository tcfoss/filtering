# Exporting Data

Two packages supporting exporting filtered/sorted queries to files are available:

- `TcfOss.Filtering.CsvOutput` for exporting to CSV format.
- `TcfOss.Filtering.XlsxOutput` for exporting to Excel format.


## TcfOss.Filtering.CsvOutput

Exports a filtered/sorted query to CSV via `IQueryable<T>` extension methods.

```csharp
// Write to an existing stream
dbContext.Employees.ToCsvStream(responseStream, filter, sorts, maxRows: 10_000);

// Or get a new MemoryStream (caller disposes)
Stream csv = dbContext.Employees.ToCsvStream(filter, sorts);
```

`SerializationOptions` controls the delimiter, blank value, and header row:

```csharp
var options = new SerializationOptions
{
    Delimiter = ";",
    BlankValue = "N/A",
    IncludeHeaders = true, // whether to write a header row with property names
    Headers = ["Full Name", "Department"],  // overrides property names
};
```

For full streaming behavior (useful for very large data sets), use `ToCsvStreamAsync`, which
accepts any `IAsyncEnumerable<T>`. For EF Core queries, call `AsAsyncEnumerable()` to obtain
one. Filtering and sorting must be applied to the `IQueryable<T>` before converting:

```csharp
using TcfOss.Filtering.EntityFrameworkCore.Queryable;

// Write to an existing stream
await dbContext.Employees
    .ApplyFiltering(filter, new ValueManager())
    .ApplySorting(sorts)
    .AsAsyncEnumerable()
    .ToCsvStreamAsync(responseStream);

// Or get a new MemoryStream (caller disposes)
Stream csv = await dbContext.Employees
    .ApplyFiltering(filter, new ValueManager())
    .ApplySorting(sorts)
    .AsAsyncEnumerable()
    .ToCsvStreamAsync();
```

Use `TcfOss.Filtering.Linq.Queryable` instead if `Microsoft.EntityFrameworkCore` is not available.


## TcfOss.Filtering.XlsxOutput

Exports a filtered/sorted query to XLSX via `IQueryable<T>` extension methods. The API mirrors `CsvOutput`.

```csharp
// Write to an existing stream (must support seek/read/write)
dbContext.Employees.ToExcelStream(responseStream, filter, sorts, maxRows: 10_000);

// Or get a new MemoryStream (caller disposes)
Stream xlsx = dbContext.Employees.ToExcelStream(filter, sorts);
```

`SerializationOptions` controls the blank value and header row (no delimiter setting for XLSX).

The same async streaming behavior is available via `ToExcelStreamAsync`:

```csharp
using TcfOss.Filtering.EntityFrameworkCore.Queryable;

// Write to an existing stream
await dbContext.Employees
    .ApplyFiltering(filter, new ValueManager())
    .ApplySorting(sorts)
    .AsAsyncEnumerable()
    .ToExcelStreamAsync(responseStream);

// Or get a new MemoryStream (caller disposes)
Stream xlsx = await dbContext.Employees
    .ApplyFiltering(filter, new ValueManager())
    .ApplySorting(sorts)
    .AsAsyncEnumerable()
    .ToExcelStreamAsync();
```

Use `TcfOss.Filtering.Linq.Queryable` instead if `Microsoft.EntityFrameworkCore` is not available.
