using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pixtract.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBatchFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BatchLimit",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "CanBatch",
                table: "Plans");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "41787c14-cd21-47cb-ad2b-586b7ceecc3a", "AQAAAAIAAYagAAAAEEzfk6FZ7FQAoPSP4uzZjiw7yZNB8HiMVXMsEyFfnWzF+MFdVoUdZYVoeY3fbsp/JQ==", "65d73cfd-c5e4-40ef-a3d9-da32f2d9d99c" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BatchLimit",
                table: "Plans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "CanBatch",
                table: "Plans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "5407321a-b44c-40b7-9327-a760743b342c", "AQAAAAIAAYagAAAAEPqEt+q/HJajn4EQY/kWKo4PG5VG9kMqCHtdTlSYIMlf29nO0ryX5PEZOeiHISohtA==", "b56055a3-24e4-458e-bdf7-816b8ae0709e" });

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BatchLimit", "CanBatch" },
                values: new object[] { 0, false });

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BatchLimit", "CanBatch" },
                values: new object[] { 10, true });

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "BatchLimit", "CanBatch" },
                values: new object[] { 50, true });
        }
    }
}
