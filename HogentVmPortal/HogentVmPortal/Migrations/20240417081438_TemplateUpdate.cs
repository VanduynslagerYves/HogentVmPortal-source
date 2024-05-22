using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HogentVmPortal.Migrations
{
    /// <inheritdoc />
    public partial class TemplateUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseTemplate");

            migrationBuilder.CreateTable(
                name: "CourseVirtualMachineTemplate",
                columns: table => new
                {
                    CoursesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplatesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseVirtualMachineTemplate", x => new { x.CoursesId, x.TemplatesId });
                    table.ForeignKey(
                        name: "FK_CourseVirtualMachineTemplate_Course_CoursesId",
                        column: x => x.CoursesId,
                        principalTable: "Course",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseVirtualMachineTemplate_Template_TemplatesId",
                        column: x => x.TemplatesId,
                        principalTable: "Template",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseVirtualMachineTemplate_TemplatesId",
                table: "CourseVirtualMachineTemplate",
                column: "TemplatesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseVirtualMachineTemplate");

            migrationBuilder.CreateTable(
                name: "CourseTemplate",
                columns: table => new
                {
                    CoursesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplatesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseTemplate", x => new { x.CoursesId, x.TemplatesId });
                    table.ForeignKey(
                        name: "FK_CourseTemplate_Course_CoursesId",
                        column: x => x.CoursesId,
                        principalTable: "Course",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseTemplate_Template_TemplatesId",
                        column: x => x.TemplatesId,
                        principalTable: "Template",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseTemplate_TemplatesId",
                table: "CourseTemplate",
                column: "TemplatesId");
        }
    }
}
