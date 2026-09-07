# Contract Keys

Under normal circumstances, you should use the provided enumerations to construct filters and
sorts. However, if for some reason you do not want to import the `Contracts` or
`tcfoss-filtering-contracts` package, you can construct the filter and sort objects yourself.
The following enumerations and their corresponding string values are understood by the mappers:

Filter objects use the `filterType` discriminator property on the wire.


## Filter Types

| Wire type | Class | Description |
|---|---|---|
| `"simple"` | `SimpleFilter` | Single field comparison using a `FilterOperators` constant |
| `"composite"` | `CompositeFilter` | Logical AND / OR combination of other filters |
| `"set"` | `SetFilter` | IN / NOT IN;  field value must be (or must not be) one of a set of values |
| `"range"` | `RangeFilter` | Range check; field value falls within (or outside) a lower and upper bound |
| `"quantified"` | `QuantifiedFilter` | ANY / ALL check on a collection navigation property |


## Simple Filter Operators

| Constant | Wire value |
|---|---|
| `FilterOperators.EqualTo` | `"eq"` |
| `FilterOperators.NotEqualTo` | `"neq"` |
| `FilterOperators.IsNull` | `"nu"` |
| `FilterOperators.IsNotNull` | `"nnu"` |
| `FilterOperators.GreaterThan` | `"gt"` |
| `FilterOperators.GreaterThanOrEqualTo` | `"gte"` |
| `FilterOperators.LessThan` | `"lt"` |
| `FilterOperators.LessThanOrEqualTo` | `"lte"` |
| `FilterOperators.StartsWith` | `"sw"` |
| `FilterOperators.DoesNotStartWith` | `"nsw"` |
| `FilterOperators.EndsWith` | `"ew"` |
| `FilterOperators.DoesNotEndWith` | `"new"` |
| `FilterOperators.Contains` | `"cn"` |
| `FilterOperators.DoesNotContain` | `"ncn"` |
| `FilterOperators.Like` | `"lk"` |
| `FilterOperators.NotLike` | `"nlk"` |


## Logical Operators

| Constant | Wire value |
|---|---|
| `LogicalOperators.And` | `"and"` |
| `LogicalOperators.Or` | `"or"` |


## Quantified Operators

| Constant | Wire value |
|---|---|
| `QuantifiedOperators.Any` | `"any"` |
| `QuantifiedOperators.All` | `"all"` |


## Sort Directions

| Constant | Wire value |
|---|---|
| `SortDirections.Ascending` | `"asc"` |
| `SortDirections.Descending` | `"desc"` |

