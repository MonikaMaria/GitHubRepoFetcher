using Spectre.Console;

namespace GitHubRepoFetcher.Application;

public interface IUIHandler
{
    void DisplayTitle();
    string DisplayUserNamePrompt();
    string DisplayRepositoryNamePrompt();
    void DisplayInitializing();
    void DisplayDescription();
    void DisplayShortSeparator();
    void DisplayResultsHeader();
    void DisplayResultsLegend();
    void DisplayCommits(IOrderedEnumerable<CommitDisplayModel>? commitsToDisplay);
    void DisplayLine();
    void DisplayUserNameError(string userName);
    void DisplayRepositoryNameError(string repositoryName);
    void DisplaySavingData();
    void DisplayDataSaved();
}

public class UIHandler : IUIHandler
{
    public void DisplayTitle()
    {
        var titleRule = new Rule("GitHub Repo Fetcher")
        {
            Justification = Justify.Left
        };

        AnsiConsole.Write(titleRule);
    }

    public string DisplayUserNamePrompt()
    {
        return AnsiConsole.Prompt(new TextPrompt<string>("[lightseagreen]GitHub user name:[/]"));
    }

    public string DisplayRepositoryNamePrompt()
    {
        return AnsiConsole.Prompt(new TextPrompt<string>("[lightseagreen]GitHub repository name:[/]"));
    }

    public void DisplayInitializing()
    {
        AnsiConsole.MarkupLine("[yellow]Initializing...[/]");
    }

    public void DisplayDescription()
    {
        AnsiConsole.MarkupLine("[deepskyblue1]→ Fetch commits from GitHub repository. Please provide data below.[/]");
    }

    public void DisplayShortSeparator()
    {
        Console.WriteLine("---");
    }

    public void DisplayResultsHeader()
    {
        AnsiConsole.MarkupLine("ALL COMMITS ON DEFAULT BRANCH:");
    }

    public void DisplayResultsLegend()
    {
        AnsiConsole.MarkupLine("[gray][[repository name]]/[[sha]]: message [[committer]][/]");
    }

    public void DisplayCommits(IOrderedEnumerable<CommitDisplayModel>? commitsToDisplay)
    {
        var commits = (commitsToDisplay ?? Enumerable.Empty<CommitDisplayModel>()).ToArray();
        if (commits.Any())
        {
            foreach (var commit in commits)
            {
                AnsiConsole.WriteLine(commit.ToString());
            }
        }
        else
        {
            AnsiConsole.MarkupLine("[yellow]No commits to display.[/]");
        }
    }

    public void DisplayLine()
    {
        AnsiConsole.Write(new Rule());
    }

    public void DisplayUserNameError(string userName)
    {
        AnsiConsole.MarkupLine($"[red3]User[/] [yellow]{userName}[/] [red3]does not exist.[/]");
    }

    public void DisplayRepositoryNameError(string repositoryName)
    {
        AnsiConsole.MarkupLine(
            $"[red3]Repository[/] [yellow]{repositoryName}[/] [red3]does not exist.[/]");
    }

    public void DisplaySavingData()
    {
        AnsiConsole.MarkupLine("[yellow]Saving data...[/]");
    }

    public void DisplayDataSaved()
    {
        AnsiConsole.MarkupLine("[yellow]Data saved![/]");
    }
}