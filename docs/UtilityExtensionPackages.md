# Utility Extension Packages

## TcfOss.Filtering.AspNetCore

Provides middleware that catches `FilterDeserializationException`—thrown when a request body
cannot be deserialized—and returns a `400 Bad Request` response. This is useful because ASP.NET
attempts to deserialize the request before invoking the controller, and System.Text.Json's
deserialization exceptions, without manual fiddling, would result in a `500 Internal Server Error`.

```csharp
app.UseFilterDeserializationExceptionHandler();
```

The response body is `{ "error": "<message>" }`. This package has no dependency on Newtonsoft.Json.


## TcfOss.Filtering.EntityFrameworkCore

Provides async counterparts to `ToDataResult` that delegate to EF Core's `CountAsync` and `ToListAsync`, keeping the core `TcfOss.Filtering.Linq` package free of EF Core dependencies.

```csharp
using TcfOss.Filtering.EntityFrameworkCore.Queryable;

// Typed result
DataResult<Employee> result = await dbContext.Employees
    .ToDataResultAsync(dataRequest, cancellationToken);

// Dynamic field selection
DataResult<dynamic> result = await dbContext.Employees
    .ToDataResultAsync(dynamicDataRequest, cancellationToken);
```

Both overloads also accept an explicit `IManageValues` as a third argument. Requires EF Core 9 or 10.

When composing queries, import only one queryable namespace per file:

- `TcfOss.Filtering.EntityFrameworkCore.Queryable` for EF-aware parsing defaults.
- `TcfOss.Filtering.Linq.Queryable` if `Microsoft.EntityFrameworkCore` is not available.



## TcfOss.Filtering.Contracts.Newtonsoft

Provides a Newtonsoft.Json `JsonConverter` for polymorphic deserialisation of `Filter`. Use this package when your project uses **Newtonsoft.Json** instead of (or alongside) `System.Text.Json`.

> **Note:** `Filter` already supports `System.Text.Json`.

### Registering the converter

For one-off deserialisation, the converter can be registered like so:

```csharp
var settings = new JsonSerializerSettings
{
    Converters = { new FilterConverter() },
};

DataRequest? dto = JsonConvert.DeserializeObject<DataRequest>(json, settings);
```

If Newtonsoft.Json is used as the deserializer for ASP.NET Core, register
the converter globally:

```csharp
builder.Services
    .AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.Converters.Add(new FilterConverter());
    });
```

The converter is read-only (`CanWrite = false`); serialisation falls through to Newtonsoft's default behavior.