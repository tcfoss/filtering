namespace TcfOss.Filtering.Linq;

public interface IFilter
{
    /// <summary>
    /// Convert the filter to a Dynamic LINQ string.
    /// </summary>
    /// <param name="valueManager"></param>
    /// <returns></returns>
    public string ToDynamicLinq(IManageValues valueManager);
}
