
using GitHubRepoFetcher.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;

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
            .ValueGeneratedOnAdd()
            .HasValueGenerator<SequentialGuidValueGenerator>();

        builder
           .Property(p => p.Sha)
           .HasMaxLength(FieldLengths.Commit.Sha);

        builder
            .Property(p => p.Message)
            .HasMaxLength(FieldLengths.Commit.Message);

        builder
            .Property(p => p.Committer)
            .HasMaxLength(FieldLengths.Commit.Committer);

    }
}
