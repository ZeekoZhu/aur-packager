using Microsoft.Extensions.Logging;

namespace AurPackger.RepoHelper.Test;

public class YayWrapTests : IDisposable
{
  private readonly string _tempDir;
  private readonly ILoggerFactory _loggerFactory;

  public YayWrapTests(ITestOutputHelper output)
  {
    _loggerFactory = LoggerFactory.Create(b => b.AddXUnit(output));
    _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    Directory.CreateDirectory(_tempDir);
  }
  [Fact]
  public async Task Build_valid_package()
  {
    var aur = new ParuWrap(_loggerFactory);
    var result = await aur.BuildPackageAsync("yay-bin");
    Assert.NotNull(result.FileName);
    var tempDir = Path.Combine(_tempDir, "yay-bin");
    Directory.CreateDirectory(tempDir);
    await result.SaveAsync(tempDir);
    Assert.True(File.Exists(Path.Combine(tempDir, result.FileName!)));
    // get file size
    var size = new FileInfo(Path.Combine(tempDir, result.FileName!)).Length;
    Assert.True(size > 0);
  }

  [Fact]
  public async Task Build_invalid_package()
  {
    var aur = new ParuWrap(_loggerFactory);
    var result = await aur.BuildPackageAsync("yay-bin-foo-bar");
    Assert.Null(result.FileName);
  }

  void IDisposable.Dispose()
  {
    Directory.Delete(_tempDir, true);
  }
}
