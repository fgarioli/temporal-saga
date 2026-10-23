using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CadastroService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "envios",
                columns: table => new
                {
                    usuario_id = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    enviado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_envios", x => x.usuario_id);
                });

            migrationBuilder.CreateTable(
                name: "simulated_failures",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    remaining = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_simulated_failures", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    habilitado = table.Column<bool>(type: "boolean", nullable: false),
                    etapa = table.Column<string>(type: "text", nullable: false),
                    historico = table.Column<string>(type: "jsonb", nullable: false),
                    motivo = table.Column<string>(type: "text", nullable: true),
                    keycloak_id = table.Column<string>(type: "text", nullable: true),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_etapa",
                table: "usuarios",
                column: "etapa");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_username",
                table: "usuarios",
                column: "username",
                unique: true,
                filter: "etapa <> 'Cancelado'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "envios");

            migrationBuilder.DropTable(
                name: "simulated_failures");

            migrationBuilder.DropTable(
                name: "usuarios");
        }
    }
}
