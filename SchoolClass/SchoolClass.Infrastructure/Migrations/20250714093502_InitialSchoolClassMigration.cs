using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolClass.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchoolClassMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SchoolClasses",
                columns: table => new
                {
                    SchoolClassID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchoolClassName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolClasses", x => x.SchoolClassID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SchoolClasses_SchoolClassName",
                table: "SchoolClasses",
                column: "SchoolClassName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SchoolClasses");
        }
    }
}
