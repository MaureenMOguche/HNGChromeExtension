using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChromeExtension.Migrations
{
    /// <inheritdoc />
    public partial class jobId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JobId",
                table: "VideoDatas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VideoTranscription",
                table: "VideoDatas",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JobId",
                table: "VideoDatas");

            migrationBuilder.DropColumn(
                name: "VideoTranscription",
                table: "VideoDatas");
        }
    }
}
