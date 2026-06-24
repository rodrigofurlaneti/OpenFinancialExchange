-- =============================================================================
-- MODELO DE BANCO DE DADOS — OFX (Open Financial Exchange)
-- Banco: SQL Server 2019+
-- Padrão: PascalCase
-- Gerado em: 2026-06-24
-- Baseado nos arquivos OFX reais (Banco do Brasil e Bradesco)
-- =============================================================================

-- Criar o banco caso não exista e selecionar
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'OpenFinancialExchange')
    CREATE DATABASE OpenFinancialExchange;
GO

USE OpenFinancialExchange;
GO

-- =============================================================================
-- 1. FinancialInstitutions
--    Normaliza os dados do bloco <FI> (<ORG>, <FID>) e do <BANKID>
-- =============================================================================
CREATE TABLE FinancialInstitutions (
    Id          BIGINT          NOT NULL IDENTITY(1,1),
    BankId      NVARCHAR(20)    NOT NULL,           -- <BANKID>   ex: '0237', '1'
    OrgName     NVARCHAR(100)   NULL,               -- <ORG>      ex: 'Banco do Brasil', 'Bradesco'
    Fid         NVARCHAR(50)    NULL,               -- <FID>      ID numérico da FI no OFX
    CreatedAt   DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt   DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_FinancialInstitutions             PRIMARY KEY (Id),
    CONSTRAINT UQ_FinancialInstitutions_BankId_Fid  UNIQUE (BankId, Fid)
);
GO


-- =============================================================================
-- 2. BankAccounts
--    Normaliza os dados do bloco <BANKACCTFROM>
-- =============================================================================
CREATE TABLE BankAccounts (
    Id                        BIGINT          NOT NULL IDENTITY(1,1),
    FinancialInstitutionId    BIGINT          NOT NULL,
    BankId                    NVARCHAR(20)    NOT NULL,   -- <BANKID>   denormalizado para busca direta
    BranchId                  NVARCHAR(20)    NULL,       -- <BRANCHID> agência (presente no BB, ausente no Bradesco)
    AcctId                    NVARCHAR(50)    NOT NULL,   -- <ACCTID>   número da conta
    AcctType                  NVARCHAR(20)    NOT NULL,   -- <ACCTTYPE> CHECKING | SAVINGS | MONEYMRKT | CREDITLINE | CD | OTHER
    CreatedAt                 DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt                 DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_BankAccounts                          PRIMARY KEY (Id),
    CONSTRAINT FK_BankAccounts_FinancialInstitutions    FOREIGN KEY (FinancialInstitutionId)
        REFERENCES FinancialInstitutions (Id),
    CONSTRAINT UQ_BankAccounts_BankId_BranchId_AcctId   UNIQUE (BankId, BranchId, AcctId),
    CONSTRAINT CK_BankAccounts_AcctType                 CHECK (AcctType IN (
        'CHECKING', 'SAVINGS', 'MONEYMRKT', 'CREDITLINE', 'CD', 'OTHER'
    ))
);
GO

CREATE INDEX IX_BankAccounts_FinancialInstitutionId
    ON BankAccounts (FinancialInstitutionId);
GO


-- =============================================================================
-- 3. OfxImports
--    Registra cada arquivo .ofx processado — metadados do cabeçalho
-- =============================================================================
CREATE TABLE OfxImports (
    Id                  BIGINT          NOT NULL IDENTITY(1,1),
    FileName            NVARCHAR(255)   NOT NULL,           -- Nome original do arquivo
    FileHash            CHAR(64)        NOT NULL,           -- SHA-256 do arquivo (evita reimportação)
    OfxHeaderVersion    SMALLINT        NULL,               -- OFXHEADER: ex 100
    OfxVersion          SMALLINT        NULL,               -- VERSION:   ex 102
    OfxData             NVARCHAR(20)    NULL,               -- DATA:      ex OFXSGML
    Encoding            NVARCHAR(20)    NULL,               -- ENCODING:  ex UTF-8, USASCII
    Charset             NVARCHAR(20)    NULL,               -- CHARSET:   ex 1252, NONE
    Security            NVARCHAR(20)    NULL,               -- SECURITY:  ex NONE
    Compression         NVARCHAR(20)    NULL,               -- COMPRESSION: ex NONE
    OldFileUid          NVARCHAR(36)    NULL,               -- OLDFILEUID
    NewFileUid          NVARCHAR(36)    NULL,               -- NEWFILEUID
    ImportedAt          DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_OfxImports            PRIMARY KEY (Id),
    CONSTRAINT UQ_OfxImports_FileHash   UNIQUE (FileHash)
);
GO


-- =============================================================================
-- 4. OfxStatements
--    Cada bloco <STMTTRNRS> dentro de um arquivo OFX importado
-- =============================================================================
CREATE TABLE OfxStatements (
    Id                  BIGINT          NOT NULL IDENTITY(1,1),
    ImportId            BIGINT          NOT NULL,
    BankAccountId       BIGINT          NOT NULL,
    TrnUid              NVARCHAR(36)    NULL,               -- <TRNUID>       ID da requisição OFX
    CurDef              CHAR(3)         NOT NULL DEFAULT 'BRL',  -- <CURDEF>  moeda ISO 4217
    DtServer            DATETIME2       NULL,               -- <DTSERVER>     data geração do arquivo
    Language            CHAR(3)         NULL,               -- <LANGUAGE>     ex: POR, ENG
    StatusCode          SMALLINT        NULL,               -- <CODE>         0 = sucesso
    StatusSeverity      NVARCHAR(10)    NULL,               -- <SEVERITY>     INFO | WARN | ERROR
    DtStart             DATETIME2       NULL,               -- <DTSTART>      início do período
    DtEnd               DATETIME2       NULL,               -- <DTEND>        fim do período
    CreatedAt           DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_OfxStatements                 PRIMARY KEY (Id),
    CONSTRAINT FK_OfxStatements_OfxImports      FOREIGN KEY (ImportId)
        REFERENCES OfxImports (Id),
    CONSTRAINT FK_OfxStatements_BankAccounts    FOREIGN KEY (BankAccountId)
        REFERENCES BankAccounts (Id)
);
GO

CREATE INDEX IX_OfxStatements_ImportId
    ON OfxStatements (ImportId);
GO

CREATE INDEX IX_OfxStatements_BankAccountId
    ON OfxStatements (BankAccountId);
GO

CREATE INDEX IX_OfxStatements_Periodo
    ON OfxStatements (DtStart, DtEnd);
GO


-- =============================================================================
-- 5. OfxTransactions
--    Cada bloco <STMTTRN> dentro de um extrato
-- =============================================================================
CREATE TABLE OfxTransactions (
    Id              BIGINT          NOT NULL IDENTITY(1,1),
    StatementId     BIGINT          NOT NULL,
    TrnType         NVARCHAR(20)    NOT NULL,               -- <TRNTYPE>
    DtPosted        DATETIME2       NOT NULL,               -- <DTPOSTED>     data da transação
    TrnAmt          DECIMAL(15,2)   NOT NULL,               -- <TRNAMT>       negativo = débito
    FitId           NVARCHAR(255)   NULL,                   -- <FITID>        ID único da FI
    Name            NVARCHAR(255)   NULL,                   -- <NAME>         nome resumido (BB)
    Memo            NVARCHAR(MAX)   NULL,                   -- <MEMO>         descrição completa
    CheckNum        NVARCHAR(50)    NULL,                   -- <CHECKNUM>     número cheque/documento (Bradesco)
    CreatedAt       DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_OfxTransactions               PRIMARY KEY (Id),
    CONSTRAINT FK_OfxTransactions_OfxStatements FOREIGN KEY (StatementId)
        REFERENCES OfxStatements (Id),
    CONSTRAINT CK_OfxTransactions_TrnType       CHECK (TrnType IN (
        'CREDIT','DEBIT','INT','DIV','FEE','SRVCHG',
        'DEP','ATM','POS','XFER','CHECK','PAYMENT',
        'CASH','DIRECTDEP','DIRECTDEBIT','REPEATPMT','OTHER'
    ))
);
GO

-- Índice de deduplicação: impede reimportar a mesma transação (FitId pode ser NULL)
CREATE UNIQUE INDEX UQ_OfxTransactions_StatementId_FitId
    ON OfxTransactions (StatementId, FitId)
    WHERE FitId IS NOT NULL;
GO

CREATE INDEX IX_OfxTransactions_StatementId
    ON OfxTransactions (StatementId);
GO

CREATE INDEX IX_OfxTransactions_DtPosted
    ON OfxTransactions (DtPosted);
GO

CREATE INDEX IX_OfxTransactions_TrnType
    ON OfxTransactions (TrnType);
GO

CREATE INDEX IX_OfxTransactions_TrnAmt
    ON OfxTransactions (TrnAmt);
GO

-- Índice composto para relatórios por período + tipo
CREATE INDEX IX_OfxTransactions_DtPosted_TrnType
    ON OfxTransactions (DtPosted, TrnType)
    INCLUDE (TrnAmt, Memo, Name);
GO


-- =============================================================================
-- 6. OfxBalances
--    Bloco <LEDGERBAL> e, opcionalmente, <AVAILBAL> de cada extrato
-- =============================================================================
CREATE TABLE OfxBalances (
    Id              BIGINT          NOT NULL IDENTITY(1,1),
    StatementId     BIGINT          NOT NULL,
    BalanceType     NVARCHAR(20)    NOT NULL,               -- LEDGER | AVAILABLE
    BalAmt          DECIMAL(15,2)   NOT NULL,               -- <BALAMT>   valor do saldo
    DtAsOf          DATETIME2       NOT NULL,               -- <DTASOF>   data de referência
    CreatedAt       DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_OfxBalances               PRIMARY KEY (Id),
    CONSTRAINT FK_OfxBalances_OfxStatements FOREIGN KEY (StatementId)
        REFERENCES OfxStatements (Id),
    CONSTRAINT CK_OfxBalances_BalanceType   CHECK (BalanceType IN ('LEDGER', 'AVAILABLE'))
);
GO

CREATE INDEX IX_OfxBalances_StatementId
    ON OfxBalances (StatementId);
GO

CREATE INDEX IX_OfxBalances_DtAsOf
    ON OfxBalances (DtAsOf);
GO


-- =============================================================================
-- VIEWS
-- =============================================================================

-- View: transações com contexto completo (conta + banco + período)
CREATE OR ALTER VIEW VwOfxTransactionsFull AS
SELECT
    t.Id                AS TransactionId,
    fi.OrgName          AS Banco,
    fi.BankId           AS BancoCodigo,
    ba.BranchId         AS Agencia,
    ba.AcctId           AS Conta,
    ba.AcctType         AS TipoConta,
    s.DtStart           AS PeriodoInicio,
    s.DtEnd             AS PeriodoFim,
    s.CurDef            AS Moeda,
    t.DtPosted          AS DataLancamento,
    t.TrnType           AS Tipo,
    t.TrnAmt            AS Valor,
    t.Name              AS Nome,
    t.Memo              AS Descricao,
    t.CheckNum          AS NumDocumento,
    t.FitId             AS IdFI,
    i.FileName          AS ArquivoOrigem
FROM OfxTransactions        t
JOIN OfxStatements          s   ON s.Id = t.StatementId
JOIN OfxImports             i   ON i.Id = s.ImportId
JOIN BankAccounts           ba  ON ba.Id = s.BankAccountId
JOIN FinancialInstitutions  fi  ON fi.Id = ba.FinancialInstitutionId;
GO


-- View: saldo mais recente (LEDGER) por conta
CREATE OR ALTER VIEW VwSaldoAtualPorConta AS
SELECT
    fi.OrgName      AS Banco,
    ba.BranchId     AS Agencia,
    ba.AcctId       AS Conta,
    ba.AcctType     AS TipoConta,
    b.BalanceType   AS TipoSaldo,
    b.BalAmt        AS Saldo,
    b.DtAsOf        AS DataReferencia
FROM OfxBalances b
JOIN OfxStatements          s   ON s.Id = b.StatementId
JOIN BankAccounts           ba  ON ba.Id = s.BankAccountId
JOIN FinancialInstitutions  fi  ON fi.Id = ba.FinancialInstitutionId
WHERE
    b.BalanceType = 'LEDGER'
    AND b.DtAsOf = (
        SELECT MAX(b2.DtAsOf)
        FROM OfxBalances        b2
        JOIN OfxStatements      s2  ON s2.Id = b2.StatementId
        WHERE s2.BankAccountId  = s.BankAccountId
          AND b2.BalanceType    = 'LEDGER'
    );
GO
