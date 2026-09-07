using System.Text.Json.Serialization;
using Demo.Api;
using TcfOss.Filtering.Linq;
using TcfOss.Filtering.Linq.FilterMapping;
using TcfOss.Filtering.Linq.Queryable;

using Contracts = TcfOss.Filtering.Contracts;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:5176").AllowAnyHeader().AllowAnyMethod()));

WebApplication app = builder.Build();
app.UseCors();

// ---------------------------------------------------------------------------
// Data
// ---------------------------------------------------------------------------

Employee[] employees = EmployeeData.All;

// ---------------------------------------------------------------------------
// Endpoints
// ---------------------------------------------------------------------------

// POST /employees  — filtered, sorted, paged — returns DataResult<Employee>
// Whitelist uses Department.* (direct children only: Department.Name allowed,
// Department.Office.City blocked — demonstrating the .* wildcard depth limit).
app.MapPost("/employees", (Contracts.DataRequest dto) =>
{
    try
    {
        var mapper = new ReflectionFilterMapper<Employee>(
            whitelist: ["Name", "Department.*", "JobTitle", "Salary", "HireDate", "IsActive"]);

        DataRequest request = mapper.ToDataRequest(dto);
        return Results.Ok(employees.AsQueryable().ToDataResult(request));
    }
    catch (Contracts.FilterMappingException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// POST /employees/dynamic  — returns only requested fields as DataResult<dynamic>
// Whitelist uses Department.** (all descendants: Department.Name AND
// Department.Office.City both allowed — demonstrating the .** recursive wildcard).
app.MapPost("/employees/dynamic", (Contracts.DynamicDataRequest dto) =>
{
    try
    {
        var mapper = new ReflectionFilterMapper<Employee>(
            whitelist: ["Name", "Department.**", "JobTitle", "Salary", "HireDate", "IsActive"],
            blacklist: ["Department.Office.SecretId"]); // blacklist takes precedence over whitelist

        DynamicDataRequest request = mapper.ToDataRequest(dto);
        return Results.Ok(employees.AsQueryable().ToDataResult(request));
    }
    catch (Contracts.FilterMappingException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.Run();
