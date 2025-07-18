﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Login.Migrations
{
    /// <inheritdoc />
    public partial class intialse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Photo",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Photo",
                table: "Employees");
        }
    }
}
