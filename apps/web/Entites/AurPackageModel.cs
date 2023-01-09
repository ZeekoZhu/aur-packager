using System.ComponentModel.DataAnnotations;

namespace AurPackager.Web.Entites;

public class AurPackageModel
{
  public string LocalRepoName { get; set; }
  [Key]
  public string PackageName { get; set; }
}
