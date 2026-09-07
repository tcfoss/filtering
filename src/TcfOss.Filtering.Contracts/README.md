# TcfOss.Filtering.Contracts

Wire-level contract models and constants for filtering, sorting, paging, and data results.

## What this package contains

- Request/response DTOs such as `DataRequest`, `DynamicDataRequest`, and `DataResult<T>`.
- Filter model DTOs such as `SimpleFilter`, `CompositeFilter`, `SetFilter`, `RangeFilter`, and `QuantifiedFilter`.
- Shared operator/type constants such as `FilterOperators`, `LogicalOperators`, and `SortDirections`.

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

- [../TcfOss.Filtering.Linq/README.md](../TcfOss.Filtering.Linq/README.md)
- [../../docs/ContractKeys.md](../../docs/ContractKeys.md)
- [../../README.md](../../README.md)
