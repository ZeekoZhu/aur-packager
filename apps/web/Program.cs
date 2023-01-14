using AurPackager.RepoHelper;
using AurPackager.Web.Entites;
using AurPackager.Web.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Quartz;

var builder = WebApplication.CreateBuilder(args);
var repoRoot = Path.Combine(builder.Environment.WebRootPath, "repos");
Directory.CreateDirectory(repoRoot);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDirectoryBrowser();
builder.Services.AddLogging(cfg => cfg.AddConsole());

// app services
builder.Services.AddSingleton<RepoManager>(
  s => new RepoManager(repoRoot, s.GetRequiredService<ILoggerFactory>()));
builder.Services.AddSingleton<ParuWrap>();

// swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// db
builder.Services.AddDbContext<PackageDbContext>();

// job scheduler
builder.Services.AddQuartz(
  q =>
  {
    q.UseMicrosoftDependencyInjectionJobFactory();
    q.AddJob<UpdateRepoJob>(
      opt => opt.WithIdentity(UpdateRepoJob.JobKey).StoreDurably(true));
  });

// ASP.NET Core hosting
builder.Services.AddQuartzServer(
  options =>
  {
    // when shutting down we want jobs to complete gracefully
    options.WaitForJobsToComplete = true;
  });
var app = builder.Build();


if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

// allow browsing /repos
void EnableRepo()
{
  var fileProvider = new PhysicalFileProvider(
    repoRoot);
  var requestPath = "/repos";

  // Enable displaying browser links.
  app.UseStaticFiles(
    new StaticFileOptions
    {
      FileProvider = fileProvider,
      RequestPath = requestPath
    });

  app.UseDirectoryBrowser(
    new DirectoryBrowserOptions
    {
      FileProvider = fileProvider,
      RequestPath = requestPath
    });
}

EnableRepo();

using (var serviceScope = app.Services
         .GetRequiredService<IServiceScopeFactory>()
         .CreateScope())
{
  var context =
    serviceScope.ServiceProvider.GetRequiredService<PackageDbContext>();
  context.Database.Migrate();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
