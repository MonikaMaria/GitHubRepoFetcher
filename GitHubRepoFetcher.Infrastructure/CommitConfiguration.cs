
using GitHubRepoFetcher.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GitHubRepoFetcher.Infrastructure;

public sealed class CommitConfiguration : IEntityTypeConfiguration<Commit>
{
    public void Configure(EntityTypeBuilder<Commit> builder)
    {
        builder.ToTable("Commits");

        builder.HasKey(p => p.Id);

        builder
            .HasIndex(p => new { p.UserName, p.RepositoryName, p.Sha })
            .IsUnique();

        builder
            .Property(p => p.Id)
            .ValueGeneratedNever();

        builder
           .Property(p => p.Sha)
           .HasMaxLength(FieldLengths.Commit.Sha);

        builder
            .Property(p => p.CommitterName)
            .HasMaxLength(FieldLengths.Commit.CommitterName);

        builder
            .Property(p => p.CommitterEmail)
            .HasMaxLength(FieldLengths.Commit.CommitterEmail);

    }
}
