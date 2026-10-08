# TcfOss.Filtering

Filtering, sorting, paging, and CSV/XLSX export libraries for .NET and TypeScript.
Define requests with shared contracts, validate client field names with a mapper,
and apply requests to `IQueryable<T>` queries.

## Choose a Package

| Need | Package |
|---|---|
| Wire-format DTOs and constants | [TcfOss.Filtering.Contracts](packages/contracts.md) |
| Filtering, sorting, paging, and field selection | [TcfOss.Filtering.Linq](packages/linq.md) |
| EF Core async queries and EF-aware parsing | [TcfOss.Filtering.EntityFrameworkCore](packages/entity-framework-core.md) |
| CSV export | [TcfOss.Filtering.CsvOutput](packages/csv-output.md) |
| Excel export | [TcfOss.Filtering.XlsxOutput](packages/xlsx-output.md) |
| Newtonsoft.Json filter deserialization | [TcfOss.Filtering.Contracts.Newtonsoft](packages/contracts-newtonsoft.md) |
| ASP.NET Core malformed-filter handling | [TcfOss.Filtering.AspNetCore](packages/aspnetcore.md) |
| Strongly typed filters constructed in C# | [TcfOss.Filtering.Programmatic](packages/programmatic.md) |
| Frontend contracts and optional Zod schemas | [@tcfoss/filtering-contracts](packages/typescript-contracts.md) |

## Getting Started

Install the contracts and query packages for a .NET application:

```bash
dotnet add package TcfOss.Filtering.Contracts
dotnet add package TcfOss.Filtering.Linq
```

Construct a request with contract constants:

```csharp
using TcfOss.Filtering.Contracts;

var request = new DataRequest
{
    Filter = new SimpleFilter("Name", FilterOperators.Contains, "smith"),
    Sorts = [new SortComponent("Name", SortDirections.Ascending)],
    Page = 1,
    PageSize = 20,
};
```

Before applying a client request to a query, use a mapper to validate field paths
and parse values. See [request mapping](RequestMapping.md), including field
whitelists and blacklists, and the [LINQ usage examples](packages/linq.md).

For a TypeScript frontend:

```bash
npm install @tcfoss/filtering-contracts
```

```typescript
import { FilterOperators, SortDirections } from '@tcfoss/filtering-contracts';
import type { DataRequest } from '@tcfoss/filtering-contracts';

const request: DataRequest = {
    filter: {
        filterType: 'simple',
        field: 'Name',
        operator: FilterOperators.Contains,
        value: 'smith',
    },
    sorts: [{ field: 'Name', direction: SortDirections.Ascending }],
    page: 1,
    pageSize: 20,
};
```

## How It Fits Together

```text
Frontend -> API -> Contracts DTOs -> Mapper -> LINQ filters -> IQueryable -> DataResult
```

The contracts define the wire format; LINQ applies validated requests to queries.
Optional packages add EF Core integration, export writers, middleware, and JSON converters.

## Guides and References

- [Contract keys and wire values](ContractKeys.md)
- [Transforming, searching, extracting, and merging filters](FilterTransformations.md)
- [Request mapping and field access control](RequestMapping.md)
- [Exporting filtered data](ExportingData.md)
- [Framework and serializer integrations](UtilityExtensionPackages.md)
- [AI coding skill and reference material](skills/tcfoss-filtering/SKILL.md)
- [Contributing and development](development/index.md)
- [Documentation maintenance](development/documentation.md)
- [Release workflow](development/ReleaseWorkflow.md)

## Source and Contributions

Browse the [repository](https://github.com/tcfoss/filtering) and
[runnable demo](https://github.com/tcfoss/filtering/tree/master/demo).
Report bugs and feature requests on [IssueTracker](https://issues.tcflanagan.net/filtering).
Contribution guidance is in the [repository README](https://github.com/tcfoss/filtering/blob/master/README.md).
The project is licensed under the [MIT License](https://github.com/tcfoss/filtering/blob/master/LICENSE).
