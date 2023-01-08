using AurPackger.RepoHelper;
using Microsoft.AspNetCore.Mvc;

namespace AurPackger.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RepoController : ControllerBase
{
  private readonly RepoManager _repoManager;
  private readonly ParuWrap _paruWrap;

  public RepoController(RepoManager repoManager, ParuWrap paruWrap)
  {
    _repoManager = repoManager;
    _paruWrap = paruWrap;
  }

  // todo:
  // - POST /repo/xxx/add-package
  // - POST /repo/xxx/remove-package
  // - POST /repo/xxx/update


  /**
   * add a aur package to the repo
   */
  [HttpPost("{repoName}/add-package")]
  public async Task<IActionResult> AddPackageAsync(
    string repoName,
    [FromBody] AddPackageReq req)
  {
    var repo = await _repoManager.GetRepoAsync(repoName);
    var result = await _paruWrap.BuildPackageAsync(req.PackageName);
    if (result.Succeed)
    {
      await result.SaveAsync(Path.Combine(repo.DbFolder, "x86_64"));
    }

    return Ok();
  }
}

public class AddPackageReq
{
  public string PackageName { get; set; }
}
