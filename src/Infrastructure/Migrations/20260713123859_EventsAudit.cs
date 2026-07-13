using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsManager.Infrastructure.Migrations
{
    /// <summary>
    /// Migration écrite à la main — le modèle EF ne connaît pas ces colonnes et le snapshot
    /// reste volontairement inchangé (elles ne seront jamais touchées par une migration générée).
    /// Colonnes d'audit DB-only de app.Events : AddedBy/AddedDate remplies à l'insertion par
    /// contraintes DEFAULT ; UpdatedBy/UpdatedDate remplies par le trigger AFTER UPDATE,
    /// déclaré côté EF via HasTrigger (cf. EventConfiguration).
    /// </summary>
    public partial class EventsAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddedBy",
                schema: "app",
                table: "Events",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValueSql: "SUSER_SNAME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "AddedDate",
                schema: "app",
                table: "Events",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "app",
                table: "Events",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                schema: "app",
                table: "Events",
                type: "datetime2",
                nullable: true);

            migrationBuilder.Sql(
                """
                CREATE TRIGGER app.TR_Events_Update_Audit
                ON app.Events
                AFTER UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    -- Ceinture-bretelles : si RECURSIVE_TRIGGERS était activé un jour,
                    -- l'UPDATE ci-dessous ne doit pas relancer ce trigger en boucle.
                    IF TRIGGER_NESTLEVEL(@@PROCID) > 1
                        RETURN;

                    UPDATE e
                    SET UpdatedBy = SUSER_SNAME(),
                        UpdatedDate = SYSUTCDATETIME()
                    FROM app.Events AS e
                    INNER JOIN inserted AS i
                        ON i.Id = e.Id;
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER app.TR_Events_Update_Audit;");

            migrationBuilder.DropColumn(name: "UpdatedDate", schema: "app", table: "Events");
            migrationBuilder.DropColumn(name: "UpdatedBy", schema: "app", table: "Events");
            migrationBuilder.DropColumn(name: "AddedDate", schema: "app", table: "Events");
            migrationBuilder.DropColumn(name: "AddedBy", schema: "app", table: "Events");
        }
    }
}
