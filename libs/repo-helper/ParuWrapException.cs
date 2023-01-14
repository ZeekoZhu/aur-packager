using System.Runtime.Serialization;

namespace AurPackager.RepoHelper;

[Serializable]
public class ParuWrapException : Exception
{
  public ParuWrapException(string message, Exception innerException) : base(
    message,
    innerException)
  {
  }

  protected ParuWrapException(SerializationInfo info, StreamingContext context)
    : base(info, context)
  {
  }
}
