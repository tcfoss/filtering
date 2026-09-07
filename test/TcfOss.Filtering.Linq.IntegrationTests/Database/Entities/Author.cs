using System.Diagnostics;

namespace TcfOss.Filtering.Linq.IntegrationTests.Database.Entities;

[DebuggerDisplay("Author({Name})")]
public class Author
{
    public uint AuthorId { get; init; }
    public uint BookId { get; init; }
    public string Name { get; init; } = null!;
    public uint DisplayOrder { get; init; }

    public Book Book { get; init; } = null!;
}
