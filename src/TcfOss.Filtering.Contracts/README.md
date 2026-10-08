# TcfOss.Filtering.Contracts

[Documentation](https://tcfoss.github.io/filtering/) | [Repository](https://github.com/tcfoss/filtering)

Wire-level contract models and constants for filtering, sorting, paging, and data results.

## What this package contains

- Request/response DTOs such as `DataRequest`, `DynamicDataRequest`, and `DataResult<T>`.
- Filter model DTOs such as `SimpleFilter`, `CompositeFilter`, `SetFilter`, `RangeFilter`, and `QuantifiedFilter`.
- Shared operator/type constants such as `FilterOperators`, `LogicalOperators`, and `SortDirections`.
- Filter-tree extensions: `Map`, `Find`, `TryExtract`, and `Merge`.

## Typical usage

Use these types at API boundaries (request body contracts and response contracts).

```csharp
var request = new DataRequest
{
    Filter = new SimpleFilter("name", FilterOperators.Contains, "smith"),
    Sorts = [new SortComponent("name", SortDirections.Ascending)],
    Page = 1,
    PageSize = 20
};
```

## Related packages

- [TcfOss.Filtering.Linq](https://tcfoss.github.io/filtering/packages/linq/)
- [Contract keys](https://tcfoss.github.io/filtering/ContractKeys/)
- [Filter transformations](https://tcfoss.github.io/filtering/FilterTransformations/)
- [Repository README](https://github.com/tcfoss/filtering/blob/master/README.md)
