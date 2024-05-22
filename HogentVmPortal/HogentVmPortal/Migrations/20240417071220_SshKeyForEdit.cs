using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HogentVmPortal.Migrations
{
    /// <inheritdoc />
    public partial class SshKeyForEdit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SshKey",
                table: "VirtualMachineEditRequest",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SshKey",
                table: "VirtualMachineEditRequest");
        }
    }
}
