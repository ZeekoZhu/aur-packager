using System.Text;

namespace AurPackager.RepoHelper.Test;

public class AutoYesTests
{
  [Fact]
  public async Task Test1()
  {
    var autoYes = new AutoYes(new StringBuilder());
    autoYes.Yes("[y/N]:").Yes("(default=1)", "1");
    await autoYes.AnswerAsync("bla bla bla bla [y/N]:");
    await autoYes.AnswerAsync("bla bla bla bla (default=1):");
    autoYes.Input.Position = 0;
    var reader = new StreamReader(autoYes.Input);
    var result = await reader.ReadToEndAsync();
    result.Should().Be("y\n1\n");
  }
}
