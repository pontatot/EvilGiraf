using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EvilGiraf.Migrations
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public partial class ApplicationSinglePort : Migration
    {
        private const string TableName = "Applications";
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ports",
                table: TableName);

            migrationBuilder.AddColumn<int>(
                name: "Port",
                table: TableName,
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Port",
                table: TableName);

            migrationBuilder.AddColumn<int[]>(
                name: "Ports",
                table: TableName,
                type: "integer[]",
                nullable: false,
                defaultValue: Array.Empty<int>());
        }
    }
}
