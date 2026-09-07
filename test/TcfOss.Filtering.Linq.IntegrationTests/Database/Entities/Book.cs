using System.Diagnostics;

namespace TcfOss.Filtering.Linq.IntegrationTests.Database.Entities;

[DebuggerDisplay("Book({SortableTitle})")]
public class Book
{
    public uint BookId { get; init; }
    public string Title { get; init; } = null!;
    public string? Subtitle { get; init; }
    public decimal? PurchasePrice { get; init; }
    public DateOnly? PurchaseDate { get; init; }
    public bool IsLoaned { get; init; }
    public string? Notes { get; init; }
    public string SortableTitle { get; init; } = null!;

    public ICollection<Author> Authors { get; init; } = [];
    public ICollection<BookGenre> BookGenres { get; init; } = [];
    public ICollection<BookOptionalGenre> BookOptionalGenres { get; init; } = [];
    public ICollection<Genre> Genres { get; init; } = [];
    public ICollection<FullGenre> FullGenres { get; init; } = [];
}
