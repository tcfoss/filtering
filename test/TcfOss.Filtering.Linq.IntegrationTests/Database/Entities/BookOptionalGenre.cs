using System.Diagnostics;

namespace TcfOss.Filtering.Linq.IntegrationTests.Database.Entities;

[DebuggerDisplay("BookOptionalGenre(BookId={BookId}, GenreId={GenreId})")]
public class BookOptionalGenre
{
    public uint BookId { get; init; }
    public uint? GenreId { get; init; }
    public uint DisplayOrder { get; init; }

    public Book Book { get; init; } = null!;
    public Genre Genre { get; init; } = null!;
    public FullGenre FullGenre { get; init; } = null!;
}
