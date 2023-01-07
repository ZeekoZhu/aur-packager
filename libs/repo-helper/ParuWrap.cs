using System.Diagnostics;
using System.Text;
using CliWrap;

namespace AurPackger.RepoHelper;

public class ParuWrap
{
  public class BuildResult
  {
    public Task SaveAsync(string destDir)
    {
      if (!Succeed || FileName is null)
      {
        return Task.CompletedTask;
      }

      Directory.CreateDirectory(destDir);
      File.Copy(FileName, Path.Combine(destDir, Path.GetFileName(FileName)));
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
      if (Directory.Exists(buildDir))
      {
        Directory.Delete(buildDir, true);
      }

      var paru = Cli.Wrap("paru")
        .WithWorkingDirectory(tmpDir)
        .WithStandardErrorPipe(PipeTarget.ToStringBuilder(output))
        .WithStandardOutputPipe(PipeTarget.ToStringBuilder(output));
      await paru.WithArguments($"-G {packageName}")
        .ExecuteAsync();
      await paru.WithArguments($"-B {packageName} --noconfirm")
        .ExecuteAsync();
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
      return new BuildResult
      {
        Output = output.ToString(),
        Succeed = false
      };
    }
  }
}
