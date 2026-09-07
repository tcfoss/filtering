# TcfOss.Filtering.Linq

Dynamic LINQ query composition for filtering, sorting, paging, and dynamic field selection.

## What this package does

- Maps contract filter DTOs into LINQ-safe request models.
- Applies filters (`ApplyFiltering`), sorting (`ApplySorting`), and paging (`ApplyPagingAndSorting`) to `IQueryable<T>`.
- Produces paged results via `ToDataResult`.
- Supports dynamic projection via `DynamicDataRequest` and `ApplyFieldSelection`.

## Typical usage

```csharp
using TcfOss.Filtering.Linq.Queryable;

var mapper = new ReflectionFilterMapper<Employee>();
DataRequest request = mapper.ToDataRequest(dto);

DataResult<Employee> result = dbContext.Employees.ToDataResult(request);
```

## Security note

When handling untrusted input, map via a mapper (`ReflectionFilterMapper<T>`, `DictFilterMapper`, `BasicFilterMapper`) rather than constructing filters directly from raw, client-supplied field names.

## Related docs

- [../../docs/RequestMapping.md](../../docs/RequestMapping.md)
- [../../docs/ContractKeys.md](../../docs/ContractKeys.md)
- [../TcfOss.Filtering.EntityFrameworkCore/README.md](../TcfOss.Filtering.EntityFrameworkCore/README.md)
- [../../README.md](../../README.md)
