using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.JuniperConnector.Data.Migrations
{
    public partial class AddParentCodeInZones : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AreaType",
                table: "Zones",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ParentCode",
                table: "Zones",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AreaType",
                table: "Zones");

            migrationBuilder.DropColumn(
                name: "ParentCode",
                table: "Zones");
        }
    }
}
