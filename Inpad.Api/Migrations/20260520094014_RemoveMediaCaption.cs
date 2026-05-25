using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inpad.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMediaCaption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Caption",
                table: "ObjectMedias");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Caption",
                table: "ObjectMedias",
                type: "text",
                nullable: true);
        }
    }
}
