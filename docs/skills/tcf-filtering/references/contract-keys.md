# Contract Keys Reference

Use the `FilterOperators`, `LogicalOperators`, `SortDirections`, and `QuantifiedOperators` constants from `TcfOss.Filtering.Contracts` (C#) or `@tcflanagan/filtering-contracts` (TypeScript) rather than hardcoding wire strings.

The `filterType` discriminator is required on every filter object on the wire.

## Filter Types

| `filterType` | Class | Description |
|---|---|---|
| `"simple"` | `SimpleFilter` | Single field comparison using a `FilterOperators` constant |
| `"composite"` | `CompositeFilter` | Logical AND / OR combination of other filters |
| `"set"` | `SetFilter` | IN / NOT IN; field value must be one of a set of values |
| `"range"` | `RangeFilter` | Field value falls within (or outside) a lower and upper bound |
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

`Like`/`NotLike` map to SQL `LIKE` via `DbFunctionsExtensions` and require EF Core queryable paths.

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

## Wire format examples

### Simple filter (C#)
```csharp
new SimpleFilter("department.name", FilterOperators.EqualTo, "Engineering")
// wire: { "filterType": "simple", "field": "department.name", "operator": "eq", "value": "Engineering" }
```

### Composite filter (C#)
```csharp
new CompositeFilter(LogicalOperators.And,
    new SimpleFilter("status", FilterOperators.EqualTo, "active"),
    new SimpleFilter("age", FilterOperators.GreaterThan, "30"))
// wire: { "filterType": "composite", "logic": "and", "filters": [...] }
```

### Set filter (C#)
```csharp
new SetFilter("role", ["admin", "manager"])
// wire: { "filterType": "set", "field": "role", "values": ["admin", "manager"] }
```

### Range filter (C#)
```csharp
new RangeFilter("salary", "50000", "100000")
// wire: { "filterType": "range", "field": "salary", "from": "50000", "to": "100000" }
```

### TypeScript
```ts
// Composite AND of two simple filters
const filter: Filter = {
  filterType: 'composite',
  logic: LogicalOperators.And,
  filters: [
    { filterType: 'simple', field: 'status', operator: FilterOperators.EqualTo, value: 'active' },
    { filterType: 'simple', field: 'age',    operator: FilterOperators.GreaterThan, value: '30' },
  ],
};
```
