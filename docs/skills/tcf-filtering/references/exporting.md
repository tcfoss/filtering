# Export Reference

## CSV—TcfOss.Filtering.CsvOutput

### Synchronous

```csharp
// Write to an existing stream
dbContext.Employees.ToCsvStream(responseStream, filter, sorts, maxRows: 10_000);

// Get a new MemoryStream (caller disposes)
Stream csv = dbContext.Employees.ToCsvStream(filter, sorts);
```

### Async streaming (large datasets, EF Core)

```csharp
using TcfOss.Filtering.EntityFrameworkCore.Queryable;

// Write to existing stream
await dbContext.Employees
    .ApplyFiltering(filter, new ValueManager())
    .ApplySorting(sorts)
    .AsAsyncEnumerable()
    .ToCsvStreamAsync(responseStream);

// Get a new MemoryStream
Stream csv = await dbContext.Employees
    .ApplyFiltering(filter, new ValueManager())
    .ApplySorting(sorts)
    .AsAsyncEnumerable()
    .ToCsvStreamAsync();
```

Use `TcfOss.Filtering.Linq.Queryable` instead of the EF namespace when EF Core is not available.

## XLSX—TcfOss.Filtering.XlsxOutput

Mirrors the CSV API. Replace `ToCsvStream`/`ToCsvStreamAsync` with `ToExcelStream`/`ToExcelStreamAsync`.

The XLSX stream must support seek/read/write.

## SerializationOptions

```csharp
var options = new SerializationOptions
{
    Delimiter      = ";",                          // CSV only
    BlankValue     = "N/A",
    IncludeHeaders = true,
    Headers        = ["Full Name", "Department"],  // overrides property names
};
```
