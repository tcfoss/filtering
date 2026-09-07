using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcfOss.Filtering.Linq.IntegrationTests.Database.Entities;

namespace TcfOss.Filtering.Linq.IntegrationTests.Database.Configs;

public class FullGenreConfiguration : IEntityTypeConfiguration<FullGenre>
{
    public void Configure(EntityTypeBuilder<FullGenre> builder)
    {
        builder.ToView("full_genre");

        builder.HasKey(g => g.GenreId);

        builder.Property(g => g.GenreId)
            .HasColumnName("genre_id")
            .IsRequired();

        builder.Property(g => g.Name)
            .HasColumnName("genre")
            .HasMaxLength(1000)
            .IsRequired();
    }
}
