using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HogentVmPortal.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEdit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContainerEditRequest");

            migrationBuilder.DropTable(
                name: "VirtualMachineEditRequest");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "ContainerCreateRequest");

            migrationBuilder.DropColumn(
                name: "SshKey",
                table: "ContainerCreateRequest");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "ContainerCreateRequest",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SshKey",
                table: "ContainerCreateRequest",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ContainerEditRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SshKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerEditRequest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VirtualMachineEditRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SshKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VirtualMachineEditRequest", x => x.Id);
                });
        }
    }
}
