# TcfOss.Filtering.CsvOutput

CSV export extensions for filtered/sorted `IQueryable<T>` data.

## What this package adds

- `ToCsvStream` overloads for direct query export.
- Overloads that accept `IFilter`, `SortComponent[]`, `maxRows`, and optional `ParsingConfig`.
- Async stream writing for `IAsyncEnumerable<T>`.

## Typical usage

```csharp
using Stream csv = query.ToCsvStream(filter, sorts, maxRows: 1000);
```

With explicit parsing config:

```csharp
using Stream csv = query.ToCsvStream(filter, sorts, maxRows: 1000, parsingConfig: customConfig);
```

## Related docs

- [../../docs/ExportingData.md](../../docs/ExportingData.md)
- [../TcfOss.Filtering.Linq/README.md](../TcfOss.Filtering.Linq/README.md)
- [../../README.md](../../README.md)
