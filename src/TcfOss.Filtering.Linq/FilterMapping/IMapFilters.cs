namespace TcfOss.Filtering.Linq.FilterMapping;

// ReSharper disable UnusedMemberInSuper.Global
public interface IMapFilters
{
    public IFilter ToFilter(Contracts.Filter dto);
    public SimpleFilter ToFilter(Contracts.SimpleFilter dto);
    public CompositeFilter ToFilter(Contracts.CompositeFilter dto);
    public SetFilter ToFilter(Contracts.SetFilter dto);
    public RangeFilter ToFilter(Contracts.RangeFilter dto);
    public QuantifiedFilter ToFilter(Contracts.QuantifiedFilter dto);
    public SortComponent ToSortComponent(Contracts.SortComponent dto);
    public string ToRequestedField(string field);
    public DataRequest ToDataRequest(Contracts.DataRequest dto);
    public DynamicDataRequest ToDataRequest(Contracts.DynamicDataRequest dto);
}
