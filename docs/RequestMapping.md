# Mapping `Contracts` to `Linq` Requests

To convert `Contracts` objects to `Linq` requests, you can use an `IMapFilters` implementation,
a few of which are provided in the `TcfOss.Filtering.Linq` package.


## The `ReflectionFilterMapper<T>`

The `ReflectionFilterMapper<T>` provides the simplest approach to mapping `Contracts` objects
to `Linq` requests. It uses reflection to resolve field paths against the properties of `T`
and to parse value strings to the correct CLR types.

```csharp
var mapper = new ReflectionFilterMapper<Employee>();

var dto = new Contracts.DataRequest
{
    Filter = new Contracts.SimpleFilter("Department.Name", Contracts.FilterOperators.EqualTo, "Engineering"),
    Sorts = [new Contracts.SortComponent("Name", Contracts.SortDirections.Ascending)],
    Page = 1,
    PageSize = 25,
};

var linqRequest = mapper.ToDataRequest(dto);
```

The linq request can then be applied to an `IQueryable<Employee>` to get a `DataResult<Employee>`:

```csharp
using TcfOss.Filtering.Linq.Queryable;

IQueryable<Employee> queryable = ...;
DataResult<Employee> result = queryable.ToDataResult(linqRequest);
// result.Data        — Employee[] for the current page
// result.TotalCount  — total rows matching the filter
// result.Page        — current page
// result.PageSize    — page size
```

### Access control via whitelist / blacklist

`ReflectionFilterMapper<T>` supports optional whitelist and blacklist patterns to control which
field paths are valid. Patterns are dot-separated property names.

```csharp
// Only allow filtering on these fields
var mapper = new ReflectionFilterMapper<Employee>(whitelist: ["Name", "Department", "HireDate"]);

// Or block specific fields
var mapper = new ReflectionFilterMapper<Employee>(
    blacklist: ["UserAccount.PasswordHash", "SocialSecurityNumber"]);
```

Any field not permitted throws `FilterMappingException` to prevent leaking field existence.

Wildcards are supported in whitelist and blacklist patterns:

- `*` matches a direct child property (e.g. `Department.*` matches `Department.Name` but not
  `Department.Location.City`)
- `**` matches any descendant property (e.g. `Department.**` matches both `Department.Name` and
  `Department.Location.City`)


## More Customization: The `DictFilterMapper`

Use this when you want complete control over field mapping and value parsing. This is useful when
you need explicit value parsers instead of reflection (e.g., projections, computed columns):

```csharp
var mapper = new DictFilterMapper(new Dictionary<string, Func<string, object>>
{
    ["Name"] = s => s,
    ["Age"] = s => int.Parse(s),
    ["Salary"] = s => decimal.Parse(s, CultureInfo.InvariantCulture),
});
```

## Custom Mappers

Implement `IMapFilters` when you need to map to a custom request type or have complex mapping logic
that doesn't fit the above mappers. This is a more involved task but gives you full control
over the mapping process.

You could also extend and override functions in `BasicFilterMapper` to customize only
parts of the mapping.
