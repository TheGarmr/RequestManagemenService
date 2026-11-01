using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RequestManagementService.DataAccess.Migrations
{
    public partial class AddServiceRequestEvents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "service_request_events",
                columns: table => new
                {
                    message_id = table.Column<long>(type: "bigint", nullable: false),
                    service_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    old_status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    new_status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    priority = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    received_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_request_events", x => x.message_id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "service_request_events");
        }
    }
}
