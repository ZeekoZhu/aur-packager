using AurPackager.RepoHelper;
using AurPackager.Web.Entites;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace AurPackager.Web.Jobs;

public class UpdateRepoJob : IJob
{
  private readonly RepoManager _repoManager;
  private readonly ParuWrap _paruWrap;
  private readonly PackageDbContext _packageDb;
  public static readonly JobKey JobKey = new("UpdateRepoJob");

  public UpdateRepoJob(
    RepoManager repoManager,
    ParuWrap paruWrap,
    PackageDbContext packageDb)
  {
    _repoManager = repoManager;
    _paruWrap = paruWrap;
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
      var buildTasks = packages.Select(
        async pkg =>
        {
          var result = await _paruWrap.BuildPackageAsync(pkg);
          if (result.Succeed)
          {
            await result.SaveAsync(repo.PackagesFolder);
          }
        });
      await Task.WhenAll(buildTasks);
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
