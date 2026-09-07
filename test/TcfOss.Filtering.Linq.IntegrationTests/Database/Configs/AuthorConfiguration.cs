using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcfOss.Filtering.Linq.IntegrationTests.Database.Entities;

namespace TcfOss.Filtering.Linq.IntegrationTests.Database.Configs;

public class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.ToTable("author");

        builder.HasKey(a => a.AuthorId);

        builder.Property(a => a.AuthorId)
            .HasColumnName("author_id")
            .IsRequired();

        builder.Property(a => a.BookId)
            .HasColumnName("book_id")
            .IsRequired();

        builder.Property(a => a.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.DisplayOrder)
            .HasColumnName("display_order")
            .IsRequired()
            .HasDefaultValue(0u);

        builder.HasOne(a => a.Book)
            .WithMany(b => b.Authors)
            .HasForeignKey(a => a.BookId);
    }
}
