using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcfOss.Filtering.Linq.IntegrationTests.Database.Entities;

namespace TcfOss.Filtering.Linq.IntegrationTests.Database.Configs;

public class BookOptionalGenreConfiguration : IEntityTypeConfiguration<BookOptionalGenre>
{
    public void Configure(EntityTypeBuilder<BookOptionalGenre> builder)
    {
        builder.ToTable("book_optional_genre");

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
            .WithMany(b => b.BookOptionalGenres)
            .HasForeignKey(bg => bg.BookId);

        builder.HasOne(bg => bg.Genre)
            .WithMany(g => g.BookOptionalGenres)
            .HasForeignKey(bg => bg.GenreId);

        builder.HasOne(bg => bg.FullGenre)
            .WithMany()
            .HasForeignKey(bg => bg.GenreId)
            .HasPrincipalKey(g => g.GenreId);
    }
}
