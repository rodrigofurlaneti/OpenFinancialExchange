using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenFinancialExchange.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBankCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BankCodes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    CodigoCompensacao = table.Column<int>(type: "int", nullable: false),
                    Cnpj = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    NomeInstituicao = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Segmento = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankCodes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "UX_BankCodes_CodigoCompensacao",
                table: "BankCodes",
                column: "CodigoCompensacao",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BankCodes");
        }
    }
}
