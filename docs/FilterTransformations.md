---
description: Transform, search, extract, and merge contract filter trees with Map, Find, TryExtract, and Merge.
---

# Filter Transformation Extensions

`FilterExtensions` in `TcfOss.Filtering.Contracts` provides four helpers for working
with contract filter trees before mapping them to LINQ requests:

| Method | Purpose | Result |
|---|---|---|
| `Map` | Replace or remove nodes recursively | The transformed tree, or `null` |
| `Find` | Locate the first node matching a predicate | The matching node, or `null` |
| `TryExtract<T>` | Extract one value and remove its corresponding node | The remaining tree, plus an `out T?` value |
| `Merge` | Combine two filters with a logical operator | The combined tree, or `null` |

Import the contract namespace for all examples on this page:

```csharp
using TcfOss.Filtering.Contracts;
```

These helpers operate on contract DTOs, not `TcfOss.Filtering.Linq` filter models.
They do not validate field names, parse values, or execute queries. Continue to use
[request mapping](RequestMapping.md) before applying a client request to a query.

## Map: Transform or Remove Nodes

```csharp
Filter? Map(this Filter? filter, Func<Filter, Filter?> transform)
```

Return a replacement filter from the callback to change a node, the node itself to
keep it, or `null` to remove it. For example, remove a UI-only condition:

```csharp
Filter filter = new CompositeFilter(LogicalOperators.And,
[
    new SimpleFilter("Name", FilterOperators.Contains, "smith"),
    new SimpleFilter("PreviewOnly", FilterOperators.EqualTo, "true"),
]);

Filter? cleaned = filter.Map(node =>
    node is SimpleFilter { Field: "PreviewOnly" } ? null : node);

// cleaned is the Name filter, not a one-child CompositeFilter.
```

Callbacks can also replace a node with a different filter:

```csharp
Filter? rewritten = filter.Map(node =>
    node is SimpleFilter { Field: "PreviewOnly" }
        ? new SimpleFilter("Status", FilterOperators.EqualTo, "Published")
        : node);
```

### Traversal and Simplification

`Map` visits children in array order before visiting their parent:

- A `null` input returns `null` without invoking the callback.
- For a `CompositeFilter`, it maps the children and removes `null` results.
  With no children remaining it returns `null`; with one child it returns that
  child directly. In both cases, the callback is **not called on the composite**.
  With two or more children, the callback receives a copy of the composite with
  the transformed children.
- For a `QuantifiedFilter`, it maps `SubFilter` first. If that becomes `null`, the
  whole quantified filter is removed without invoking its callback. Otherwise,
  the callback receives a copy with the transformed subfilter.
- Other filter types are passed directly to the callback. `SetFilter` and
  `RangeFilter` are leaves; their values are not separate filter nodes.

Replacement trees returned by the callback are not traversed again or simplified
afterward. Even an identity mapping (`node => node`) can collapse existing empty
or one-child composites.

## Find: Locate a Node

```csharp
Filter? Find(this Filter? filter, Func<Filter, bool> predicate)
```

`Find` checks the current node first, then searches composite children in array
order or a quantified filter's subfilter. It stops at the first match and does
not transform or simplify the tree. A `null` input or no match returns `null`.

```csharp
Filter filter = new CompositeFilter(LogicalOperators.And,
[
    new SimpleFilter("Name", FilterOperators.Contains, "smith"),
    new QuantifiedFilter
    {
        Field = "Tags",
        Operator = QuantifiedOperators.Any,
        SubFilter = new SimpleFilter("Value", FilterOperators.EqualTo, "featured"),
    },
]);

Filter? found = filter.Find(node =>
    node is SimpleFilter { Field: "Value" });

// found is the Value filter inside the Tags quantified filter.
```

Unlike `Map`, `Find` searches parent-before-child. A predicate matching the root
composite returns that composite immediately rather than searching its children.

## TryExtract: Remove One Node and Return a Value

```csharp
Filter? TryExtract<T>(
    this Filter? filter,
    Func<Filter, T?> extractor,
    out T? extracted) where T : class
```

The extractor returns a non-null reference value when a node should be extracted,
or `null` when it should remain. The value can be the matching filter itself or
another reference type such as a string or an application-specific object.

```csharp
Filter filter = new CompositeFilter(LogicalOperators.And,
[
    new SimpleFilter("SearchTerm", FilterOperators.Contains, "smith"),
    new SimpleFilter("IsActive", FilterOperators.EqualTo, "true"),
]);

Filter? remaining = filter.TryExtract<SimpleFilter>(
    node => node is SimpleFilter { Field: "SearchTerm" } simple ? simple : null,
    out SimpleFilter? searchTerm);

// searchTerm is the SearchTerm filter; handle it separately.
// remaining is the IsActive filter after its composite is collapsed.
```

`TryExtract` uses `Map`, so it follows the same child-before-parent traversal and
simplification rules. Only the first non-null extracted value is retained; its
node is removed, and the extractor is not called again after that match. A parent
visited afterward sees the already-transformed children. Collapsed or removed
composites are never offered to the extractor.

If no value is extracted, `extracted` is `null`, but the returned tree can still
be simplified by `Map`. For a `null` input, both outputs are `null`.
Despite its name, the method returns the remaining filter, **not a success boolean**;
check `extracted` for a match. `T` must be a reference type, not a value type such as `int`.


## Merge: Combine Filters

```csharp
Filter? Merge(
    this Filter? filter,
    Filter? other,
    string op = LogicalOperators.And)
```

By default, `Merge` combines two filters with `AND`. Supply `LogicalOperators.Or`
to combine alternatives instead:

```csharp
Filter name = new SimpleFilter("Name", FilterOperators.Contains, "smith");
Filter status = new SimpleFilter("Status", FilterOperators.EqualTo, "Published");

Filter? both = name.Merge(status);
Filter? either = name.Merge(status, LogicalOperators.Or);
```

- If either argument is `null`, the other is returned unchanged. Two `null`
  arguments return `null`.
- When an argument is a top-level composite using the requested operator, its
  children are included directly in the result. If both are matching composites,
  their child arrays are concatenated, left before right.
- Composites using a different operator stay nested to preserve their grouping.
- Flattening is only at the arguments' top level, not recursive. There is no
  deduplication or general logical simplification, and `op` is not validated.
  Use the [logical operator constants](ContractKeys.md#logical-operators).

For example, merging `(A OR B)` with `C` using the default operator produces
`(A OR B) AND C`, not `A AND B AND C`.
