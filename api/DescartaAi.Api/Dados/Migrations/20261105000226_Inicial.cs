using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DescartaAi.Api.Dados.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "categoria",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    icone = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    cor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categoria", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    email = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    senha_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    papel = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuario", x => x.id);
                    table.CheckConstraint("ck_usuario_papel", "papel IN ('Parceiro','Admin')");
                });

            migrationBuilder.CreateTable(
                name: "material",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    categoria_id = table.Column<int>(type: "integer", nullable: false),
                    nome = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    instrucoes = table.Column<string>(type: "text", nullable: false),
                    cuidados = table.Column<string>(type: "text", nullable: true),
                    sinonimos = table.Column<string[]>(type: "varchar(60)[]", nullable: true),
                    perigoso = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_material", x => x.id);
                    table.UniqueConstraint("uq_material_nome", x => x.nome);
                    table.ForeignKey(
                        name: "fk_material_categoria_categoria_id",
                        column: x => x.categoria_id,
                        principalTable: "categoria",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "parceiro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    documento = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: true),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parceiro", x => x.id);
                    table.ForeignKey(
                        name: "fk_parceiro_usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identificacao",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    material_sugerido_id = table.Column<int>(type: "integer", nullable: true),
                    material_confirmado_id = table.Column<int>(type: "integer", nullable: true),
                    confianca = table.Column<decimal>(type: "numeric(4,3)", precision: 4, scale: 3, nullable: true),
                    corrigido = table.Column<bool>(type: "boolean", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identificacao", x => x.id);
                    table.ForeignKey(
                        name: "fk_identificacao_material_material_confirmado_id",
                        column: x => x.material_confirmado_id,
                        principalTable: "material",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_identificacao_material_material_sugerido_id",
                        column: x => x.material_sugerido_id,
                        principalTable: "material",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ponto_descarte",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    parceiro_id = table.Column<Guid>(type: "uuid", nullable: true),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    endereco = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cidade = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    uf = table.Column<string>(type: "char(2)", nullable: false),
                    cep = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    localizacao = table.Column<Point>(type: "geography(Point,4326)", nullable: false),
                    telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    horario = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    aprovado_por = table.Column<Guid>(type: "uuid", nullable: true),
                    aprovado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ponto_descarte", x => x.id);
                    table.CheckConstraint("ck_ponto_status", "status IN ('Pendente','Aprovado','Rejeitado','Inativo')");
                    table.ForeignKey(
                        name: "fk_ponto_descarte_parceiro_parceiro_id",
                        column: x => x.parceiro_id,
                        principalTable: "parceiro",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_ponto_descarte_usuario_aprovado_por",
                        column: x => x.aprovado_por,
                        principalTable: "usuario",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ponto_material",
                columns: table => new
                {
                    ponto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    material_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ponto_material", x => new { x.ponto_id, x.material_id });
                    table.ForeignKey(
                        name: "fk_ponto_material_material_material_id",
                        column: x => x.material_id,
                        principalTable: "material",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_ponto_material_ponto_descarte_ponto_id",
                        column: x => x.ponto_id,
                        principalTable: "ponto_descarte",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reporte",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ponto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    motivo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    comentario = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reporte", x => x.id);
                    table.CheckConstraint("ck_reporte_motivo", "motivo IN ('Fechado','EnderecoErrado','NaoAceitaMaterial','Outro')");
                    table.CheckConstraint("ck_reporte_status", "status IN ('Pendente','Resolvido','Descartado')");
                    table.ForeignKey(
                        name: "fk_reporte_ponto_descarte_ponto_id",
                        column: x => x.ponto_id,
                        principalTable: "ponto_descarte",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_categoria_nome",
                table: "categoria",
                column: "nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_identificacao_material_confirmado_id",
                table: "identificacao",
                column: "material_confirmado_id");

            migrationBuilder.CreateIndex(
                name: "ix_identificacao_material_sugerido_id",
                table: "identificacao",
                column: "material_sugerido_id");

            migrationBuilder.CreateIndex(
                name: "ix_material_categoria_id",
                table: "material",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "ix_material_nome_trgm",
                table: "material",
                column: "nome")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_parceiro_usuario_id",
                table: "parceiro",
                column: "usuario_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ponto_descarte_aprovado_por",
                table: "ponto_descarte",
                column: "aprovado_por");

            migrationBuilder.CreateIndex(
                name: "ix_ponto_descarte_parceiro_id",
                table: "ponto_descarte",
                column: "parceiro_id");

            migrationBuilder.CreateIndex(
                name: "ix_ponto_localizacao",
                table: "ponto_descarte",
                column: "localizacao")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "ix_ponto_status",
                table: "ponto_descarte",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_ponto_material_material_id",
                table: "ponto_material",
                column: "material_id");

            migrationBuilder.CreateIndex(
                name: "ix_reporte_ponto_id",
                table: "reporte",
                column: "ponto_id");

            migrationBuilder.CreateIndex(
                name: "ix_usuario_email",
                table: "usuario",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "identificacao");

            migrationBuilder.DropTable(
                name: "ponto_material");

            migrationBuilder.DropTable(
                name: "reporte");

            migrationBuilder.DropTable(
                name: "material");

            migrationBuilder.DropTable(
                name: "ponto_descarte");

            migrationBuilder.DropTable(
                name: "categoria");

            migrationBuilder.DropTable(
                name: "parceiro");

            migrationBuilder.DropTable(
                name: "usuario");
        }
    }
}
