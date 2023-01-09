using AurPackager.RepoHelper;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
var repoRoot = Path.Combine(builder.Environment.WebRootPath, "repos");
Directory.CreateDirectory(repoRoot);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDirectoryBrowser();
builder.Services.AddLogging(cfg => cfg.AddConsole());
builder.Services.AddSingleton<RepoManager>(
  s => new RepoManager(repoRoot, s.GetRequiredService<ILoggerFactory>()));
builder.Services.AddSingleton<ParuWrap>();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
