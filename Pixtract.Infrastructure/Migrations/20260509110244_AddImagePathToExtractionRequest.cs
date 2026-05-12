using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pixtract.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImagePathToExtractionRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "15b7431d-ea2b-46d6-8ab7-28d858070e23", "AQAAAAIAAYagAAAAELZznERRldW6fCN0RuuiNOMnzydAc9CPGbgWEPThIpfmWjq5Dner004L9XceR3aaDg==", "b0f82da1-85ba-4888-baad-d1b111fa04f1" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "16be7142-00a3-426a-9bee-47ce836810e5", "AQAAAAIAAYagAAAAEM/3eu2/xyArHiwuCMijHmUrtwRcWC7DKorFde/LAYGiC+z9n3JQj1gD2UUyd1x7Kg==", "aca89935-ebc6-4609-bcbc-17a5492ba0bf" });
        }
    }
}
