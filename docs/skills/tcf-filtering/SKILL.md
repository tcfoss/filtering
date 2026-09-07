---
name: tcfoss-filtering
description: 'Guide for using the TcfOss.Filtering package family (TcfOss.Filtering.Contracts, TcfOss.Filtering.Linq, TcfOss.Filtering.EntityFrameworkCore, TcfOss.Filtering.CsvOutput, TcfOss.Filtering.XlsxOutput, TcfOss.Filtering.AspNetCore, TcfOss.Filtering.Programmatic, TcfOss.Filtering.Contracts.Newtonsoft, @tcflanagan/filtering-contracts). Use when adding filtering, sorting, paging, or data export to a project; when wiring up DataRequest/DataResult; when writing filter mappers; when using IQueryable extensions; when building frontend requests against a filtering API; when debugging filter deserialization errors.'
---

# TcfOss.Filtering Packages

Filtering, sorting, paging, and export library for .NET and TypeScript.

## Package Selection

| Need | Package |
|---|---|
| Wire-format DTOs only | `TcfOss.Filtering.Contracts` |
| Apply filters to `IQueryable<T>` (no EF) | `TcfOss.Filtering.Linq` |
| Async `ToDataResultAsync`, LIKE support | `TcfOss.Filtering.EntityFrameworkCore` |
| Export to CSV | `TcfOss.Filtering.CsvOutput` |
| Export to XLSX | `TcfOss.Filtering.XlsxOutput` |
| Catch bad filter payloads in ASP.NET Core | `TcfOss.Filtering.AspNetCore` |
| Build filters in C# code (Blazor / WPF) | `TcfOss.Filtering.Programmatic` |
| Newtonsoft.Json filter deserialization | `TcfOss.Filtering.Contracts.Newtonsoft` |
| TypeScript / frontend | `@tcflanagan/filtering-contracts` (npm) |

## Core Flow

```
Frontend  →  API  →  Contracts DTOs  →  Mapper  →  Linq filters  →  IQueryable  →  DataResult
```

## Contracts

`DataRequest` is the primary wire-level request. `DataResult<T>` is always the response:

```csharp
var request = new DataRequest
{
    Filter   = new SimpleFilter("name", FilterOperators.Contains, "smith"),
    Sorts    = [new SortComponent("name", SortDirections.Ascending)],
    Page     = 1,
    PageSize = 20,
};
```

```csharp
// result.Data        — T[] for the current page
// result.TotalCount  — total rows matching the filter
// result.Page / result.PageSize
```

Filter types and all operator/constant wire values: [references/contract-keys.md](./references/contract-keys.md)

## Linq: Applying filters to IQueryable

Import exactly **one** queryable namespace per file to avoid extension ambiguity.

```csharp
using TcfOss.Filtering.Linq.Queryable;

var mapper = new ReflectionFilterMapper<Employee>();
DataRequest request = mapper.ToDataRequest(dto);

DataResult<Employee> result = dbContext.Employees.ToDataResult(request);
```

### Mappers (always use one—never pass raw client field names to Dynamic LINQ)

| Mapper | When to use |
|---|---|
| `ReflectionFilterMapper<T>` | Validates field paths exist on `T` via reflection |
| `DictFilterMapper` | Explicit field → value-parser dictionary |
| `BasicFilterMapper` | Base class; subclass for custom logic |

Access control on `ReflectionFilterMapper<T>`:

```csharp
// Only these paths are accepted
var mapper = new ReflectionFilterMapper<Employee>(whitelist: ["Name", "Department", "HireDate"]);

// Block sensitive paths (* = direct child, ** = any descendant)
var mapper = new ReflectionFilterMapper<Employee>(blacklist: ["UserAccount.**"]);
```

Any disallowed field throws `FilterMappingException`.

## EF Core integration

```csharp
using TcfOss.Filtering.EntityFrameworkCore.Queryable;

DataResult<Employee> result = await dbContext.Employees
    .ToDataResultAsync(request, cancellationToken);

// Dynamic field selection
DataResult<dynamic> result = await dbContext.Employees
    .ToDataResultAsync(dynamicDataRequest, cancellationToken);
```

- Automatically uses EF-aware `CustomParsingConfig`—required for `Like`/`NotLike` operators.
- Do **not** import both `TcfOss.Filtering.Linq.Queryable` and `TcfOss.Filtering.EntityFrameworkCore.Queryable` in the same file.

## Exporting

Full API reference: [references/exporting.md](./references/exporting.md)

```csharp
// CSV, synchronous
Stream csv = dbContext.Employees.ToCsvStream(filter, sorts, maxRows: 10_000);
n
// XLSX, synchronous (mirrors CSV API)
Stream xlsx = dbContext.Employees.ToExcelStream(filter, sorts, maxRows: 10_000);

// Async streaming (large datasets, EF Core)
using TcfOss.Filtering.EntityFrameworkCore.Queryable;

Stream csv = await dbContext.Employees
    .ApplyFiltering(filter, new ValueManager())
    .ApplySorting(sorts)
    .AsAsyncEnumerable()
    .ToCsvStreamAsync(responseStream);
```

## ASP.NET Core error handling

```csharp
app.UseFilterDeserializationExceptionHandler();
// Returns 400 { "error": "..." } instead of 500 on bad filter payloads
```

## Programmatic filters (Blazor / WPF / server-side construction)

Build strongly-typed filter objects in C# code, convert to `Contracts` wire models, then map to Linq. The intermediate step gives you the mapper's validation.

## TypeScript / Frontend

```ts
import { FilterOperators, SortDirections } from '@tcflanagan/filtering-contracts';
import type { DataRequest } from '@tcflanagan/filtering-contracts';

const request: DataRequest = {
  filter: {
    filterType: 'simple',
    field: 'name',
    operator: FilterOperators.Contains,
    value: 'smith',
  },
  sorts: [{ field: 'name', direction: SortDirections.Ascending }],
  page: 1,
  pageSize: 20,
};
```

Optional Zod validation schemas are also exported from the package.

## Security checklist

- Always use a mapper—never expose raw client field names to Dynamic LINQ.
- Use `whitelist`/`blacklist` on `ReflectionFilterMapper` to prevent field enumeration of sensitive properties.
- Register `UseFilterDeserializationExceptionHandler()` to avoid leaking stack traces via 500 responses.
- `Like`/`NotLike` (`lk`/`nlk`) require EF Core paths; they throw `InvalidOperationException` on non-EF paths.

## Common mistakes

| Mistake | Fix |
|---|---|
| Import both Linq and EF queryable namespaces in the same file | Use only one per file |
| Use `lk`/`nlk` without EF Core | Use `TcfOss.Filtering.EntityFrameworkCore.Queryable` |
| Construct Linq filters directly from client input | Use a mapper |
| Expect `FilterDeserializationException` to auto-return 400 | Register `UseFilterDeserializationExceptionHandler()` |
| Newtonsoft.Json without the converter | Register `FilterConverter` in `JsonSerializerSettings` |
