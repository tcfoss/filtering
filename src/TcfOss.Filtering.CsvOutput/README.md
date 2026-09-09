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

- [Exporting data](https://github.com/tcfoss/filtering/blob/master/docs/ExportingData.md)
- [TcfOss.Filtering.Linq](https://github.com/tcfoss/filtering/blob/master/src/TcfOss.Filtering.Linq/README.md)
- [Repository README](https://github.com/tcfoss/filtering/blob/master/README.md)
