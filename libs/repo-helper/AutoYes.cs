using System.Text;

namespace AurPackager.RepoHelper;

public class AutoYes : IDisposable
{
  private readonly StringBuilder _output;
  private readonly List<KeyValuePair<string, string>> _promptAnswers = new();

  public AutoYes(StringBuilder output)
  {
    _output = output;
    Input = new MemoryStream();
  }

  public Stream Input { get; }

  public async Task AnswerAsync(string output)
  {
    _output.AppendLine(output);
    foreach (var (promptOutput, input) in _promptAnswers)
    {
      if (output.Contains(promptOutput))
      {
        await Input.WriteAsync(
          Console.InputEncoding.GetBytes(input + Environment.NewLine));
        await Input.FlushAsync();
        break;
      }
    }
  }

  public AutoYes Yes(string output, string input = "y")
  {
    _promptAnswers.Add(new(output, input));
    return this;
  }


  public void Dispose()
  {
    Input.Dispose();
  }
}
