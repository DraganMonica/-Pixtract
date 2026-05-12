using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pixtract.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImagePathToExtractionRequestFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "ExtractionRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "16be7142-00a3-426a-9bee-47ce836810e5", "AQAAAAIAAYagAAAAEM/3eu2/xyArHiwuCMijHmUrtwRcWC7DKorFde/LAYGiC+z9n3JQj1gD2UUyd1x7Kg==", "aca89935-ebc6-4609-bcbc-17a5492ba0bf" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "ExtractionRequests");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "e7fc8347-b16b-4e64-9c6c-555968dc6ca9", "AQAAAAIAAYagAAAAENi8VyffTw31E96hTmRDSqMnYO1nfoUIdxfWZbiX2t/ZDihABYUfwlyX/sUSJwj4Eg==", "66fa999f-a827-4a0c-ad77-cdac1139827c" });
        }
    }
}
