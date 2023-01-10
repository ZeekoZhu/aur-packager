using AurPackager.RepoHelper;
using AurPackager.Web.Entites;
using AurPackager.Web.Jobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace AurPackager.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RepoController : ControllerBase
{
  private readonly PackageDbContext _packageDb;
  private readonly ISchedulerFactory _schedulerFactory;

  public RepoController(
    PackageDbContext packageDb,
    ISchedulerFactory schedulerFactory)
  {
    _packageDb = packageDb;
    _schedulerFactory = schedulerFactory;
  }

  // todo:
  // x POST /repo/xxx/remove-package
  // - POST /repo/xxx/update
  // - async build
  // - dockerfile


  /**
   * add a aur package to the repo
   */
  [HttpPost("{repoName}/package")]
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

    return Ok();
  }

  [HttpDelete("{repoName}/package/{packageName}")]
  public async Task<IActionResult> RemovePackageAsync(
    string repoName,
    string packageName)
  {
    var aurPackageModel = await _packageDb.AurPackages.FirstOrDefaultAsync(
      it => it.PackageName == packageName &&
            it.LocalRepoName == repoName);
    if (aurPackageModel != null)
    {
      _packageDb.AurPackages.Remove(aurPackageModel);
      await _packageDb.SaveChangesAsync();
    }

    return Ok();
  }

  [HttpPost("{repoName}/update")]
  public async Task<IActionResult> UpdateRepoAsync(
    string repoName)
  {
    var scheduler = await _schedulerFactory.GetScheduler();
    await scheduler.TriggerJob(
      UpdateRepoJob.JobKey,
      new JobDataMap { { "repoName", repoName } });

    return Ok();
  }
}

public class AddPackageReq
{
  public string PackageName { get; set; }
}
