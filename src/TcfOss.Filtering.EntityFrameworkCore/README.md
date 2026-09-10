# TcfOss.Filtering.EntityFrameworkCore

EF Core integration helpers for `TcfOss.Filtering.Linq`.

## What this package adds

- Async result helpers (`ToDataResultAsync`) for EF Core queries.
- EF-aware parsing for Dynamic LINQ operations that rely on EF symbols (e.g. `LIKE` filters).
- Canonical query extension methods in the `TcfOss.Filtering.EntityFrameworkCore.Queryable` namespace (for example, `ApplyFiltering`, `ApplyDataFilter`, `ToDataResult`) that automatically use EF parsing config.

## Typical usage

```csharp
using TcfOss.Filtering.EntityFrameworkCore.Queryable;

Contracts.DataResult<Employee> result = await dbContext.Employees
    .ToDataResultAsync(request, cancellationToken);
```

## Notes

- For explicit LIKE operators (`lk` / `nlk`), use EF-aware paths so Dynamic LINQ has access to EF symbols.
- Prefer importing only one queryable namespace per file to avoid extension-method ambiguity.

## Related packages

- [TcfOss.Filtering.Linq](https://github.com/tcfoss/filtering/blob/master/src/TcfOss.Filtering.Linq/README.md)
- [Utility extension packages](https://github.com/tcfoss/filtering/blob/master/docs/UtilityExtensionPackages.md)
- [Repository README](https://github.com/tcfoss/filtering/blob/master/README.md)
