using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Inpad.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoriesAuditSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "ArchObjects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DesignStage",
                table: "ArchObjects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProjectStatus",
                table: "ArchObjects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortName",
                table: "ArchObjects",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Action = table.Column<string>(type: "text", nullable: false),
                    EntityType = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<int>(type: "integer", nullable: true),
                    Details = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    UserEmail = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ObjectCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArchObjectCategories",
                columns: table => new
                {
                    CategoriesId = table.Column<int>(type: "integer", nullable: false),
                    ObjectsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchObjectCategories", x => new { x.CategoriesId, x.ObjectsId });
                    table.ForeignKey(
                        name: "FK_ArchObjectCategories_ArchObjects_ObjectsId",
                        column: x => x.ObjectsId,
                        principalTable: "ArchObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArchObjectCategories_ObjectCategories_CategoriesId",
                        column: x => x.CategoriesId,
                        principalTable: "ObjectCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppSettings_Key",
                table: "AppSettings",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArchObjectCategories_ObjectsId",
                table: "ArchObjectCategories",
                column: "ObjectsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "ArchObjectCategories");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "ObjectCategories");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "ArchObjects");

            migrationBuilder.DropColumn(
                name: "DesignStage",
                table: "ArchObjects");

            migrationBuilder.DropColumn(
                name: "ProjectStatus",
                table: "ArchObjects");

            migrationBuilder.DropColumn(
                name: "ShortName",
                table: "ArchObjects");
        }
    }
}
