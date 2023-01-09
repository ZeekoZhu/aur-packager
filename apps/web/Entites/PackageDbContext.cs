using Microsoft.EntityFrameworkCore;

namespace AurPackger.Web.Entites;

public class PackageDbContext : DbContext
{
  public DbSet<AurPackageModel> AurPackages { get; } = null!;

  public string DbPath { get; }

  public PackageDbContext()
  {
    var folder = Environment.SpecialFolder.LocalApplicationData;
    var path = Environment.GetFolderPath(folder);
    DbPath = Path.Combine(path, "aur-packager", "aur-packager.db");
  }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    => optionsBuilder.UseSqlite($"Data Source={DbPath}");


}
