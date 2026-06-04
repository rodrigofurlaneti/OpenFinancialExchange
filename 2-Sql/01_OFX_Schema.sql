-- ============================================================
-- OPEN FINANCIAL EXCHANGE - RELATIONAL DATA MODEL
-- File        : 01_OFX_Schema.sql
-- Description : Full DDL for SQL Server based on OFX/SGML v1.02
--               capturing 100% of the Bradesco bank statement data
--               for corporate accounting purposes.
-- ============================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'OpenFinancialExchange')
    CREATE DATABASE OpenFinancialExchange
        COLLATE Latin1_General_CI_AS;
GO

USE OpenFinancialExchange;
GO

-- ============================================================
-- SCHEMA
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'ofx')
    EXEC sp_executesql N'CREATE SCHEMA ofx AUTHORIZATION dbo';
GO

-- ============================================================
-- 1. OFX_TransactionCategory
--    Domain table: classifies each transaction for accounting
--    (revenue / expense / transfer).
-- ============================================================
IF OBJECT_ID('ofx.OFX_TransactionCategory', 'U') IS NOT NULL
    DROP TABLE ofx.OFX_TransactionCategory;
GO

CREATE TABLE ofx.OFX_TransactionCategory
(
    Id              INT           NOT NULL IDENTITY(1,1),
    Code            NVARCHAR(60)  NOT NULL,
    Description     NVARCHAR(255) NOT NULL,
    OperationType   NVARCHAR(80)  NOT NULL,    -- e.g. PIX_SENT, SALARY, INVESTMENT...
    AccountingNature NVARCHAR(20) NOT NULL      -- REVENUE | EXPENSE | TRANSFER
        CHECK (AccountingNature IN ('REVENUE','EXPENSE','TRANSFER')),
    IsActive        BIT           NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2(0)  NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT PK_TransactionCategory       PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_TransactionCategory_Code  UNIQUE (Code)
);
GO

-- ============================================================
-- 2. OFX_Bank
--    Financial institution registry (COMPE/ISPB codes).
-- ============================================================
IF OBJECT_ID('ofx.OFX_Bank', 'U') IS NOT NULL
    DROP TABLE ofx.OFX_Bank;
GO

CREATE TABLE ofx.OFX_Bank
(
    Id          INT           NOT NULL IDENTITY(1,1),
    COMPECode   NVARCHAR(10)  NOT NULL,    -- BANKID from OFX
    BankName    NVARCHAR(120) NOT NULL,
    ISPB        NVARCHAR(20)  NULL,
    IsActive    BIT           NOT NULL DEFAULT 1,
    CreatedAt   DATETIME2(0)  NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Bank              PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Bank_COMPECode    UNIQUE (COMPECode)
);
GO

-- ============================================================
-- 3. OFX_Import
--    One record per imported OFX file.
--    Preserves the full technical OFX protocol header.
-- ============================================================
IF OBJECT_ID('ofx.OFX_Import', 'U') IS NOT NULL
    DROP TABLE ofx.OFX_Import;
GO

CREATE TABLE ofx.OFX_Import
(
    Id              INT           NOT NULL IDENTITY(1,1),
    FileName        NVARCHAR(260) NOT NULL,
    ImportedAt      DATETIME2(0)  NOT NULL DEFAULT SYSDATETIME(),
    -- OFX header fields (lines 1-9 of SGML file)
    OFXHeader       NVARCHAR(10)  NOT NULL,    -- ex: 100
    OFXData         NVARCHAR(20)  NOT NULL,    -- OFXSGML
    OFXVersion      NVARCHAR(10)  NOT NULL,    -- 102
    OFXSecurity     NVARCHAR(20)  NOT NULL,    -- NONE | TYPE1
    OFXEncoding     NVARCHAR(20)  NOT NULL,    -- USASCII | UTF-8
    OFXCharset      NVARCHAR(10)  NOT NULL,    -- 1252
    OFXCompression  NVARCHAR(20)  NOT NULL,    -- NONE
    OFXOldFileUID   NVARCHAR(50)  NOT NULL,    -- NONE or GUID
    OFXNewFileUID   NVARCHAR(50)  NOT NULL,    -- NONE or GUID
    Notes           NVARCHAR(500) NULL,
    ImportedBy      NVARCHAR(100) NULL,

    CONSTRAINT PK_Import PRIMARY KEY CLUSTERED (Id)
);
GO

-- ============================================================
-- 4. OFX_SignonSession
--    Server authentication response (SONRS).
-- ============================================================
IF OBJECT_ID('ofx.OFX_SignonSession', 'U') IS NOT NULL
    DROP TABLE ofx.OFX_SignonSession;
GO

CREATE TABLE ofx.OFX_SignonSession
(
    Id              INT           NOT NULL IDENTITY(1,1),
    ImportId        INT           NOT NULL,
    StatusCode      NVARCHAR(10)  NOT NULL,    -- e.g. 0
    StatusSeverity  NVARCHAR(20)  NOT NULL,    -- INFO | WARN | ERROR
    ServerDateRaw   NVARCHAR(20)  NOT NULL,    -- DTSERVER raw value
    ServerDate      DATETIME2(0)  NULL,        -- converted when valid
    Language        NVARCHAR(10)  NOT NULL,    -- POR | ENG

    CONSTRAINT PK_SignonSession         PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_SignonSession_Import  FOREIGN KEY (ImportId)
        REFERENCES ofx.OFX_Import (Id) ON DELETE CASCADE
);
GO

-- ============================================================
-- 5. OFX_Account
--    Source bank account for the transactions (BANKACCTFROM).
-- ============================================================
IF OBJECT_ID('ofx.OFX_Account', 'U') IS NOT NULL
    DROP TABLE ofx.OFX_Account;
GO

CREATE TABLE ofx.OFX_Account
(
    Id              INT           NOT NULL IDENTITY(1,1),
    ImportId        INT           NOT NULL,
    BankId          INT           NOT NULL,
    BranchNumber    NVARCHAR(10)  NULL,         -- branch part of ACCTID
    AccountNumber   NVARCHAR(30)  NOT NULL,     -- full ACCTID (e.g. 449/32016)
    AccountType     NVARCHAR(20)  NOT NULL      -- CHECKING | SAVINGS | MONEYMRKT | CREDITLINE
        CHECK (AccountType IN ('CHECKING','SAVINGS','MONEYMRKT','CREDITLINE')),
    DefaultCurrency NCHAR(3)      NOT NULL DEFAULT 'BRL',

    CONSTRAINT PK_Account           PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Account_Import    FOREIGN KEY (ImportId)
        REFERENCES ofx.OFX_Import (Id) ON DELETE CASCADE,
    CONSTRAINT FK_Account_Bank      FOREIGN KEY (BankId)
        REFERENCES ofx.OFX_Bank (Id)
);
GO

-- ============================================================
-- 6. OFX_Statement
--    Statement header (STMTTRNRS / STMTRS).
--    A single file may contain more than one statement.
-- ============================================================
IF OBJECT_ID('ofx.OFX_Statement', 'U') IS NOT NULL
    DROP TABLE ofx.OFX_Statement;
GO

CREATE TABLE ofx.OFX_Statement
(
    Id              INT           NOT NULL IDENTITY(1,1),
    AccountId       INT           NOT NULL,
    TRNUID          NVARCHAR(20)  NOT NULL,    -- unique request ID
    StatusCode      NVARCHAR(10)  NOT NULL,
    StatusSeverity  NVARCHAR(20)  NOT NULL,
    StartDate       DATETIME2(0)  NOT NULL,    -- DTSTART
    EndDate         DATETIME2(0)  NOT NULL,    -- DTEND
    TimeZone        NVARCHAR(20)  NULL,        -- e.g. -03:EST

    CONSTRAINT PK_Statement             PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Statement_Account     FOREIGN KEY (AccountId)
        REFERENCES ofx.OFX_Account (Id) ON DELETE CASCADE
);
GO

-- ============================================================
-- 7. OFX_LedgerBalance
--    Balances reported in the statement (LEDGERBAL / AVAILBAL).
-- ============================================================
IF OBJECT_ID('ofx.OFX_LedgerBalance', 'U') IS NOT NULL
    DROP TABLE ofx.OFX_LedgerBalance;
GO

CREATE TABLE ofx.OFX_LedgerBalance
(
    Id              INT            NOT NULL IDENTITY(1,1),
    StatementId     INT            NOT NULL,
    BalanceType     NVARCHAR(20)   NOT NULL    -- LEDGER | AVAIL
        CHECK (BalanceType IN ('LEDGER','AVAIL')),
    Amount          DECIMAL(18,2)  NOT NULL,
    AsOfDate        DATETIME2(0)   NOT NULL,   -- DTASOF

    CONSTRAINT PK_LedgerBalance             PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_LedgerBalance_Statement   FOREIGN KEY (StatementId)
        REFERENCES ofx.OFX_Statement (Id) ON DELETE CASCADE
);
GO

-- ============================================================
-- 8. OFX_Transaction
--    Individual statement entries (STMTTRN). Core of the model.
--    All native OFX fields are preserved, plus derived fields
--    to support accounting classification.
-- ============================================================
IF OBJECT_ID('ofx.OFX_Transaction', 'U') IS NOT NULL
    DROP TABLE ofx.OFX_Transaction;
GO

CREATE TABLE ofx.OFX_Transaction
(
    Id                  INT            NOT NULL IDENTITY(1,1),
    StatementId         INT            NOT NULL,
    CategoryId          INT            NULL,

    -- Native OFX fields -----------------------------------------
    TransactionType     NVARCHAR(15)   NOT NULL   -- TRNTYPE
        CHECK (TransactionType IN ('CREDIT','DEBIT','INT','DIV','FEE','SRVCHG',
                                   'DEP','ATM','POS','XFER','CHECK','PAYMENT',
                                   'CASH','DIRECTDEP','DIRECTDEBIT','REPEATPMT','OTHER')),
    PostedDateRaw       NVARCHAR(30)   NOT NULL,  -- DTPOSTED original with timezone
    PostedDate          DATE           NOT NULL,  -- DTPOSTED converted
    TimeZone            NVARCHAR(20)   NULL,      -- e.g. -03:EST
    Amount              DECIMAL(18,2)  NOT NULL,  -- TRNAMT (signed)
    FITID               NVARCHAR(255)  NOT NULL,  -- Financial Institution Transaction ID
    CheckNumber         NVARCHAR(50)   NULL,      -- CHECKNUM
    Memo                NVARCHAR(500)  NULL,      -- MEMO (original description)

    -- Derived fields for accounting -----------------------------
    AbsoluteAmount      AS ABS(Amount) PERSISTED,
    MovementNature      AS CASE WHEN Amount >= 0 THEN 'CREDIT' ELSE 'DEBIT' END PERSISTED,
    PayeeName           NVARCHAR(255)  NULL,      -- extracted from MEMO
    TransactionDateMemo NVARCHAR(10)   NULL,      -- date found in MEMO (e.g. 13/02)
    OperationSubtype    NVARCHAR(100)  NULL,      -- e.g. Pix Transfer, QR Code Payment...
    IsReconciled        BIT            NOT NULL DEFAULT 0,
    ReconciledAt        DATETIME2(0)   NULL,
    CreatedAt           DATETIME2(0)   NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Transaction               PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Transaction_FITID         UNIQUE (FITID),
    CONSTRAINT FK_Transaction_Statement     FOREIGN KEY (StatementId)
        REFERENCES ofx.OFX_Statement (Id),
    CONSTRAINT FK_Transaction_Category      FOREIGN KEY (CategoryId)
        REFERENCES ofx.OFX_TransactionCategory (Id)
);
GO

-- ============================================================
-- INDEXES
-- ============================================================

-- Queries by posting date (accounting reports)
CREATE NONCLUSTERED INDEX IX_Transaction_PostedDate
    ON ofx.OFX_Transaction (PostedDate)
    INCLUDE (Amount, TransactionType, Memo, CategoryId);
GO

-- Queries by category (P&L / cost center)
CREATE NONCLUSTERED INDEX IX_Transaction_Category
    ON ofx.OFX_Transaction (CategoryId, PostedDate)
    INCLUDE (Amount, MovementNature);
GO

-- Payee lookup
CREATE NONCLUSTERED INDEX IX_Transaction_Payee
    ON ofx.OFX_Transaction (PayeeName)
    INCLUDE (PostedDate, Amount);
GO

-- Filter by statement
CREATE NONCLUSTERED INDEX IX_Transaction_Statement
    ON ofx.OFX_Transaction (StatementId, PostedDate);
GO

-- ============================================================
-- VIEW: vw_AccountingStatement
--    Denormalized view for reporting / BI consumption
-- ============================================================
IF OBJECT_ID('ofx.vw_AccountingStatement', 'V') IS NOT NULL
    DROP VIEW ofx.vw_AccountingStatement;
GO

CREATE VIEW ofx.vw_AccountingStatement
AS
SELECT
    imp.Id                  AS ImportId,
    imp.FileName,
    imp.ImportedAt,
    b.COMPECode             AS BankCode,
    b.BankName,
    a.AccountNumber,
    a.AccountType,
    a.DefaultCurrency,
    s.TRNUID,
    s.StartDate             AS StatementStartDate,
    s.EndDate               AS StatementEndDate,
    t.Id                    AS TransactionId,
    t.FITID,
    t.PostedDate,
    t.TransactionType,
    t.Amount,
    t.AbsoluteAmount,
    t.MovementNature,
    t.CheckNumber,
    t.Memo,
    t.OperationSubtype,
    t.PayeeName,
    t.TransactionDateMemo,
    cat.Code                AS AccountingCategory,
    cat.Description         AS CategoryDescription,
    cat.OperationType,
    cat.AccountingNature,
    t.IsReconciled,
    t.CreatedAt
FROM ofx.OFX_Transaction            t
JOIN ofx.OFX_Statement              s   ON s.Id  = t.StatementId
JOIN ofx.OFX_Account                a   ON a.Id  = s.AccountId
JOIN ofx.OFX_Bank                   b   ON b.Id  = a.BankId
JOIN ofx.OFX_Import                 imp ON imp.Id = a.ImportId
LEFT JOIN ofx.OFX_TransactionCategory cat ON cat.Id = t.CategoryId;
GO

PRINT 'OFX schema created successfully.';
GO
