using System.Text;
using CliWrap;
using Microsoft.Extensions.Logging;

namespace AurPackger.RepoHelper;

public class ParuWrap
{
  private readonly ILogger<ParuWrap> _logger;

  public ParuWrap(ILoggerFactory loggerFactory)
  {
    _logger = loggerFactory.CreateLogger<ParuWrap>();
  }

  public class BuildResult
  {
    public Task SaveAsync(string destDir)
    {
      if (!Succeed || FileName is null)
      {
        return Task.CompletedTask;
      }

      Directory.CreateDirectory(destDir);
      var destFileName = Path.Combine(destDir, Path.GetFileName(FileName));
      if (File.Exists(destFileName))
      {
        File.Delete(destFileName);
      }

      File.Copy(FileName, destFileName);
      return Task.CompletedTask;
    }

    public string? FileName { get; set; }
    public string? Output { get; set; }
    public bool Succeed { get; set; }
  }

  public async Task<BuildResult> BuildPackageAsync(string packageName)
  {
    var tmpDir = Path.Combine(Path.GetTempPath(), "paru-wrap");
    var output = new StringBuilder();
    try
    {
      Directory.CreateDirectory(tmpDir);
      var buildDir = Path.Combine(tmpDir, packageName);
      _logger.LogInformation("Build dir: {BuildDir}", buildDir);
      if (Directory.Exists(buildDir))
      {
        _logger.LogInformation("Deleting previous build dir");
        Directory.Delete(buildDir, true);
      }

      var paru = Cli.Wrap("paru")
        .WithWorkingDirectory(tmpDir)
        .WithStandardErrorPipe(PipeTarget.ToStringBuilder(output))
        .WithStandardOutputPipe(PipeTarget.ToStringBuilder(output));
      _logger.LogInformation("Fetching package {PackageName}", packageName);
      var fetchPkgBuild = paru.WithArguments($"-G {packageName}");
      _logger.LogInformation("Command: {Command}", fetchPkgBuild.ToString());
      await fetchPkgBuild.ExecuteAsync();
      var buildPkg = paru.WithArguments(
        $"-B {packageName}");
      _logger.LogInformation("Building package {PackageName}", packageName);
      _logger.LogInformation("Command: {Command}", buildPkg.ToString());
      await buildPkg
        .WithStandardInputPipe(PipeSource.FromString(LotsOfYes()))
        .ExecuteAsync();
      _logger.LogInformation("Package {PackageName} succeed", packageName);
      var result = new BuildResult
      {
        Output = output.ToString(),
        Succeed = true,
        FileName = Directory.GetFiles(buildDir, "*.pkg.tar.zst")
          .FirstOrDefault()!
      };
      return result;
    }
    catch (Exception e)
    {
      _logger.LogError(
        e,
        "Build {PackageName} failed, output: {Output}",
        packageName,
        output);
      return new BuildResult
      {
        Output = output.ToString(),
        Succeed = false
      };
    }
  }

  private string LotsOfYes()
  {
    return string.Join(Environment.NewLine, Enumerable.Repeat("y", 50));
  }
}
