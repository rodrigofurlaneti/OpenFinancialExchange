using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OpenFinancialExchange.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserScoping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OfxTransactions_DtPosted_TrnType",
                table: "OfxTransactions");

            migrationBuilder.DropIndex(
                name: "UQ_OfxImports_FileHash",
                table: "OfxImports");

            migrationBuilder.DropIndex(
                name: "UQ_FinancialInstitutions_BankId_Fid",
                table: "FinancialInstitutions");

            migrationBuilder.DropIndex(
                name: "UQ_BankAccounts_BankId_BranchId_AcctId",
                table: "BankAccounts");

            migrationBuilder.AddColumn<long>(
                name: "CategoryId",
                table: "OfxTransactions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "OfxTransactions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "OfxStatements",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "OfxImports",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "OfxBalances",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "FinancialInstitutions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "BankAccounts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            // Backfill: assign pre-existing (pre-multi-tenant) rows to the oldest user
            // so the NOT NULL UserId foreign keys below can be created. Dev data only.
            migrationBuilder.Sql(@"
DECLARE @uid bigint = (SELECT MIN([Id]) FROM [Users]);
IF @uid IS NOT NULL
BEGIN
    UPDATE [OfxTransactions]       SET [UserId] = @uid WHERE [UserId] = 0;
    UPDATE [OfxStatements]         SET [UserId] = @uid WHERE [UserId] = 0;
    UPDATE [OfxImports]            SET [UserId] = @uid WHERE [UserId] = 0;
    UPDATE [OfxBalances]           SET [UserId] = @uid WHERE [UserId] = 0;
    UPDATE [FinancialInstitutions] SET [UserId] = @uid WHERE [UserId] = 0;
    UPDATE [BankAccounts]          SET [UserId] = @uid WHERE [UserId] = 0;
END");

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Color", "CreatedAt", "IsActive", "IsSystem", "Kind", "Name", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { 1L, "#ef4444", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "DEBIT", "Alimentação", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 2L, "#f97316", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "DEBIT", "Transporte", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 3L, "#eab308", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "DEBIT", "Moradia", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 4L, "#14b8a6", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "DEBIT", "Saúde", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 5L, "#3b82f6", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "DEBIT", "Educação", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 6L, "#a855f7", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "DEBIT", "Lazer", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 7L, "#ec4899", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "DEBIT", "Compras", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 8L, "#64748b", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "DEBIT", "Serviços", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 9L, "#22c55e", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "CREDIT", "Salário", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 10L, "#10b981", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "BOTH", "Investimentos", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 11L, "#06b6d4", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "BOTH", "Transferências", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 12L, "#f43f5e", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "DEBIT", "Tarifas Bancárias", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 13L, "#94a3b8", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "BOTH", "Outros", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_OfxTransactions_CategoryId",
                table: "OfxTransactions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_OfxTransactions_User_DtPosted_TrnType",
                table: "OfxTransactions",
                columns: new[] { "UserId", "DtPosted", "TrnType" });

            migrationBuilder.CreateIndex(
                name: "IX_OfxStatements_UserId",
                table: "OfxStatements",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ_OfxImports_User_FileHash",
                table: "OfxImports",
                columns: new[] { "UserId", "FileHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OfxBalances_UserId",
                table: "OfxBalances",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ_FinancialInstitutions_User_BankId_Fid",
                table: "FinancialInstitutions",
                columns: new[] { "UserId", "BankId", "Fid" },
                unique: true,
                filter: "[Fid] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_BankAccounts_User_BankId_BranchId_AcctId",
                table: "BankAccounts",
                columns: new[] { "UserId", "BankId", "BranchId", "AcctId" },
                unique: true,
                filter: "[BranchId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_Categories_User_Name",
                table: "Categories",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_BankAccounts_Users",
                table: "BankAccounts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialInstitutions_Users",
                table: "FinancialInstitutions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OfxBalances_Users",
                table: "OfxBalances",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OfxImports_Users",
                table: "OfxImports",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OfxStatements_Users",
                table: "OfxStatements",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OfxTransactions_Categories",
                table: "OfxTransactions",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OfxTransactions_Users",
                table: "OfxTransactions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankAccounts_Users",
                table: "BankAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialInstitutions_Users",
                table: "FinancialInstitutions");

            migrationBuilder.DropForeignKey(
                name: "FK_OfxBalances_Users",
                table: "OfxBalances");

            migrationBuilder.DropForeignKey(
                name: "FK_OfxImports_Users",
                table: "OfxImports");

            migrationBuilder.DropForeignKey(
                name: "FK_OfxStatements_Users",
                table: "OfxStatements");

            migrationBuilder.DropForeignKey(
                name: "FK_OfxTransactions_Categories",
                table: "OfxTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_OfxTransactions_Users",
                table: "OfxTransactions");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_OfxTransactions_CategoryId",
                table: "OfxTransactions");

            migrationBuilder.DropIndex(
                name: "IX_OfxTransactions_User_DtPosted_TrnType",
                table: "OfxTransactions");

            migrationBuilder.DropIndex(
                name: "IX_OfxStatements_UserId",
                table: "OfxStatements");

            migrationBuilder.DropIndex(
                name: "UQ_OfxImports_User_FileHash",
                table: "OfxImports");

            migrationBuilder.DropIndex(
                name: "IX_OfxBalances_UserId",
                table: "OfxBalances");

            migrationBuilder.DropIndex(
                name: "UQ_FinancialInstitutions_User_BankId_Fid",
                table: "FinancialInstitutions");

            migrationBuilder.DropIndex(
                name: "UQ_BankAccounts_User_BankId_BranchId_AcctId",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "OfxTransactions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "OfxTransactions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "OfxStatements");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "OfxImports");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "OfxBalances");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "FinancialInstitutions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "BankAccounts");

            migrationBuilder.CreateIndex(
                name: "IX_OfxTransactions_DtPosted_TrnType",
                table: "OfxTransactions",
                columns: new[] { "DtPosted", "TrnType" });

            migrationBuilder.CreateIndex(
                name: "UQ_OfxImports_FileHash",
                table: "OfxImports",
                column: "FileHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_FinancialInstitutions_BankId_Fid",
                table: "FinancialInstitutions",
                columns: new[] { "BankId", "Fid" },
                unique: true,
                filter: "[Fid] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_BankAccounts_BankId_BranchId_AcctId",
                table: "BankAccounts",
                columns: new[] { "BankId", "BranchId", "AcctId" },
                unique: true,
                filter: "[BranchId] IS NOT NULL");
        }
    }
}
