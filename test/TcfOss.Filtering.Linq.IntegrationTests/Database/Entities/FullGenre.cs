using System.Diagnostics;

namespace TcfOss.Filtering.Linq.IntegrationTests.Database.Entities;

[DebuggerDisplay("FullGenre({Name})")]
public class FullGenre
{
    public uint GenreId { get; init; }
    public string Name { get; init; } = null!;

    public ICollection<Book> Books { get; init; } = [];
    public ICollection<BookGenre> BookGenres { get; init; } = [];
}
