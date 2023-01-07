namespace AurPackger.RepoHelper.Test;

public class YayWrapTests : IDisposable
{
  private string _tempDir;
  public YayWrapTests()
  {
    _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    Directory.CreateDirectory(_tempDir);
  }
  [Fact]
  public async Task Create_package()
  {
    var yay = new ParuWrap();
    var result = await yay.BuildPackageAsync("yay-bin");
    Assert.NotNull(result.FileName);
    var tempDir = Path.Combine(_tempDir, "yay-bin");
    Directory.CreateDirectory(tempDir);
    await result.SaveAsync(tempDir);
    Assert.True(File.Exists(Path.Combine(tempDir, result.FileName)));
    // get file size
    var size = new FileInfo(Path.Combine(tempDir, result.FileName)).Length;
    Assert.True(size > 0);
  }

  public void Dispose()
  {
    Directory.Delete(_tempDir, true);
  }
}
