using System.Text;
using CliWrap;
using Microsoft.Extensions.Logging;

namespace AurPackger.RepoHelper;

public class RepoManager
{
  private readonly string _repoPath;
  private readonly ILoggerFactory _loggerFactory;
  private readonly ILogger<RepoManager> _logger;

  public RepoManager(string repoPath, ILoggerFactory loggerFactory)
  {
    _repoPath = repoPath;
    _loggerFactory = loggerFactory;
    _logger = loggerFactory.CreateLogger<RepoManager>();
  }

  public class LocalRepo
  {
    private readonly ILogger<LocalRepo> _logger;

    public LocalRepo(string name, string dbFolder, ILoggerFactory loggerFactory)
    {
      Name = name;
      DbFolder = dbFolder;
      _logger = loggerFactory.CreateLogger<LocalRepo>();
    }

    public string Name { get; }
    public string DbPath => Path.Combine(DbFolder, $"{Name}.db.tar.gz");
    public string DbFolder { get; }

    public async Task InitAsync()
    {
      if (!File.Exists(DbPath))
      {
        var output = new StringBuilder();
        try
        {
          _logger.LogInformation("Creating new repo {Name}", Name);
          Directory.CreateDirectory(DbFolder);
          var repoAdd = Cli.Wrap("repo-add")
            .WithArguments(DbPath);
          _logger.LogInformation("Command: {Command}", repoAdd.ToString());
          await repoAdd
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(output))
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(output))
            .ExecuteAsync();
          _logger.LogInformation("Output: {Output}", output);
        }
        catch (Exception e)
        {
          _logger.LogError(e, "Error creating repo {Name}, command output: {Output}", Name, output);
          throw;
        }
      }
    }
  }

  public async Task<LocalRepo> GetRepoAsync(string repoName)
  {
    _logger.LogInformation("Getting repo {RepoName}", repoName);
    var dbFolder = GetDbFolder(repoName);
    var repo = new LocalRepo(repoName, dbFolder, _loggerFactory);
    await repo.InitAsync();
    return repo;
  }

  private string GetDbFolder(string repoName)
  {
    return Path.Combine(_repoPath, repoName);
  }

  public Task RemoveRepoAsync(string repoName)
  {
    _logger.LogInformation("Removing repo {RepoName}", repoName);
    Directory.Delete(GetDbFolder(repoName), true);
    return Task.CompletedTask;
  }
}