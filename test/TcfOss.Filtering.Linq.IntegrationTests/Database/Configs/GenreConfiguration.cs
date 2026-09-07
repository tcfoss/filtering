using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcfOss.Filtering.Linq.IntegrationTests.Database.Entities;

namespace TcfOss.Filtering.Linq.IntegrationTests.Database.Configs;

public class GenreConfiguration : IEntityTypeConfiguration<Genre>
{
    public void Configure(EntityTypeBuilder<Genre> builder)
    {
        builder.ToTable("genre");

        builder.HasKey(g => g.GenreId);

        builder.Property(g => g.GenreId)
            .HasColumnName("genre_id")
            .IsRequired();

        builder.Property(g => g.ParentId)
            .HasColumnName("parent_id");

        builder.Property(g => g.Name)
            .HasColumnName("genre")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasOne(g => g.Parent)
            .WithMany(g => g.Children)
            .HasForeignKey(g => g.ParentId);
    }
}
