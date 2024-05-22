using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HogentVmPortal.Migrations
{
    /// <inheritdoc />
    public partial class Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseVirtualMachineTemplate_Template_TemplatesId",
                table: "CourseVirtualMachineTemplate");

            migrationBuilder.DropForeignKey(
                name: "FK_VirtualMachine_Template_TemplateId",
                table: "VirtualMachine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Template",
                table: "Template");

            migrationBuilder.RenameTable(
                name: "Template",
                newName: "VirtualMachineTemplate");

            migrationBuilder.RenameColumn(
                name: "TemplatesId",
                table: "CourseVirtualMachineTemplate",
                newName: "VirtualMachineTemplatesId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseVirtualMachineTemplate_TemplatesId",
                table: "CourseVirtualMachineTemplate",
                newName: "IX_CourseVirtualMachineTemplate_VirtualMachineTemplatesId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VirtualMachineTemplate",
                table: "VirtualMachineTemplate",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ContainerCreateRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OwnerId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SshKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CloneId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerCreateRequest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContainerEditRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SshKey = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerEditRequest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContainerRemoveRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerRemoveRequest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContainerTemplate",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProxmoxId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerTemplate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Container",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProxmoxId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OwnerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Container", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Container_ContainerTemplate_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "ContainerTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Container_HogentUser_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "HogentUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContainerTemplateCourse",
                columns: table => new
                {
                    ContainerTemplatesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CoursesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerTemplateCourse", x => new { x.ContainerTemplatesId, x.CoursesId });
                    table.ForeignKey(
                        name: "FK_ContainerTemplateCourse_ContainerTemplate_ContainerTemplatesId",
                        column: x => x.ContainerTemplatesId,
                        principalTable: "ContainerTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContainerTemplateCourse_Course_CoursesId",
                        column: x => x.CoursesId,
                        principalTable: "Course",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Container_OwnerId",
                table: "Container",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Container_TemplateId",
                table: "Container",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Name",
                table: "Container",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Name",
                table: "ContainerTemplate",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ContainerTemplateCourse_CoursesId",
                table: "ContainerTemplateCourse",
                column: "CoursesId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseVirtualMachineTemplate_VirtualMachineTemplate_VirtualMachineTemplatesId",
                table: "CourseVirtualMachineTemplate",
                column: "VirtualMachineTemplatesId",
                principalTable: "VirtualMachineTemplate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VirtualMachine_VirtualMachineTemplate_TemplateId",
                table: "VirtualMachine",
                column: "TemplateId",
                principalTable: "VirtualMachineTemplate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseVirtualMachineTemplate_VirtualMachineTemplate_VirtualMachineTemplatesId",
                table: "CourseVirtualMachineTemplate");

            migrationBuilder.DropForeignKey(
                name: "FK_VirtualMachine_VirtualMachineTemplate_TemplateId",
                table: "VirtualMachine");

            migrationBuilder.DropTable(
                name: "Container");

            migrationBuilder.DropTable(
                name: "ContainerCreateRequest");

            migrationBuilder.DropTable(
                name: "ContainerEditRequest");

            migrationBuilder.DropTable(
                name: "ContainerRemoveRequest");

            migrationBuilder.DropTable(
                name: "ContainerTemplateCourse");

            migrationBuilder.DropTable(
                name: "ContainerTemplate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VirtualMachineTemplate",
                table: "VirtualMachineTemplate");

            migrationBuilder.RenameTable(
                name: "VirtualMachineTemplate",
                newName: "Template");

            migrationBuilder.RenameColumn(
                name: "VirtualMachineTemplatesId",
                table: "CourseVirtualMachineTemplate",
                newName: "TemplatesId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseVirtualMachineTemplate_VirtualMachineTemplatesId",
                table: "CourseVirtualMachineTemplate",
                newName: "IX_CourseVirtualMachineTemplate_TemplatesId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Template",
                table: "Template",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseVirtualMachineTemplate_Template_TemplatesId",
                table: "CourseVirtualMachineTemplate",
                column: "TemplatesId",
                principalTable: "Template",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VirtualMachine_Template_TemplateId",
                table: "VirtualMachine",
                column: "TemplateId",
                principalTable: "Template",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
