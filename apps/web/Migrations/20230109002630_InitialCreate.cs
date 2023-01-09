using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AurPackger.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AurPackages",
                columns: table => new
                {
                    PackageName = table.Column<string>(type: "TEXT", nullable: false),
                    LocalRepoName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AurPackages", x => x.PackageName);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AurPackages");
        }
    }
}
