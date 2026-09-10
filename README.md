# TcfOss.Filtering

Monorepo containing filtering, sorting, paging, and export libraries for .NET and TypeScript.

Use this README as an index. Package-level usage and examples live in each package README.

## Package Map

### Core packages

| Package | README | Purpose |
|---|---|---|
| `TcfOss.Filtering.Contracts` | [src/TcfOss.Filtering.Contracts/README.md](src/TcfOss.Filtering.Contracts/README.md) | Wire-level DTOs and constants (`DataRequest`, `DataResult`, operator keys) |
| `TcfOss.Filtering.Linq` | [src/TcfOss.Filtering.Linq/README.md](src/TcfOss.Filtering.Linq/README.md) | Dynamic LINQ filtering/sorting/paging and request mapping |
| `@tcfoss/filtering-contracts` | [npm/tcfoss-filtering-contracts/README.md](npm/tcfoss-filtering-contracts/README.md) | TypeScript mirror of contract types (+ optional Zod schemas) |

### Extension packages

| Package | README | Purpose |
|---|---|---|
| `TcfOss.Filtering.EntityFrameworkCore` | [src/TcfOss.Filtering.EntityFrameworkCore/README.md](src/TcfOss.Filtering.EntityFrameworkCore/README.md) | EF Core async helpers and EF-specific parsing config |
| `TcfOss.Filtering.CsvOutput` | [src/TcfOss.Filtering.CsvOutput/README.md](src/TcfOss.Filtering.CsvOutput/README.md) | CSV export from filtered/sorted queries |
| `TcfOss.Filtering.XlsxOutput` | [src/TcfOss.Filtering.XlsxOutput/README.md](src/TcfOss.Filtering.XlsxOutput/README.md) | XLSX export from filtered/sorted queries |
| `TcfOss.Filtering.Contracts.Newtonsoft` | [src/TcfOss.Filtering.Contracts.Newtonsoft/README.md](src/TcfOss.Filtering.Contracts.Newtonsoft/README.md) | Newtonsoft.Json converter for contract filters |
| `TcfOss.Filtering.AspNetCore` | [src/TcfOss.Filtering.AspNetCore/README.md](src/TcfOss.Filtering.AspNetCore/README.md) | ASP.NET Core middleware for malformed filter payloads |
| `TcfOss.Filtering.Programmatic` | [src/TcfOss.Filtering.Programmatic/README.md](src/TcfOss.Filtering.Programmatic/README.md) | Programmatic mapping of search filters to contract filters |

## Docs

- [docs/ContractKeys.md](docs/ContractKeys.md)
- [docs/RequestMapping.md](docs/RequestMapping.md)
- [docs/ExportingData.md](docs/ExportingData.md)
- [docs/UtilityExtensionPackages.md](docs/UtilityExtensionPackages.md)

## Quick Start

```bash
dotnet build
dotnet test
```

## How It Fits Together

```text
Frontend -> API -> Contracts DTOs -> Mapper -> Linq filters -> IQueryable -> DataResult
```

The `Contracts` package defines the wire format and keys.
The `Linq` package applies filtering/sorting/paging to queries.
Optional extension packages add framework-specific features (EF Core, export writers, ASP.NET middleware, Newtonsoft converter).


## Reporting Issues and Contributing

Issues for this project are tracked on [IssueTracker](https://issues.tcflanagan.net/filtering). If you encounter any bugs or have feature requests, please submit them there.

Contributions are welcome. Follow the usual fork-and-pull request workflow. Before submitting a pull request, make sure

1. Code is linted: run `dotnet format --severity info --verify-no-changes` in the repo root.
2. All existing tests succeed.
3. Any new code includes appropriate tests.
4. Documentation is updated as necessary (and—especially if AI generates the updates—spaces are removed around any em-dashes).


## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

