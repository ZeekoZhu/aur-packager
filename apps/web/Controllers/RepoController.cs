using AurPackager.RepoHelper;
using AurPackager.Web.Entites;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AurPackager.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RepoController : ControllerBase
{
  private readonly RepoManager _repoManager;
  private readonly ParuWrap _paruWrap;
  private readonly PackageDbContext _packageDb;

  public RepoController(
    RepoManager repoManager,
    ParuWrap paruWrap,
    PackageDbContext packageDb)
  {
    _repoManager = repoManager;
    _paruWrap = paruWrap;
    _packageDb = packageDb;
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
    var aurPackageModel = new AurPackageModel
    {
      PackageName = req.PackageName,
      LocalRepoName = repoName,
    };
    if (!await _packageDb.AurPackages.AnyAsync(
          it => it.PackageName == req.PackageName &&
                it.LocalRepoName == repoName))
    {
      _packageDb.AurPackages.Add(aurPackageModel);
      await _packageDb.SaveChangesAsync();
    }

    var repo = await _repoManager.GetRepoAsync(repoName);
    var result = await _paruWrap.BuildPackageAsync(req.PackageName);
    if (result.Succeed)
    {
      await result.SaveAsync(Path.Combine(repo.DbFolder, "x86_64"));
      await repo.UpdatePackagesAsync();
    }

    return Ok(result);
  }
}

public class AddPackageReq
{
  public string PackageName { get; set; }
}
