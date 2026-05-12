using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pixtract.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanExpiresAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PlanExpiresAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "PlanExpiresAt", "SecurityStamp" },
                values: new object[] { "5407321a-b44c-40b7-9327-a760743b342c", "AQAAAAIAAYagAAAAEPqEt+q/HJajn4EQY/kWKo4PG5VG9kMqCHtdTlSYIMlf29nO0ryX5PEZOeiHISohtA==", null, "b56055a3-24e4-458e-bdf7-816b8ae0709e" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlanExpiresAt",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "15b7431d-ea2b-46d6-8ab7-28d858070e23", "AQAAAAIAAYagAAAAELZznERRldW6fCN0RuuiNOMnzydAc9CPGbgWEPThIpfmWjq5Dner004L9XceR3aaDg==", "b0f82da1-85ba-4888-baad-d1b111fa04f1" });
        }
    }
}
