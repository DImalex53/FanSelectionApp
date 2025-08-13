using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BladesCalc.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AerodynamicsDataBlades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    TypeOfBlades = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: true),
                    TypeOfBladesKod = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false),
                    Scheme = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: true),
                    StaticPressure1 = table.Column<double>(type: "float", nullable: false),
                    StaticPressure2 = table.Column<double>(type: "float", nullable: false),
                    StaticPressure3 = table.Column<double>(type: "float", nullable: false),
                    Efficiency1 = table.Column<double>(type: "float", nullable: false),
                    Efficiency2 = table.Column<double>(type: "float", nullable: false),
                    Efficiency3 = table.Column<double>(type: "float", nullable: false),
                    Efficiency4 = table.Column<double>(type: "float", nullable: false),
                    OutletLength = table.Column<double>(type: "float", nullable: false),
                    OutletWidth = table.Column<double>(type: "float", nullable: false),
                    MinDeltaEfficiency = table.Column<double>(type: "float", nullable: false),
                    MaxDeltaEfficiency = table.Column<double>(type: "float", nullable: false),
                    NewMarkOfFan = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: true),
                    BladeLength = table.Column<double>(type: "float", nullable: false),
                    BladeWidth = table.Column<double>(type: "float", nullable: false),
                    ImpellerWidth = table.Column<double>(type: "float", nullable: false),
                    ImpellerInletDiameter = table.Column<double>(type: "float", nullable: false),
                    NumberOfBlades = table.Column<int>(type: "int", nullable: false),
                    NewMarkOfFand = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AerodynamicsDataBlades", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AerodynamicsDataBlades_TypeOfBlades",
                table: "AerodynamicsDataBlades",
                column: "TypeOfBlades");

            migrationBuilder.CreateIndex(
                name: "IX_AerodynamicsDataBlades_TypeOfBladesKod",
                table: "AerodynamicsDataBlades",
                column: "TypeOfBladesKod");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AerodynamicsDataBlades");
        }
    }
}
