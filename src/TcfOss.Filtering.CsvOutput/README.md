# TcfOss.Filtering.CsvOutput

[Documentation](https://tcfoss.github.io/filtering/) | [Repository](https://github.com/tcfoss/filtering)

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

- [Exporting data](https://tcfoss.github.io/filtering/ExportingData/)
- [TcfOss.Filtering.Linq](https://tcfoss.github.io/filtering/packages/linq/)
- [Repository README](https://github.com/tcfoss/filtering/blob/master/README.md)
