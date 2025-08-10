using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Message.API.Migrations;

public partial class AppointmentsAndIdentityUpdates : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
            table: "AspNetUserRoles"
        );

        migrationBuilder.DropForeignKey(
            name: "FK_AspNetUserRoles_AspNetUsers_UserId",
            table: "AspNetUserRoles"
        );

        migrationBuilder.DropForeignKey(
            name: "FK_AspNetUserClaims_AspNetUsers_UserId",
            table: "AspNetUserClaims"
        );

        migrationBuilder.DropForeignKey(
            name: "FK_AspNetUserLogins_AspNetUsers_UserId",
            table: "AspNetUserLogins"
        );

        migrationBuilder.DropForeignKey(
            name: "FK_AspNetUserTokens_AspNetUsers_UserId",
            table: "AspNetUserTokens"
        );

        migrationBuilder.DropForeignKey(
            name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
            table: "AspNetRoleClaims"
        );

        migrationBuilder.Sql(
            "ALTER TABLE \"AspNetUserRoles\" ALTER COLUMN \"RoleId\" TYPE uuid USING \"RoleId\"::uuid;"
        );
        migrationBuilder.Sql(
            "ALTER TABLE \"AspNetUserRoles\" ALTER COLUMN \"UserId\" TYPE uuid USING \"UserId\"::uuid;"
        );

        migrationBuilder.Sql(
            "ALTER TABLE \"AspNetUserClaims\" ALTER COLUMN \"UserId\" TYPE uuid USING \"UserId\"::uuid;"
        );

        migrationBuilder.Sql(
            "ALTER TABLE \"AspNetUserLogins\" ALTER COLUMN \"UserId\" TYPE uuid USING \"UserId\"::uuid;"
        );

        migrationBuilder.Sql(
            "ALTER TABLE \"AspNetUserTokens\" ALTER COLUMN \"UserId\" TYPE uuid USING \"UserId\"::uuid;"
        );

        migrationBuilder.Sql(
            "ALTER TABLE \"AspNetRoles\" ALTER COLUMN \"Id\" TYPE uuid USING \"Id\"::uuid;"
        );

        migrationBuilder.Sql(
            "ALTER TABLE \"AspNetRoleClaims\" ALTER COLUMN \"RoleId\" TYPE uuid USING \"RoleId\"::uuid;"
        );

        migrationBuilder.Sql(
            "ALTER TABLE \"AspNetUsers\" ALTER COLUMN \"Id\" TYPE uuid USING \"Id\"::uuid;"
        );

        migrationBuilder.AddForeignKey(
            name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
            table: "AspNetUserRoles",
            column: "RoleId",
            principalTable: "AspNetRoles",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
        );

        migrationBuilder.AddForeignKey(
            name: "FK_AspNetUserRoles_AspNetUsers_UserId",
            table: "AspNetUserRoles",
            column: "UserId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
        );

        migrationBuilder.AddForeignKey(
            name: "FK_AspNetUserClaims_AspNetUsers_UserId",
            table: "AspNetUserClaims",
            column: "UserId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
        );

        migrationBuilder.AddForeignKey(
            name: "FK_AspNetUserLogins_AspNetUsers_UserId",
            table: "AspNetUserLogins",
            column: "UserId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
        );

        migrationBuilder.AddForeignKey(
            name: "FK_AspNetUserTokens_AspNetUsers_UserId",
            table: "AspNetUserTokens",
            column: "UserId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
        );

        migrationBuilder.AddForeignKey(
            name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
            table: "AspNetRoleClaims",
            column: "RoleId",
            principalTable: "AspNetRoles",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
        );

        migrationBuilder.AddColumn<DateTime>(
            name: "BirthDate",
            table: "AspNetUsers",
            type: "timestamp with time zone",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
        );

        migrationBuilder.CreateTable(
            name: "Appointments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Title = table.Column<string>(type: "text", nullable: false),
                Description = table.Column<string>(type: "text", nullable: false),
                StartTime = table.Column<DateTime>(
                    type: "timestamp with time zone",
                    nullable: false
                ),
                EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ClientId = table.Column<Guid>(type: "uuid", nullable: false),
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
                table.PrimaryKey("PK_Appointments", x => x.Id);
                table.ForeignKey(
                    name: "FK_Appointments_AspNetUsers_ClientId",
                    column: x => x.ClientId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateIndex(
            name: "IX_Appointments_ClientId",
            table: "Appointments",
            column: "ClientId"
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Revertir los cambios en el orden inverso.
        migrationBuilder.DropTable(name: "Appointments");

        migrationBuilder.DropColumn(name: "BirthDate", table: "AspNetUsers");

        // Revertir las columnas de uuid a text, eliminando y recreando las FKs.
        migrationBuilder.DropForeignKey(
            name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
            table: "AspNetUserRoles"
        );

        migrationBuilder.DropForeignKey(
            name: "FK_AspNetUserRoles_AspNetUsers_UserId",
            table: "AspNetUserRoles"
        );

        migrationBuilder.Sql(
            "ALTER TABLE \"AspNetUserRoles\" ALTER COLUMN \"RoleId\" TYPE text USING \"RoleId\"::text;"
        );
        migrationBuilder.Sql(
            "ALTER TABLE \"AspNetUserRoles\" ALTER COLUMN \"UserId\" TYPE text USING \"UserId\"::text;"
        );

        migrationBuilder.AddForeignKey(
            name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
            table: "AspNetUserRoles",
            column: "RoleId",
            principalTable: "AspNetRoles",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
        );

        migrationBuilder.AddForeignKey(
            name: "FK_AspNetUserRoles_AspNetUsers_UserId",
            table: "AspNetUserRoles",
            column: "UserId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
        );

        // Revertir las columnas de uuid a text para las otras tablas de Identity.
        migrationBuilder.DropForeignKey(
            name: "FK_AspNetUserClaims_AspNetUsers_UserId",
            table: "AspNetUserClaims"
        );

        migrationBuilder.DropForeignKey(
            name: "FK_AspNetUserLogins_AspNetUsers_UserId",
            table: "AspNetUserLogins"
        );

        migrationBuilder.DropForeignKey(
            name: "FK_AspNetUserTokens_AspNetUsers_UserId",
            table: "AspNetUserTokens"
        );

        migrationBuilder.DropForeignKey(
            name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
            table: "AspNetRoleClaims"
        );

        migrationBuilder.Sql(
            "ALTER TABLE \"AspNetUserClaims\" ALTER COLUMN \"UserId\" TYPE text USING \"UserId\"::text;"
        );
        migrationBuilder.Sql(
            "ALTER TABLE \"AspNetUserLogins\" ALTER COLUMN \"UserId\" TYPE text USING \"UserId\"::text;"
        );
        migrationBuilder.Sql(
            "ALTER TABLE \"AspNetUserTokens\" ALTER COLUMN \"UserId\" TYPE text USING \"UserId\"::text;"
        );

        migrationBuilder.AddForeignKey(
            name: "FK_AspNetUserClaims_AspNetUsers_UserId",
            table: "AspNetUserClaims",
            column: "UserId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
        );

        migrationBuilder.AddForeignKey(
            name: "FK_AspNetUserLogins_AspNetUsers_UserId",
            table: "AspNetUserLogins",
            column: "UserId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
        );

        migrationBuilder.AddForeignKey(
            name: "FK_AspNetUserTokens_AspNetUsers_UserId",
            table: "AspNetUserTokens",
            column: "UserId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
        );

        migrationBuilder.AddForeignKey(
            name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
            table: "AspNetRoleClaims",
            column: "RoleId",
            principalTable: "AspNetRoles",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
        );
    }
}
