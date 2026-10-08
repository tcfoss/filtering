# TcfOss.Filtering.XlsxOutput

[Documentation](https://tcfoss.github.io/filtering/) | [Repository](https://github.com/tcfoss/filtering)

XLSX export extensions for filtered/sorted `IQueryable<T>` data.

## What this package adds

- `ToExcelStream` overloads for direct query export to OpenXML workbook streams.
- Overloads that accept `IFilter`, `SortComponent[]`, `maxRows`, and optional `ParsingConfig`.
- Async stream writing for `IAsyncEnumerable<T>`.

## Typical usage

```csharp
using Stream xlsx = query.ToExcelStream(filter, sorts, maxRows: 1000);
```

With explicit parsing config:

```csharp
using Stream xlsx = query.ToExcelStream(filter, sorts, maxRows: 1000, parsingConfig: customConfig);
```

## Related docs

- [Exporting data](https://tcfoss.github.io/filtering/ExportingData/)
- [TcfOss.Filtering.Linq](https://tcfoss.github.io/filtering/packages/linq/)
- [Repository README](https://github.com/tcfoss/filtering/blob/master/README.md)
