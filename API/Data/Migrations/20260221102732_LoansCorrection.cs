using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Data.Migrations
{
    /// <inheritdoc />
    public partial class LoansCorrection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserBookId",
                table: "Loans",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Loans_UserBookId",
                table: "Loans",
                column: "UserBookId");

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_UserBooks_UserBookId",
                table: "Loans",
                column: "UserBookId",
                principalTable: "UserBooks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_UserBooks_UserBookId",
                table: "Loans");

            migrationBuilder.DropIndex(
                name: "IX_Loans_UserBookId",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "UserBookId",
                table: "Loans");
        }
    }
}
