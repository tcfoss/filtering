using BenchmarkDotNet.Attributes;
using TcfOss.Filtering.EntityFrameworkCore.Queryable;
using TcfOss.Filtering.Linq;
using TcfOss.Filtering.Linq.Queryable;

namespace TcfOss.Filtering.Benchmarks;

[MemoryDiagnoser]
public class QueryableHotPathBenchmarks
{
    [Params(1000, 10000)]
    public int RowCount { get; set; }

    private IQueryable<Employee> _query = null!;
    private DataRequest _request = null!;
    private DynamicDataRequest _dynamicRequest = null!;

    [GlobalSetup]
    public void Setup()
    {
        List<Employee> employees = [.. Enumerable.Range(1, RowCount)
            .Select(i => new Employee(
                Id: i,
                Name: $"Employee {i}",
                Age: 20 + (i % 45),
                Salary: 35_000 + ((i * 137) % 90_000),
                Department: new Department($"Department {(i % 12)}")))];

        _query = employees.AsQueryable();

        _request = new DataRequest
        {
            Filter = new SimpleFilter("Age", QueryOperator.GreaterThanOrEqualTo, 35),
            Sorts = [new SortComponent("Salary", SortDirection.Descending), new SortComponent("Name", SortDirection.Ascending)],
            Page = 2,
            PageSize = 100
        };

        _dynamicRequest = new DynamicDataRequest
        {
            Filter = _request.Filter,
            Sorts = _request.Sorts,
            Page = _request.Page,
            PageSize = _request.PageSize,
            RequestedFields = ["Name", "Department.Name", "Salary"]
        };
    }

    [Benchmark(Baseline = true)]
    public int Linq_ApplyDataFilter_Count()
    {
        IQueryable<Employee> filtered = LinqQueryableExtensions.ApplyDataFilter(_query, _request, new ValueManager());
        return filtered.Count();
    }

    [Benchmark]
    public int Ef_ApplyDataFilter_Count()
    {
        IQueryable<Employee> filtered = EfQueryableExtensions.ApplyDataFilter(_query, _request, new ValueManager());
        return filtered.Count();
    }

    [Benchmark]
    public int Linq_ToDataResult_Dynamic_Count()
    {
        Contracts.DataResult<dynamic> result = LinqQueryableExtensions.ToDataResult(_query, _dynamicRequest, new ValueManager());
        return result.Data.Length;
    }

    [Benchmark]
    public int Ef_ToDataResult_Dynamic_Count()
    {
        Contracts.DataResult<dynamic> result = EfQueryableExtensions.ToDataResult(_query, _dynamicRequest, new ValueManager());
        return result.Data.Length;
    }

    private sealed record Department(string Name);

    private sealed record Employee(int Id, string Name, int Age, decimal Salary, Department Department);
}
