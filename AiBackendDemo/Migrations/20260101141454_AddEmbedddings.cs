using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiBackendDemo.Migrations
{
    /// <inheritdoc />
    public partial class AddEmbedddings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float[]>(
                name: "Embedding",
                table: "Actions",
                type: "real[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Embedding",
                table: "Actions");
        }
    }
}
