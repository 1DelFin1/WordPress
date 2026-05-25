using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inpad.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaFieldsAndSeoFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AltText",
                table: "ObjectMedias",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Caption",
                table: "ObjectMedias",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "ObjectMedias",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OgImageUrl",
                table: "ArchObjects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeoKeywords",
                table: "ArchObjects",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AltText",
                table: "ObjectMedias");

            migrationBuilder.DropColumn(
                name: "Caption",
                table: "ObjectMedias");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "ObjectMedias");

            migrationBuilder.DropColumn(
                name: "OgImageUrl",
                table: "ArchObjects");

            migrationBuilder.DropColumn(
                name: "SeoKeywords",
                table: "ArchObjects");
        }
    }
}
