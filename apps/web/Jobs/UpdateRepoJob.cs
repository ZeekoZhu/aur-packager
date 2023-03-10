using AurPackager.RepoHelper;
using AurPackager.Web.Entites;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace AurPackager.Web.Jobs;

public class UpdateRepoJob : IJob
{
  private readonly RepoManager _repoManager;
  private readonly AurWrap _aurWrap;
  private readonly PackageDbContext _packageDb;
  public static readonly JobKey JobKey = new("UpdateRepoJob");

  public UpdateRepoJob(
    RepoManager repoManager,
    AurWrap aurWrap,
    PackageDbContext packageDb)
  {
    _repoManager = repoManager;
    _aurWrap = aurWrap;
    _packageDb = packageDb;
  }

  public async Task Execute(IJobExecutionContext context)
  {
    try
    {
      var repoName =
        context.MergedJobDataMap.GetString("repoName") ??
        throw new InvalidOperationException(
          "JobDataMap must contain a value for the key 'repoName'.");
      var repo = await _repoManager.GetRepoAsync(repoName);
      var packages = await _packageDb.AurPackages
        .Where(it => it.LocalRepoName == repoName)
        .Select(it => it.PackageName)
        .ToListAsync();
      foreach (var package in packages)
      {
        var result = await _aurWrap.BuildPackageAsync(package);
        if (result.Succeed)
        {
          await result.SaveAsync(repo.PackagesFolder);
        }
      }

      await repo.UpdatePackagesAsync();
    }
    catch (Exception e)
    {
      throw new JobExecutionException(
        msg: "Update repo failed",
        refireImmediately: false,
        cause: e);
    }
  }
}
