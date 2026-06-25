using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenFinancialExchange.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryKeywords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Keywords",
                table: "Categories",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1L,
                column: "Keywords",
                value: "PANIFICADORA\nPADARIA\nRESTAURANTE\nSUPERMERCADO\nMERCADO\nHORTIFRUTI\nACOUGUE\nLANCHONETE\nIFOOD\nRAPPI");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2L,
                column: "Keywords",
                value: "POSTO\nCOMBUSTIVEL\nGASOLINA\nESTACIONAMENTO\nUBER\n99 TECNOLOGIA\n99POP\nPEDAGIO\nONIBUS\nMETRO");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3L,
                column: "Keywords",
                value: "ALUGUEL\nCONDOMINIO\nENERGIA\nENEL\nCEMIG\nSABESP\nAGUA\nINTERNET\nVIVO FIBRA\nCLARO NET");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4L,
                column: "Keywords",
                value: "FARMACIA\nDROGARIA\nDROGASIL\nRAIA\nPACHECO\nHOSPITAL\nCLINICA\nLABORATORIO\nUNIMED\nDENTISTA");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5L,
                column: "Keywords",
                value: "ESCOLA\nCOLEGIO\nFACULDADE\nUNIVERSIDADE\nCURSO\nUDEMY\nALURA\nLIVRARIA");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6L,
                column: "Keywords",
                value: "CINEMA\nNETFLIX\nSPOTIFY\nDISNEY\nHBO\nAMAZON PRIME\nINGRESSO\nTEATRO\nSTEAM");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7L,
                column: "Keywords",
                value: "MAGAZINE\nMAGALU\nAMERICANAS\nMERCADO LIVRE\nMERCADOLIVRE\nAMAZON\nSHOPEE\nALIEXPRESS\nSHOPPING\nRENNER\nRIACHUELO");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8L,
                column: "Keywords",
                value: "BARBEARIA\nSALAO\nLAVANDERIA\nASSINATURA");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 9L,
                column: "Keywords",
                value: "SALARIO\nFOLHA PGTO\nFOLHA DE PAGAMENTO\nPROVENTOS\nREMUNERACAO\nORDENADO");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 10L,
                column: "Keywords",
                value: "RESGATE\nAPLICACAO\nAPLICACAO INV\nINV FAC\nINVEST FACIL\nINVESTIMENTO FACIL\nCDB\nTESOURO DIRETO\nFUNDO");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 11L,
                column: "Keywords",
                value: "");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 12L,
                column: "Keywords",
                value: "TARIFA\nCESTA\nANUIDADE\nIOF\nPACOTE SERVICOS\nMANUTENCAO CONTA");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 13L,
                column: "Keywords",
                value: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Keywords",
                table: "Categories");
        }
    }
}
