using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenFinancialExchange.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinancialInstitutions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OrgName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Fid = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialInstitutions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OfxImports",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OfxHeaderVersion = table.Column<short>(type: "smallint", nullable: true),
                    OfxVersion = table.Column<short>(type: "smallint", nullable: true),
                    OfxData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Encoding = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Charset = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Security = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Compression = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    OldFileUid = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NewFileUid = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ImportedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfxImports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BankAccounts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FinancialInstitutionId = table.Column<long>(type: "bigint", nullable: false),
                    BankId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BranchId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AcctId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AcctType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankAccounts_FinancialInstitutions",
                        column: x => x.FinancialInstitutionId,
                        principalTable: "FinancialInstitutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OfxStatements",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImportId = table.Column<long>(type: "bigint", nullable: false),
                    BankAccountId = table.Column<long>(type: "bigint", nullable: false),
                    TrnUid = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true),
                    CurDef = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    DtServer = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Language = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    StatusCode = table.Column<short>(type: "smallint", nullable: true),
                    StatusSeverity = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DtStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DtEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfxStatements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfxStatements_BankAccounts",
                        column: x => x.BankAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OfxStatements_OfxImports",
                        column: x => x.ImportId,
                        principalTable: "OfxImports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OfxBalances",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatementId = table.Column<long>(type: "bigint", nullable: false),
                    BalanceType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BalAmt = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DtAsOf = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfxBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfxBalances_OfxStatements",
                        column: x => x.StatementId,
                        principalTable: "OfxStatements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OfxTransactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatementId = table.Column<long>(type: "bigint", nullable: false),
                    TrnType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DtPosted = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrnAmt = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FitId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Memo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CheckNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfxTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfxTransactions_OfxStatements",
                        column: x => x.StatementId,
                        principalTable: "OfxStatements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_FinancialInstitutionId",
                table: "BankAccounts",
                column: "FinancialInstitutionId");

            migrationBuilder.CreateIndex(
                name: "UQ_BankAccounts_BankId_BranchId_AcctId",
                table: "BankAccounts",
                columns: new[] { "BankId", "BranchId", "AcctId" },
                unique: true,
                filter: "[BranchId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_FinancialInstitutions_BankId_Fid",
                table: "FinancialInstitutions",
                columns: new[] { "BankId", "Fid" },
                unique: true,
                filter: "[Fid] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OfxBalances_StatementId",
                table: "OfxBalances",
                column: "StatementId");

            migrationBuilder.CreateIndex(
                name: "UQ_OfxImports_FileHash",
                table: "OfxImports",
                column: "FileHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OfxStatements_BankAccountId",
                table: "OfxStatements",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_OfxStatements_ImportId",
                table: "OfxStatements",
                column: "ImportId");

            migrationBuilder.CreateIndex(
                name: "IX_OfxTransactions_DtPosted_TrnType",
                table: "OfxTransactions",
                columns: new[] { "DtPosted", "TrnType" });

            migrationBuilder.CreateIndex(
                name: "UIX_OfxTransactions_StatementId_FitId",
                table: "OfxTransactions",
                columns: new[] { "StatementId", "FitId" },
                unique: true,
                filter: "[FitId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OfxBalances");

            migrationBuilder.DropTable(
                name: "OfxTransactions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "OfxStatements");

            migrationBuilder.DropTable(
                name: "BankAccounts");

            migrationBuilder.DropTable(
                name: "OfxImports");

            migrationBuilder.DropTable(
                name: "FinancialInstitutions");
        }
    }
}
