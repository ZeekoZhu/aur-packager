using Microsoft.Extensions.Logging;

namespace AurPackger.RepoHelper.Test;

public class RepoManagerTests
{
  private readonly string _tempDir;
  private readonly ILoggerFactory _loggerFactory;

  public RepoManagerTests(ITestOutputHelper outputHelper)
  {
    _loggerFactory =
      LoggerFactory.Create(builder => builder.AddXUnit(outputHelper));
    _tempDir = Path.Combine(
      Path.GetTempPath(),
      "repo-manager-tests",
      Path.GetRandomFileName());
    outputHelper.WriteLine("Temp dir: " + _tempDir);
    Directory.CreateDirectory(_tempDir);
  }

  [Fact]
  public async Task Create_repo_if_not_exists()
  {
    var repoManager = new RepoManager(Path.Combine(_tempDir, "repo1"), _loggerFactory);
    var repo = await repoManager.GetRepoAsync("foo");
    Assert.NotNull(repo);
    Assert.True(File.Exists(repo.DbPath), repo.DbPath);

    var repo2 = await repoManager.GetRepoAsync("foo");
    repo2.Should().BeEquivalentTo(repo);
  }

  [Fact]
  public async Task Remove_repo()
  {
    var repoManager = new RepoManager(Path.Combine(_tempDir, "repo2"), _loggerFactory);
    var repo = await repoManager.GetRepoAsync("foo");
    Assert.NotNull(repo);
    await repoManager.RemoveRepoAsync("foo");
    File.Exists(repo.DbPath).Should().BeFalse();
  }
}
