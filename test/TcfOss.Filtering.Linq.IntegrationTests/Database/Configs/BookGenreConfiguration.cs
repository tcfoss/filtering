using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcfOss.Filtering.Linq.IntegrationTests.Database.Entities;

namespace TcfOss.Filtering.Linq.IntegrationTests.Database.Configs;

public class BookGenreConfiguration : IEntityTypeConfiguration<BookGenre>
{
    public void Configure(EntityTypeBuilder<BookGenre> builder)
    {
        builder.ToTable("book_genre");

        builder.HasKey(bg => new { bg.BookId, bg.GenreId });

        builder.Property(bg => bg.BookId)
            .HasColumnName("book_id")
            .IsRequired();

        builder.Property(bg => bg.GenreId)
            .HasColumnName("genre_id")
            .IsRequired();

        builder.Property(bg => bg.DisplayOrder)
            .HasColumnName("display_order")
            .IsRequired()
            .HasDefaultValue(0u);

        builder.HasOne(bg => bg.Book)
            .WithMany(b => b.BookGenres)
            .HasForeignKey(bg => bg.BookId);

        builder.HasOne(bg => bg.Genre)
            .WithMany(g => g.BookGenres)
            .HasForeignKey(bg => bg.GenreId);
    }
}
