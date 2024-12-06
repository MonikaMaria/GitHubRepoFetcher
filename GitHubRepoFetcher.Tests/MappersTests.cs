using FluentAssertions;
using GitHubRepoFetcher.Application;
using GitHubRepoFetcher.Domain;

namespace GitHubRepoFetcher.Tests;

public class MappersTests
{
    private readonly string _repositoryName = "repoName";
    private readonly string _userName = "exampleUser";
    private readonly string _sha1 = "9df9af6bc4a05df0836246554fe3a25156be95ef";
    private readonly string _sha2 = "246554fe3a25156be95ef9df9af6bc4a05df0836";
    private readonly string _committerName1 = "commiter1";
    private readonly string _committerEmail1 = "commiter1@test.com";
    private readonly string _message1 = "message1";
    private readonly string _committerName2 = "committer2";
    private readonly string _committerEmail2 = "committer2@test.com";
    private readonly string _message2 = "message2";
    private readonly DateTimeOffset _date1 = DateTimeOffset.UtcNow;
    private readonly DateTimeOffset _date2 = DateTimeOffset.UtcNow.AddDays(-1);

    [Fact]
    public void MapToCommitsToDisplay_ReturnsProperResult()
    {
        // Arrange
        var data = ArrangeGitHubCommitItems();

        // Act
        var result = data.MapToCommitsToDisplay(_repositoryName);

        // Assert
        result.Should().BeEquivalentTo(new List<CommitDisplayModel>
        {
            new(_repositoryName, _sha1, _message1, _committerName1, _date1),
            new(_repositoryName, _sha2, _message2, _committerName2, _date2)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public void MapToEntities_ReturnsProperResult()
    {
        // Arrange
        var data = ArrangeGitHubCommitItems();

        // Act
        var result = data.MapToEntities(_userName, _repositoryName);

        // Assert
        result.Should().BeEquivalentTo(new List<Commit>
        {
            Commit.Create(_userName, _repositoryName, _sha1, _message1, _committerName1, _committerEmail1, _date1),
            Commit.Create(_userName, _repositoryName, _sha2, _message2, _committerName2, _committerEmail2, _date2)
        }, options =>
        {
            options.WithoutStrictOrdering();
            return options.Excluding(c => c.Id);
        });
    }

    private List<GitHubCommitItem> ArrangeGitHubCommitItems()
    {
        return
        [
            new()
            {
                Commit = new GitHubCommit
                {
                    Committer = new GitHubCommitter
                    {
                        Date = _date1,
                        Email = _committerEmail1,
                        Name = _committerName1
                    },
                    Message = _message1
                },
                Sha = _sha1
            },
            new()
            {
                Commit = new GitHubCommit
                {
                    Committer = new GitHubCommitter
                    {
                        Date = _date2,
                        Email = _committerEmail2,
                        Name = _committerName2
                    },
                    Message = _message2
                },
                Sha = _sha2
            }
        ];
    }
}