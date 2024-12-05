namespace GitHubRepoFetcher.Application
{
    public interface IMainLoopService
    {
        Task<string> GetValidatedUserName(string inputUserName,
            CancellationToken cancellationToken);

        Task<string> GetValidatedRepositoryName(string inputUserName,
            string inputRepositoryName, CancellationToken cancellationToken);

        Task<IEnumerable<GitHubCommitItem>> GetCommits(string userName, string repositoryName,
            CancellationToken cancellationToken);

        void DisplayResult(IEnumerable<GitHubCommitItem> commits, string repositoryName);

        Task SaveData(string userName, string repositoryName, IEnumerable<GitHubCommitItem> commits, CancellationToken cancellationToken);
    }

    public sealed class MainLoopService(IGitHubRepositoryService gitHubRepositoryService, IUIHandler uiHandler)
        : IMainLoopService
    {
        public async Task<string> GetValidatedUserName(string inputUserName, CancellationToken cancellationToken)
        {
            while (string.IsNullOrEmpty(inputUserName))
            {
                inputUserName = uiHandler.DisplayUserNamePrompt();

                var userExists = await gitHubRepositoryService.CheckUserExistsAsync(inputUserName, cancellationToken);
                if (userExists is false)
                {
                    uiHandler.DisplayUserNameError(inputUserName);
                    inputUserName = string.Empty;
                }
            }

            return inputUserName;
        }

        public async Task<string> GetValidatedRepositoryName(string inputUserName, string inputRepositoryName,
            CancellationToken cancellationToken)
        {
            while (string.IsNullOrEmpty(inputRepositoryName))
            {
                inputRepositoryName = uiHandler.DisplayRepositoryNamePrompt();

                var repositoryExists =
                    await gitHubRepositoryService.CheckRepositoryExistsAsync(inputUserName, inputRepositoryName,
                        cancellationToken);
                if (repositoryExists is false)
                {
                    uiHandler.DisplayRepositoryNameError(inputRepositoryName);
                    inputRepositoryName = string.Empty;
                }
            }

            return inputRepositoryName;
        }

        public async Task<IEnumerable<GitHubCommitItem>> GetCommits(string userName, string repositoryName,
            CancellationToken cancellationToken)
        {
            return await gitHubRepositoryService.GetGitHubCommitsAsync(userName, repositoryName, cancellationToken);
        }

        public void DisplayResult(IEnumerable<GitHubCommitItem> commits, string repositoryName)
        {
            uiHandler.DisplayShortSeparator();
            uiHandler.DisplayResultsHeader();
            uiHandler.DisplayResultsLegend();
            uiHandler.DisplayShortSeparator();

            var commitsToDisplay = commits.MapToCommitsToDisplay(repositoryName);
            uiHandler.DisplayCommits(commitsToDisplay);

            uiHandler.DisplayLine();
        }

        public async Task SaveData(string userName, string repositoryName, IEnumerable<GitHubCommitItem> commits,
            CancellationToken cancellationToken)
        {
            uiHandler.DisplaySavingData();
            await gitHubRepositoryService.SaveCommits(userName, repositoryName, commits, cancellationToken);
            uiHandler.DisplayDataSaved();
        }
    }
}
