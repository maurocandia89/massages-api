using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Message.API.Migrations
{
    /// <inheritdoc />
    public partial class AddEstadoAndMotivoToAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Appointments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MotivoCancelacion",
                table: "Appointments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "MotivoCancelacion",
                table: "Appointments");
        }
    }
}
