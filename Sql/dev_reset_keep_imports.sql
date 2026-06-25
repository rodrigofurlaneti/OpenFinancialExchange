/* =====================================================================
   DEV RESET — limpa os dados mantendo o necessário para reprocessar.

   MANTÉM:  Users, OfxImports (com OfxData), Categorias de SISTEMA, BankCodes
   APAGA:   OfxTransactions, OfxBalances, OfxStatements, BankAccounts,
            FinancialInstitutions, Categorias customizadas (UserId != NULL)

   Depois de rodar isto, use o botão "Reprocessar tudo" na tela OFX:
   ele re-parseia o OfxData salvo e recria instituição/conta/extrato/transações.

   ATENÇÃO: destrutivo. Rode no banco de desenvolvimento OpenFinancialExchange.
   ===================================================================== */
SET NOCOUNT ON;

BEGIN TRANSACTION;

DELETE FROM [OfxTransactions];
DELETE FROM [OfxBalances];
DELETE FROM [OfxStatements];
DELETE FROM [BankAccounts];
DELETE FROM [FinancialInstitutions];

-- Mantém as 13 categorias de sistema (UserId IS NULL); apaga só as customizadas.
DELETE FROM [Categories] WHERE [UserId] IS NOT NULL;

COMMIT TRANSACTION;

/* Conferência rápida */
SELECT 'OfxImports'           AS Tabela, COUNT(*) AS Linhas FROM [OfxImports]
UNION ALL SELECT 'OfxStatements',        COUNT(*) FROM [OfxStatements]
UNION ALL SELECT 'OfxTransactions',      COUNT(*) FROM [OfxTransactions]
UNION ALL SELECT 'OfxBalances',          COUNT(*) FROM [OfxBalances]
UNION ALL SELECT 'BankAccounts',         COUNT(*) FROM [BankAccounts]
UNION ALL SELECT 'FinancialInstitutions',COUNT(*) FROM [FinancialInstitutions]
UNION ALL SELECT 'Categories (total)',   COUNT(*) FROM [Categories]
UNION ALL SELECT 'Users',                COUNT(*) FROM [Users];
