using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Monitoring.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixEntityBaseProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "Pages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "System",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Pages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "System",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "Pages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "System");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Pages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "System");
        }
    }
}
