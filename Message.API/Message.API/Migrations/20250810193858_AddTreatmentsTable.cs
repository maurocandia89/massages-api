using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Message.API.Migrations;

public partial class AddTreatmentsTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "EndTime", table: "Appointments");

        migrationBuilder.DropColumn(name: "Title", table: "Appointments");

        migrationBuilder.RenameColumn(
            name: "StartTime",
            table: "Appointments",
            newName: "AppointmentDate"
        );

        migrationBuilder.AddColumn<Guid>(
            name: "TreatmentId",
            table: "Appointments",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
        );

        migrationBuilder.CreateTable(
            name: "Treatments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Title = table.Column<string>(type: "text", nullable: false),
                Description = table.Column<string>(type: "text", nullable: false),
                CreatedAt = table.Column<DateTime>(
                    type: "timestamp with time zone",
                    nullable: false
                ),
                ModifiedAt = table.Column<DateTime>(
                    type: "timestamp with time zone",
                    nullable: true
                ),
                IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Treatments", x => x.Id);
            }
        );

        // Código para solucionar el error de la llave foránea
        // 1. Define un ID para el tratamiento por defecto.
        var defaultTreatmentId = Guid.NewGuid();

        // 2. Inserta el tratamiento por defecto en la nueva tabla 'Treatments'.
        migrationBuilder.InsertData(
            table: "Treatments",
            columns: new[] { "Id", "CreatedAt", "Title", "Description", "IsEnabled" },
            values: new object[]
            {
                defaultTreatmentId,
                DateTime.UtcNow,
                "Tratamiento por defecto",
                "Tratamiento asignado a los turnos preexistentes.",
                true,
            }
        );

        // 3. Actualiza todos los 'Appointments' existentes para que apunten a este nuevo tratamiento.
        migrationBuilder.Sql(
            $"UPDATE \"Appointments\" SET \"TreatmentId\" = '{defaultTreatmentId}' WHERE \"TreatmentId\" = '00000000-0000-0000-0000-000000000000';"
        );

        migrationBuilder.CreateIndex(
            name: "IX_Appointments_TreatmentId",
            table: "Appointments",
            column: "TreatmentId"
        );

        migrationBuilder.AddForeignKey(
            name: "FK_Appointments_Treatments_TreatmentId",
            table: "Appointments",
            column: "TreatmentId",
            principalTable: "Treatments",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Appointments_Treatments_TreatmentId",
            table: "Appointments"
        );

        migrationBuilder.DropTable(name: "Treatments");

        migrationBuilder.DropIndex(name: "IX_Appointments_TreatmentId", table: "Appointments");

        migrationBuilder.DropColumn(name: "TreatmentId", table: "Appointments");

        migrationBuilder.RenameColumn(
            name: "AppointmentDate",
            table: "Appointments",
            newName: "StartTime"
        );

        migrationBuilder.AddColumn<DateTime>(
            name: "EndTime",
            table: "Appointments",
            type: "timestamp with time zone",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
        );

        migrationBuilder.AddColumn<string>(
            name: "Title",
            table: "Appointments",
            type: "text",
            nullable: false,
            defaultValue: ""
        );
    }
}
