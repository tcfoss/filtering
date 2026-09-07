using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcfOss.Filtering.Linq.IntegrationTests.Database.Entities;

namespace TcfOss.Filtering.Linq.IntegrationTests.Database.Configs;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("book");

        builder.HasKey(b => b.BookId);

        builder.Property(b => b.BookId)
            .HasColumnName("book_id")
            .IsRequired();

        builder.Property(b => b.Title)
            .HasColumnName("title")
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(b => b.Subtitle)
            .HasColumnName("subtitle")
            .HasMaxLength(250);

        builder.Property(b => b.PurchasePrice)
            .HasColumnName("purchase_price")
            .HasColumnType("decimal(10,2)");

        builder.Property(b => b.PurchaseDate)
            .HasColumnName("purchase_date");

        builder.Property(b => b.IsLoaned)
            .HasColumnName("is_loaned")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(b => b.Notes)
            .HasColumnName("notes")
            .HasColumnType("text");

        builder.Property(b => b.SortableTitle)
            .HasColumnName("sortable_title")
            .HasMaxLength(550)
            .ValueGeneratedOnAddOrUpdate();

        builder.HasMany(b => b.Authors)
            .WithOne(a => a.Book)
            .HasForeignKey(a => a.BookId);

        builder.HasMany(b => b.BookGenres)
            .WithOne(bg => bg.Book)
            .HasForeignKey(bg => bg.BookId);

        builder.HasMany(b => b.Genres)
            .WithMany(g => g.Books)
            .UsingEntity<BookGenre>(
                j => j
                    .HasOne(bg => bg.Genre)
                    .WithMany(g => g.BookGenres)
                    .HasForeignKey(bg => bg.GenreId),
                j => j
                    .HasOne(bg => bg.Book)
                    .WithMany(b => b.BookGenres)
                    .HasForeignKey(bg => bg.BookId));

        builder.HasMany(b => b.FullGenres)
            .WithMany(g => g.Books)
            .UsingEntity<BookGenre>(
                j => j
                    .HasOne(bg => bg.FullGenre)
                    .WithMany(g => g.BookGenres)
                    .HasForeignKey(bg => bg.GenreId),
                j => j
                    .HasOne(bg => bg.Book)
                    .WithMany(b => b.BookGenres)
                    .HasForeignKey(bg => bg.BookId));
    }
}
