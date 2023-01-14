using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace AurPackager.RepoHelper;

public class AutoYes : IDisposable
{
  private readonly StringBuilder _output;
  private readonly ILogger _logger;
  private readonly List<KeyValuePair<Regex, string>> _promptAnswers = new();

  public AutoYes(StringBuilder output, ILogger logger)
  {
    _output = output;
    _logger = logger;
    Input = new MemoryStream();
  }

  public Stream Input { get; }

  public async Task AnswerAsync(string output, bool stderr = false)
  {
    _output.AppendLine($"{(stderr ? "stderr" : "stdout")}: {output}");
    foreach (var (promptOutput, input) in _promptAnswers)
    {
      if (promptOutput.IsMatch(output))
      {
        _logger.LogInformation("Ask: {Output}", output);
        _logger.LogInformation("Answer: {Input}", input);
        _output.AppendLine($"      > {input}");
        await Input.WriteAsync(
          Console.InputEncoding.GetBytes(input + Environment.NewLine));
        break;
      }
    }

    _logger.LogInformation("{Output}", output);
  }

  public AutoYes Yes(Regex output, string input = "y")
  {
    _promptAnswers.Add(new(output, input));
    return this;
  }


  public void Dispose()
  {
    Input.Dispose();
  }
}
