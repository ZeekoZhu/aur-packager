using System.Text;
using System.Text.RegularExpressions;
using CliWrap;
using Microsoft.Extensions.Logging;

namespace AurPackager.RepoHelper;

public class AurWrap
{
  private readonly ILogger<AurWrap> _logger;

  public AurWrap(ILoggerFactory loggerFactory)
  {
    _logger = loggerFactory.CreateLogger<AurWrap>();
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
    public bool Succeed { get; set; }
  }

  public async Task<BuildResult> BuildPackageAsync(string packageName)
  {
    var tmpDir = Path.Combine(Path.GetTempPath(), "paru-wrap");
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

      await GetPkgBuildAsync(packageName, tmpDir);

      await BuildFromSourceAsync(packageName, tmpDir);

      _logger.LogInformation("Package {PackageName} succeed", packageName);

      var pikaurPkgDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Personal),
        ".cache",
        "pikaur",
        "pkg");
      var result = new BuildResult
      {
        Succeed = true,
        FileName = Directory.GetFiles(
            pikaurPkgDir,
            $"{packageName}-*.pkg.tar.zst")
          .FirstOrDefault()!
      };
      return result;
    }
    catch (Exception e)
    {
      _logger.LogError(
        e,
        "Build {PackageName} failed",
        packageName);
      return new BuildResult
      {
        Succeed = false
      };
    }
  }

  private async Task BuildFromSourceAsync(string packageName, string workingDir)
  {
    var output = new StringBuilder();
    try
    {
      using var autoYes = new AutoYes(output, _logger);
      autoYes.Yes(new Regex(@"\[y/N]$"))
        .Yes(new Regex(@"\(default=1\)"), "1")
        .Yes(new Regex(@"\[Y/n]"), "Y");
      var paru = Cli.Wrap("pikaur")
        .WithWorkingDirectory(Path.Combine(workingDir, packageName))
        .WithStandardInputPipe(PipeSource.FromStream(autoYes.Input))
        .WithStandardErrorPipe(
          PipeTarget.ToDelegate(it => autoYes.AnswerAsync(it, true)))
        .WithStandardOutputPipe(
          PipeTarget.ToDelegate(it => autoYes.AnswerAsync(it, false)));

      var buildPkg = paru.WithArguments(
        $"-P --removemake --skipreview --noconfirm --noedit --nodeps");
      _logger.LogInformation("Building package {PackageName}", packageName);
      _logger.LogInformation("Command: {Command}", buildPkg.ToString());

      await buildPkg
        .ExecuteAsync();
    }
    catch (Exception e)
    {
      throw new ParuWrapException($"Build '{packageName}' failed\n{output}", e);
    }
  }

  private async Task GetPkgBuildAsync(string packageName, string workingDir)
  {
    var output = new StringBuilder();
    try
    {
      using var autoYes = new AutoYes(output, _logger);
      autoYes.Yes(new Regex(@"\[y/N]$"))
        .Yes(new Regex(@"\(default=1\)"), "1")
        .Yes(new Regex(@"\[Y/n]$"), "Y");

      var paru = Cli.Wrap("pikaur")
        .WithWorkingDirectory(workingDir)
        .WithStandardInputPipe(PipeSource.FromStream(autoYes.Input))
        .WithStandardErrorPipe(
          PipeTarget.ToDelegate(it => autoYes.AnswerAsync(it, true)))
        .WithStandardOutputPipe(
          PipeTarget.ToDelegate(it => autoYes.AnswerAsync(it, false)));

      _logger.LogInformation("Fetching package {PackageName}", packageName);
      var fetchPkgBuild = paru.WithArguments($"-G {packageName}");

      _logger.LogInformation("Command: {Command}", fetchPkgBuild.ToString());
      await fetchPkgBuild.ExecuteAsync();
    }
    catch (Exception e)
    {
      throw new ParuWrapException(
        $"Failed to fetch '{packageName}', output:\n{output}",
        e);
    }
  }
}
