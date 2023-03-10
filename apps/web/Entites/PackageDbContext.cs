using Microsoft.EntityFrameworkCore;

namespace AurPackager.Web.Entites;

public class PackageDbContext : DbContext
{
  public DbSet<AurPackageModel> AurPackages { get; set; } = null!;

  public string DbPath { get; }

  public PackageDbContext()
  {
    var folder = Environment.SpecialFolder.LocalApplicationData;
    var path = Environment.GetFolderPath(folder);
    DbPath = Path.Combine(path, "aur-packager", "aur-packager.db");
    var dbFolder = Path.GetDirectoryName(DbPath)!;
    if (!Directory.Exists(dbFolder))
    {
      Directory.CreateDirectory(dbFolder);
    }
  }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    => optionsBuilder.UseSqlite($"Data Source={DbPath}");
}
