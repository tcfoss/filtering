using System.Diagnostics;

namespace TcfOss.Filtering.Linq.IntegrationTests.Database.Entities;

[DebuggerDisplay("Genre({Name})")]
public class Genre
{
    public uint GenreId { get; init; }
    public uint? ParentId { get; init; }
    public string Name { get; init; } = null!;

    public Genre? Parent { get; init; }
    public ICollection<Genre> Children { get; init; } = [];
    public ICollection<BookGenre> BookGenres { get; init; } = [];
    public ICollection<BookOptionalGenre> BookOptionalGenres { get; init; } = [];
    public ICollection<Book> Books { get; init; } = [];
}
