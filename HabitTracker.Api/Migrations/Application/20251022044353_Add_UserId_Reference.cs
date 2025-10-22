﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HabitTracker.Api.Migrations.Application
{
    /// <inheritdoc />
    public partial class Add_UserId_Reference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM dev_habit.HabitTags;
                DELETE FROM dev_habit.Habits;
                DELETE FROM dev_habit.Tags;
                """);

            migrationBuilder.DropIndex(
                name: "IX_Tags_Name",
                schema: "dev_habit",
                table: "Tags");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                schema: "dev_habit",
                table: "Tags",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                schema: "dev_habit",
                table: "Habits",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_UserId_Name",
                schema: "dev_habit",
                table: "Tags",
                columns: new[] { "UserId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Habits_UserId",
                schema: "dev_habit",
                table: "Habits",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Habits_Users_UserId",
                schema: "dev_habit",
                table: "Habits",
                column: "UserId",
                principalSchema: "dev_habit",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Users_UserId",
                schema: "dev_habit",
                table: "Tags",
                column: "UserId",
                principalSchema: "dev_habit",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Habits_Users_UserId",
                schema: "dev_habit",
                table: "Habits");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Users_UserId",
                schema: "dev_habit",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_UserId_Name",
                schema: "dev_habit",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Habits_UserId",
                schema: "dev_habit",
                table: "Habits");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "dev_habit",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "dev_habit",
                table: "Habits");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                schema: "dev_habit",
                table: "Tags",
                column: "Name",
                unique: true);
        }
    }
}
