-- ============================================================
-- OPEN FINANCIAL EXCHANGE - SEED DATA
-- File        : 02_OFX_SeedData.sql
-- Description : Full data population extracted from
--               Bradesco.ofx (statement Jan-May/2026).
--               Run AFTER 01_OFX_Schema.sql
-- ============================================================

USE OpenFinancialExchange;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- ============================================================
-- 1. TRANSACTION CATEGORIES (domain / lookup)
-- ============================================================
INSERT INTO ofx.OFX_TransactionCategory
    (Code, Description, OperationType, AccountingNature)
VALUES
  ('SALARY_TRANSFER',   'Salary Transfer to Checking Account',    'SALARY_TRANSFER',        'REVENUE'),
  ('INVEST_APPLICATION','Investment Application - Invest Facil',  'INVESTMENT_APPLICATION',  'EXPENSE'),
  ('INVEST_REDEMPTION', 'Investment Redemption - Invest Facil',   'INVESTMENT_REDEMPTION',   'REVENUE'),
  ('INVEST_YIELD',      'Investment Yield - Invest Facil',        'INVESTMENT_YIELD',        'REVENUE'),
  ('PIX_SENT',          'PIX Transfer Sent',                      'PIX_SENT',                'EXPENSE'),
  ('PIX_RECEIVED',      'PIX Transfer Received',                  'PIX_RECEIVED',            'REVENUE'),
  ('PIX_REFUND',        'PIX Refund Received',                    'PIX_REFUND',              'REVENUE'),
  ('PIX_QR_DYNAMIC',    'PIX Dynamic QR Code Payment',            'PIX_QRCODE_DYNAMIC',      'EXPENSE'),
  ('PIX_QR_STATIC',     'PIX Static QR Code Payment',             'PIX_QRCODE_STATIC',       'EXPENSE'),
  ('BILL_PAYMENT',      'Bill / Boleto Payment',                  'BILL_PAYMENT',            'EXPENSE'),
  ('CARD_CREDIT_ELO',   'Elo Credit Card Purchase',               'CARD_PURCHASE_CREDIT',    'EXPENSE'),
  ('CARD_DEBIT_ELO',    'Elo Debit Card Purchase',                'CARD_PURCHASE_DEBIT',     'EXPENSE'),
  ('INVEST_AUTO_APP',   'Automatic Investment Application',        'INVESTMENT_APPLICATION',  'EXPENSE'),
  ('PIX_TRANSFER_IN',   'Incoming PIX Transfer (Remitter)',        'PIX_RECEIVED',            'REVENUE');
GO

-- ============================================================
-- 2. BANK
-- ============================================================
INSERT INTO ofx.OFX_Bank (COMPECode, BankName, ISPB)
VALUES ('0237', 'Banco Bradesco S.A.', '60746948');
GO

-- ============================================================
-- 3. IMPORT (OFX file header)
-- ============================================================
INSERT INTO ofx.OFX_Import
    (FileName, OFXHeader, OFXData, OFXVersion, OFXSecurity,
     OFXEncoding, OFXCharset, OFXCompression, OFXOldFileUID,
     OFXNewFileUID, Notes, ImportedBy)
VALUES
    ('Bradesco.ofx', '100', 'OFXSGML', '102', 'NONE',
     'USASCII', '1252', 'NONE', 'NONE', 'NONE',
     'Bradesco checking account statement - period 01/01/2026 to 10/05/2026',
     'OpenFinancialExchange');
GO

-- ============================================================
-- 4. SIGNON SESSION
-- ============================================================
INSERT INTO ofx.OFX_SignonSession
    (ImportId, StatusCode, StatusSeverity, ServerDateRaw, ServerDate, Language)
SELECT TOP 1 Id, '0', 'INFO', '00000000000000', NULL, 'POR'
FROM ofx.OFX_Import ORDER BY Id DESC;
GO

-- ============================================================
-- 5. ACCOUNT
-- ============================================================
INSERT INTO ofx.OFX_Account
    (ImportId, BankId, BranchNumber, AccountNumber, AccountType, DefaultCurrency)
SELECT
    (SELECT TOP 1 Id FROM ofx.OFX_Import ORDER BY Id DESC),
    (SELECT Id FROM ofx.OFX_Bank WHERE COMPECode = '0237'),
    '00449', '449/32016', 'CHECKING', 'BRL';
GO

-- ============================================================
-- 6. STATEMENT
-- ============================================================
INSERT INTO ofx.OFX_Statement
    (AccountId, TRNUID, StatusCode, StatusSeverity, StartDate, EndDate, TimeZone)
SELECT TOP 1
    Id, '1001', '0', 'INFO', '2026-01-01', '2026-05-10', '-03:EST'
FROM ofx.OFX_Account ORDER BY Id DESC;
GO

-- ============================================================
-- 7. LEDGER BALANCE
-- ============================================================
INSERT INTO ofx.OFX_LedgerBalance (StatementId, BalanceType, Amount, AsOfDate)
SELECT TOP 1 Id, 'LEDGER', 10.26, '2026-05-07'
FROM ofx.OFX_Statement ORDER BY Id DESC;
GO

-- ============================================================
-- 8. TRANSACTIONS
--    All 227 entries from Bradesco.ofx (Jan-May 2026)
-- ============================================================

DECLARE @StmtId   INT = (SELECT TOP 1 Id FROM ofx.OFX_Statement ORDER BY Id DESC);

-- Category ID helpers
DECLARE @catSal   INT = (SELECT Id FROM ofx.OFX_TransactionCategory WHERE Code = 'SALARY_TRANSFER');
DECLARE @catApl   INT = (SELECT Id FROM ofx.OFX_TransactionCategory WHERE Code = 'INVEST_APPLICATION');
DECLARE @catRes   INT = (SELECT Id FROM ofx.OFX_TransactionCategory WHERE Code = 'INVEST_REDEMPTION');
DECLARE @catYld   INT = (SELECT Id FROM ofx.OFX_TransactionCategory WHERE Code = 'INVEST_YIELD');
DECLARE @catPS    INT = (SELECT Id FROM ofx.OFX_TransactionCategory WHERE Code = 'PIX_SENT');
DECLARE @catPR    INT = (SELECT Id FROM ofx.OFX_TransactionCategory WHERE Code = 'PIX_RECEIVED');
DECLARE @catPF    INT = (SELECT Id FROM ofx.OFX_TransactionCategory WHERE Code = 'PIX_REFUND');
DECLARE @catQD    INT = (SELECT Id FROM ofx.OFX_TransactionCategory WHERE Code = 'PIX_QR_DYNAMIC');
DECLARE @catQS    INT = (SELECT Id FROM ofx.OFX_TransactionCategory WHERE Code = 'PIX_QR_STATIC');
DECLARE @catBP    INT = (SELECT Id FROM ofx.OFX_TransactionCategory WHERE Code = 'BILL_PAYMENT');
DECLARE @catCC    INT = (SELECT Id FROM ofx.OFX_TransactionCategory WHERE Code = 'CARD_CREDIT_ELO');
DECLARE @catCD    INT = (SELECT Id FROM ofx.OFX_TransactionCategory WHERE Code = 'CARD_DEBIT_ELO');
DECLARE @catAA    INT = (SELECT Id FROM ofx.OFX_TransactionCategory WHERE Code = 'INVEST_AUTO_APP');
DECLARE @catPI    INT = (SELECT Id FROM ofx.OFX_TransactionCategory WHERE Code = 'PIX_TRANSFER_IN');

INSERT INTO ofx.OFX_Transaction
    (StatementId, CategoryId, TransactionType, PostedDateRaw, PostedDate,
     TimeZone, Amount, FITID, CheckNumber, Memo,
     PayeeName, TransactionDateMemo, OperationSubtype)
VALUES

-- ===  02/13/2026  ===
(@StmtId,@catSal,'CREDIT','20260213000000[-03:EST]','2026-02-13','-03:EST',
  733.42,'N205C4:13/02/26:733.42:1300449','1300449',
  'Salary Transfer Bco:237 Ag:00449 Acct:0032016-1',
  'Banco Bradesco Ag.00449 Acct:0032016-1','02/13','Salary Transfer'),

(@StmtId,@catApl,'DEBIT','20260213000000[-03:EST]','2026-02-13','-03:EST',
  -732.42,'N205E1:13/02/26:-732.42:3125571','3125571',
  'Investment Application Invest Facil',
  NULL,'02/13','Investment Application'),

-- ===  02/19/2026  ===
(@StmtId,@catRes,'CREDIT','20260219000000[-03:EST]','2026-02-19','-03:EST',
  413.15,'N205FE:19/02/26:413.15:3125571','3125571',
  'Investment Redemption Invest Facil',
  NULL,'02/19','Investment Redemption'),

(@StmtId,@catYld,'CREDIT','20260219000000[-03:EST]','2026-02-19','-03:EST',
  0.01,'N2061A:19/02/26:0.01:3125571','3125571',
  'Investment Yield Invest Facil',
  NULL,'02/19','Investment Yield'),

(@StmtId,@catPS,'DEBIT','20260219000000[-03:EST]','2026-02-19','-03:EST',
  -250.00,'N20636:19/02/26:-250.0:1156144','1156144',
  'PIX Transfer Sent To: Bruno da Silva 02/19',
  'Bruno da Silva','02/19','PIX Transfer Sent'),

(@StmtId,@catQD,'DEBIT','20260219000000[-03:EST]','2026-02-19','-03:EST',
  -5.00,'N20654:19/02/26:-5.0:1307020','1307020',
  'PIX QR Dynamic Payment To: Geovan Pereira de Alm 02/19',
  'Geovan Pereira de Alm','02/19','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260219000000[-03:EST]','2026-02-19','-03:EST',
  -158.16,'N20672:19/02/26:-158.16:1434319','1434319',
  'PIX QR Dynamic Payment To: Fam Centro Universitario 02/19',
  'FAM Centro Universitario','02/19','PIX QR Dynamic'),

-- ===  02/20/2026  ===
(@StmtId,@catRes,'CREDIT','20260220000000[-03:EST]','2026-02-20','-03:EST',
  139.12,'N20690:20/02/26:139.12:3125571','3125571',
  'Investment Redemption Invest Facil',
  NULL,'02/20','Investment Redemption'),

(@StmtId,@catQD,'DEBIT','20260220000000[-03:EST]','2026-02-20','-03:EST',
  -20.00,'N206AC:20/02/26:-20.0:0700522','0700522',
  'PIX QR Dynamic Payment To: af Line.com 02/20',
  'af Line.com','02/20','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260220000000[-03:EST]','2026-02-20','-03:EST',
  -23.00,'N206CA:20/02/26:-23.0:0718443','0718443',
  'PIX QR Dynamic Payment To: Sant Sucos e Lanches 02/20',
  'Sant Sucos e Lanches','02/20','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260220000000[-03:EST]','2026-02-20','-03:EST',
  -19.99,'N206E8:20/02/26:-19.99:0747278','0747278',
  'PIX QR Dynamic Payment To: af Line.com 02/20',
  'af Line.com','02/20','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260220000000[-03:EST]','2026-02-20','-03:EST',
  -76.13,'N20706:20/02/26:-76.13:0803008','0803008',
  'PIX QR Dynamic Payment To: Companhia Brasileira 02/20',
  'Companhia Brasileira','02/20','PIX QR Dynamic'),

-- ===  02/23/2026  ===
(@StmtId,@catRes,'CREDIT','20260223000000[-03:EST]','2026-02-23','-03:EST',
  50.00,'N20724:23/02/26:50.0:3125571','3125571',
  'Investment Redemption Invest Facil',
  NULL,'02/23','Investment Redemption'),

(@StmtId,@catQD,'DEBIT','20260223000000[-03:EST]','2026-02-23','-03:EST',
  -50.00,'N20740:23/02/26:-50.0:1847352','1847352',
  'PIX QR Dynamic Payment To: Speedy Auto Posto Ltd 02/22',
  'Speedy Auto Posto Ltd','02/22','PIX QR Dynamic'),

-- ===  02/24/2026  ===
(@StmtId,@catRes,'CREDIT','20260224000000[-03:EST]','2026-02-24','-03:EST',
  100.49,'N2075E:24/02/26:100.49:3125571','3125571',
  'Investment Redemption Invest Facil',
  NULL,'02/24','Investment Redemption'),

(@StmtId,@catYld,'CREDIT','20260224000000[-03:EST]','2026-02-24','-03:EST',
  0.01,'N2077A:24/02/26:0.01:3125571','3125571',
  'Investment Yield Invest Facil',
  NULL,'02/24','Investment Yield'),

(@StmtId,@catQD,'DEBIT','20260224000000[-03:EST]','2026-02-24','-03:EST',
  -20.00,'N20796:24/02/26:-20.0:0640557','0640557',
  'PIX QR Dynamic Payment To: af Line.com 02/24',
  'af Line.com','02/24','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260224000000[-03:EST]','2026-02-24','-03:EST',
  -23.00,'N207B4:24/02/26:-23.0:0659216','0659216',
  'PIX QR Dynamic Payment To: Sant Sucos e Lanches 02/24',
  'Sant Sucos e Lanches','02/24','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260224000000[-03:EST]','2026-02-24','-03:EST',
  -49.00,'N207D2:24/02/26:-49.0:1304103','1304103',
  'PIX QR Dynamic Payment To: Restaurante Bar e Cafe 02/24',
  'Restaurante Bar e Cafe','02/24','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260224000000[-03:EST]','2026-02-24','-03:EST',
  -8.50,'N207F0:24/02/26:-8.5:1308129','1308129',
  'PIX QR Dynamic Payment To: Horti Fruit Haddock 02/24',
  'Horti Fruit Haddock','02/24','PIX QR Dynamic'),

-- ===  02/25/2026  ===
(@StmtId,@catRes,'CREDIT','20260225000000[-03:EST]','2026-02-25','-03:EST',
  29.66,'N2080E:25/02/26:29.66:3125571','3125571',
  'Investment Redemption Invest Facil',
  NULL,'02/25','Investment Redemption'),

(@StmtId,@catPS,'DEBIT','20260225000000[-03:EST]','2026-02-25','-03:EST',
  -30.66,'N2082A:25/02/26:-30.66:0622124','0622124',
  'PIX Transfer Sent To: Rodrigo Luiz Madeira 02/25',
  'Rodrigo Luiz Madeira','02/25','PIX Transfer Sent'),

-- ===  02/27/2026  ===
(@StmtId,@catSal,'CREDIT','20260227000000[-03:EST]','2026-02-27','-03:EST',
  6624.72,'N20848:27/02/26:6624.72:2700449','2700449',
  'Salary Transfer Bco:237 Ag:00449 Acct:0032016-1',
  'Banco Bradesco Ag.00449 Acct:0032016-1','02/27','Salary Transfer'),

(@StmtId,@catBP,'DEBIT','20260227000000[-03:EST]','2026-02-27','-03:EST',
  -1513.39,'N20865:27/02/26:-1513.39:0000001','0000001',
  'Bill Payment GCI Caixa - Housing',
  'GCI Caixa Housing Loan','02/27','Bill Payment'),

(@StmtId,@catApl,'DEBIT','20260227000000[-03:EST]','2026-02-27','-03:EST',
  -3317.34,'N20883:27/02/26:-3317.34:4608848','4608848',
  'Investment Application Invest Facil',
  NULL,'02/27','Investment Application'),

(@StmtId,@catPS,'DEBIT','20260227000000[-03:EST]','2026-02-27','-03:EST',
  -1650.00,'N208A0:27/02/26:-1650.0:1527204','1527204',
  'PIX Transfer Sent To: Lazaro Augusto Gusmao 02/27',
  'Lazaro Augusto Gusmao','02/27','PIX Transfer Sent'),

(@StmtId,@catQS,'DEBIT','20260227000000[-03:EST]','2026-02-27','-03:EST',
  -142.99,'N208BE:27/02/26:-142.99:1421017','1421017',
  'PIX QR Static Payment To: Pix Marketplace 02/27',
  'Pix Marketplace','02/27','PIX QR Static'),

-- ===  03/02/2026  ===
(@StmtId,@catRes,'CREDIT','20260302000000[-03:EST]','2026-03-02','-03:EST',
  3317.34,'N208DC:02/03/26:3317.34:4608848','4608848',
  'Investment Redemption Invest Facil',
  NULL,'03/02','Investment Redemption'),

(@StmtId,@catYld,'CREDIT','20260302000000[-03:EST]','2026-03-02','-03:EST',
  0.01,'N208F8:02/03/26:0.01:4608848','4608848',
  'Investment Yield Invest Facil',
  NULL,'03/02','Investment Yield'),

(@StmtId,@catPS,'DEBIT','20260302000000[-03:EST]','2026-03-02','-03:EST',
  -2582.35,'N20914:02/03/26:-2582.35:2106407','2106407',
  'PIX Transfer Sent To: Rodrigo Luiz Madeira 03/01',
  'Rodrigo Luiz Madeira','03/01','PIX Transfer Sent'),

(@StmtId,@catQD,'DEBIT','20260302000000[-03:EST]','2026-03-02','-03:EST',
  -736.00,'N20932:02/03/26:-736.0:1601480','1601480',
  'PIX QR Dynamic Payment To: Galpao Fortes Distribuidora 02/28',
  'Galpao Fortes Distribuidora','02/28','PIX QR Dynamic'),

-- ===  03/13/2026  ===
(@StmtId,@catSal,'CREDIT','20260313000000[-03:EST]','2026-03-13','-03:EST',
  5280.00,'N20950:13/03/26:5280.0:1300449','1300449',
  'Salary Transfer Bco:237 Ag:00449 Acct:0032016-1',
  'Banco Bradesco Ag.00449 Acct:0032016-1','03/13','Salary Transfer'),

(@StmtId,@catBP,'DEBIT','20260313000000[-03:EST]','2026-03-13','-03:EST',
  -3441.97,'N2096D:13/03/26:-3441.97:0000002','0000002',
  'Bill Payment Grpqa',
  'Grpqa','03/13','Bill Payment'),

(@StmtId,@catApl,'DEBIT','20260313000000[-03:EST]','2026-03-13','-03:EST',
  -1793.05,'N2098B:13/03/26:-1793.05:2832416','2832416',
  'Investment Application Invest Facil',
  NULL,'03/13','Investment Application'),

(@StmtId,@catQD,'DEBIT','20260313000000[-03:EST]','2026-03-13','-03:EST',
  -43.98,'N209A8:13/03/26:-43.98:2332122','2332122',
  'PIX QR Dynamic Payment To: Bar Vila Augusta 03/13',
  'Bar Vila Augusta','03/13','PIX QR Dynamic'),

-- ===  03/16/2026  ===
(@StmtId,@catRes,'CREDIT','20260316000000[-03:EST]','2026-03-16','-03:EST',
  1248.35,'N209C6:16/03/26:1248.35:2832416','2832416',
  'Investment Redemption Invest Facil',
  NULL,'03/16','Investment Redemption'),

(@StmtId,@catYld,'CREDIT','20260316000000[-03:EST]','2026-03-16','-03:EST',
  0.01,'N209E2:16/03/26:0.01:2832416','2832416',
  'Investment Yield Invest Facil',
  NULL,'03/16','Investment Yield'),

(@StmtId,@catBP,'DEBIT','20260316000000[-03:EST]','2026-03-16','-03:EST',
  -284.46,'N209FE:16/03/26:-284.46:0000003','0000003',
  'Bill Payment Itau Unibanco S.A.',
  'Itau Unibanco S.A.','03/16','Bill Payment'),

(@StmtId,@catPS,'DEBIT','20260316000000[-03:EST]','2026-03-16','-03:EST',
  -200.00,'N20A1C:16/03/26:-200.0:0749195','0749195',
  'PIX Transfer Sent To: Rodrigo Luiz Madeira 03/14',
  'Rodrigo Luiz Madeira','03/14','PIX Transfer Sent'),

(@StmtId,@catPS,'DEBIT','20260316000000[-03:EST]','2026-03-16','-03:EST',
  -300.00,'N20A3A:16/03/26:-300.0:1333040','1333040',
  'PIX Transfer Sent To: Katia Cristina Piacenzi 03/16',
  'Katia Cristina Piacenzi','03/16','PIX Transfer Sent'),

(@StmtId,@catPS,'DEBIT','20260316000000[-03:EST]','2026-03-16','-03:EST',
  -40.00,'N20A58:16/03/26:-40.0:1345472','1345472',
  'PIX Transfer Sent To: Emerson Anizio 03/14',
  'Emerson Anizio','03/14','PIX Transfer Sent'),

(@StmtId,@catQS,'DEBIT','20260316000000[-03:EST]','2026-03-16','-03:EST',
  -150.00,'N20A76:16/03/26:-150.0:1127087','1127087',
  'PIX QR Static Payment To: Rodrigo Luiz Madeira 03/15',
  'Rodrigo Luiz Madeira','03/15','PIX QR Static'),

(@StmtId,@catQD,'DEBIT','20260316000000[-03:EST]','2026-03-16','-03:EST',
  -190.90,'N20A94:16/03/26:-190.9:0922035','0922035',
  'PIX QR Dynamic Payment To: Sem Parar Instituicao 03/14',
  'Sem Parar Instituicao','03/14','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260316000000[-03:EST]','2026-03-16','-03:EST',
  -22.00,'N20AB2:16/03/26:-22.0:1016384','1016384',
  'PIX QR Dynamic Payment To: Rede lp Park Estacionamento 03/14',
  'Rede lp Park Estacionamento','03/14','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260316000000[-03:EST]','2026-03-16','-03:EST',
  -61.00,'N20AD0:16/03/26:-61.0:1035368','1035368',
  'PIX QR Dynamic Payment To: Deocleciano Macedo 03/14',
  'Deocleciano Macedo','03/14','PIX QR Dynamic'),

-- ===  03/17/2026  ===
(@StmtId,@catRes,'CREDIT','20260317000000[-03:EST]','2026-03-17','-03:EST',
  94.43,'N20AEE:17/03/26:94.43:2832416','2832416',
  'Investment Redemption Invest Facil',
  NULL,'03/17','Investment Redemption'),

(@StmtId,@catQD,'DEBIT','20260317000000[-03:EST]','2026-03-17','-03:EST',
  -94.43,'N20B0A:17/03/26:-94.43:0754067','0754067',
  'PIX QR Dynamic Payment To: Companhia Brasileira 03/17',
  'Companhia Brasileira','03/17','PIX QR Dynamic'),

-- ===  03/18/2026  ===
(@StmtId,@catRes,'CREDIT','20260318000000[-03:EST]','2026-03-18','-03:EST',
  67.99,'N20B28:18/03/26:67.99:2832416','2832416',
  'Investment Redemption Invest Facil',
  NULL,'03/18','Investment Redemption'),

(@StmtId,@catQD,'DEBIT','20260318000000[-03:EST]','2026-03-18','-03:EST',
  -32.99,'N20B44:18/03/26:-32.99:0743328','0743328',
  'PIX QR Dynamic Payment To: Smoov 010 Ibirapuera 03/18',
  'Smoov 010 Ibirapuera','03/18','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260318000000[-03:EST]','2026-03-18','-03:EST',
  -35.00,'N20B62:18/03/26:-35.0:1755084','1755084',
  'PIX QR Dynamic Payment To: Ana Maria da Penha 03/18',
  'Ana Maria da Penha','03/18','PIX QR Dynamic'),

-- ===  03/19/2026  ===
(@StmtId,@catRes,'CREDIT','20260319000000[-03:EST]','2026-03-19','-03:EST',
  85.58,'N20B80:19/03/26:85.58:2832416','2832416',
  'Investment Redemption Invest Facil',
  NULL,'03/19','Investment Redemption'),

(@StmtId,@catQD,'DEBIT','20260319000000[-03:EST]','2026-03-19','-03:EST',
  -85.58,'N20B9C:19/03/26:-85.58:1332318','1332318',
  'PIX QR Dynamic Payment To: Jhow Jhow 03/19',
  'Jhow Jhow','03/19','PIX QR Dynamic'),

-- ===  03/20/2026  ===
(@StmtId,@catRes,'CREDIT','20260320000000[-03:EST]','2026-03-20','-03:EST',
  39.77,'N20BBA:20/03/26:39.77:2832416','2832416',
  'Investment Redemption Invest Facil',
  NULL,'03/20','Investment Redemption'),

(@StmtId,@catPI,'CREDIT','20260320000000[-03:EST]','2026-03-20','-03:EST',
  75.57,'N20BD6:20/03/26:75.57:2033148','2033148',
  'PIX Transfer Received From: Rodrigo Furlaneti 03/20',
  'Rodrigo Furlaneti','03/20','PIX Transfer Received'),

(@StmtId,@catQD,'DEBIT','20260320000000[-03:EST]','2026-03-20','-03:EST',
  -28.00,'N20BF3:20/03/26:-28.0:0752280','0752280',
  'PIX QR Dynamic Payment To: Ana Maria da Penha 03/20',
  'Ana Maria da Penha','03/20','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260320000000[-03:EST]','2026-03-20','-03:EST',
  -61.85,'N20C11:20/03/26:-61.85:1312508','1312508',
  'PIX QR Dynamic Payment To: Galeria Paulista 03/20',
  'Galeria Paulista','03/20','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260320000000[-03:EST]','2026-03-20','-03:EST',
  -25.49,'N20C2F:20/03/26:-25.49:1339421','1339421',
  'PIX QR Dynamic Payment To: Rocha Mini Mercado 03/20',
  'Rocha Mini Mercado','03/20','PIX QR Dynamic'),

-- ===  03/23/2026  ===
(@StmtId,@catRes,'CREDIT','20260323000000[-03:EST]','2026-03-23','-03:EST',
  224.02,'N20C4D:23/03/26:224.02:2832416','2832416',
  'Investment Redemption Invest Facil',
  NULL,'03/23','Investment Redemption'),

(@StmtId,@catYld,'CREDIT','20260323000000[-03:EST]','2026-03-23','-03:EST',
  0.01,'N20C69:23/03/26:0.01:2832416','2832416',
  'Investment Yield Invest Facil',
  NULL,'03/23','Investment Yield'),

(@StmtId,@catQS,'DEBIT','20260323000000[-03:EST]','2026-03-23','-03:EST',
  -39.60,'N20C85:23/03/26:-39.6:0750198','0750198',
  'PIX QR Static Payment To: Lanchonete e Restaurante 03/22',
  'Lanchonete e Restaurante','03/22','PIX QR Static'),

(@StmtId,@catQS,'DEBIT','20260323000000[-03:EST]','2026-03-23','-03:EST',
  -14.56,'N20CA3:23/03/26:-14.56:1547178','1547178',
  'PIX QR Static Payment To: Nu Pagamentos S/A 03/22',
  'Nu Pagamentos S/A','03/22','PIX QR Static'),

(@StmtId,@catQD,'DEBIT','20260323000000[-03:EST]','2026-03-23','-03:EST',
  -23.00,'N20CC1:23/03/26:-23.0:0826283','0826283',
  'PIX QR Dynamic Payment To: Bar e Lanches 03/23',
  'Bar e Lanches','03/23','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260323000000[-03:EST]','2026-03-23','-03:EST',
  -19.00,'N20CDF:23/03/26:-19.0:1120104','1120104',
  'PIX QR Dynamic Payment To: Ana Maria da Penha 03/21',
  'Ana Maria da Penha','03/21','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260323000000[-03:EST]','2026-03-23','-03:EST',
  -4.49,'N20CFD:23/03/26:-4.49:1125505','1125505',
  'PIX QR Dynamic Payment To: Mercado Saga Ltda 03/21',
  'Mercado Saga Ltda','03/21','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260323000000[-03:EST]','2026-03-23','-03:EST',
  -12.99,'N20D1B:23/03/26:-12.99:1130536','1130536',
  'PIX QR Dynamic Payment To: Horti Fruit Haddock 03/21',
  'Horti Fruit Haddock','03/21','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260323000000[-03:EST]','2026-03-23','-03:EST',
  -5.00,'N20D39:23/03/26:-5.0:1254146','1254146',
  'PIX QR Dynamic Payment To: Auto Posto Grana Ltda 03/22',
  'Auto Posto Grana Ltda','03/22','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260323000000[-03:EST]','2026-03-23','-03:EST',
  -29.90,'N20D57:23/03/26:-29.9:1319118','1319118',
  'PIX QR Dynamic Payment To: Imifarma 03/22',
  'Imifarma','03/22','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260323000000[-03:EST]','2026-03-23','-03:EST',
  -5.49,'N20D75:23/03/26:-5.49:1342111','1342111',
  'PIX QR Dynamic Payment To: Companhia Brasileira 03/22',
  'Companhia Brasileira','03/22','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260323000000[-03:EST]','2026-03-23','-03:EST',
  -59.00,'N20D93:23/03/26:-59.0:1608316','1608316',
  'PIX QR Dynamic Payment To: Boteco Seu Joao II 03/22',
  'Boteco Seu Joao II','03/22','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260323000000[-03:EST]','2026-03-23','-03:EST',
  -5.00,'N20DB1:23/03/26:-5.0:2123324','2123324',
  'PIX QR Dynamic Payment To: Luxo Companhia Das Frutas 03/21',
  'Luxo Companhia Das Frutas','03/21','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260323000000[-03:EST]','2026-03-23','-03:EST',
  -6.00,'N20DCF:23/03/26:-6.0:2157223','2157223',
  'PIX QR Dynamic Payment To: Espaco Moraes 03/21',
  'Espaco Moraes','03/21','PIX QR Dynamic'),

-- ===  03/25/2026  ===
(@StmtId,@catPF,'CREDIT','20260325000000[-03:EST]','2026-03-25','-03:EST',
  142.99,'N20DED:25/03/26:142.99:0951163','0951163',
  'PIX Refund Received From: Pix Marketplace 03/25',
  'Pix Marketplace','03/25','PIX Refund'),

(@StmtId,@catApl,'DEBIT','20260325000000[-03:EST]','2026-03-25','-03:EST',
  -100.98,'N20E0A:25/03/26:-100.98:2642804','2642804',
  'Investment Application Invest Facil',
  NULL,'03/25','Investment Application'),

(@StmtId,@catQD,'DEBIT','20260325000000[-03:EST]','2026-03-25','-03:EST',
  -42.01,'N20E27:25/03/26:-42.01:1343269','1343269',
  'PIX QR Dynamic Payment To: Dany Comercial 03/25',
  'Dany Comercial','03/25','PIX QR Dynamic'),

-- ===  03/26/2026  ===
(@StmtId,@catRes,'CREDIT','20260326000000[-03:EST]','2026-03-26','-03:EST',
  21.16,'N20E45:26/03/26:21.16:2642804','2642804',
  'Investment Redemption Invest Facil',
  NULL,'03/26','Investment Redemption'),

(@StmtId,@catRes,'CREDIT','20260326000000[-03:EST]','2026-03-26','-03:EST',
  32.91,'N20E61:26/03/26:32.91:2832416','2832416',
  'Investment Redemption Invest Facil',
  NULL,'03/26','Investment Redemption'),

(@StmtId,@catQD,'DEBIT','20260326000000[-03:EST]','2026-03-26','-03:EST',
  -44.00,'N20E7D:26/03/26:-44.0:1749429','1749429',
  'PIX QR Dynamic Payment To: Geovan Pereira de Alm 03/26',
  'Geovan Pereira de Alm','03/26','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260326000000[-03:EST]','2026-03-26','-03:EST',
  -10.07,'N20E9B:26/03/26:-10.07:1756097','1756097',
  'PIX QR Dynamic Payment To: Mercado Yab s 03/26',
  'Mercado Yab s','03/26','PIX QR Dynamic'),

-- ===  03/27/2026  ===
(@StmtId,@catRes,'CREDIT','20260327000000[-03:EST]','2026-03-27','-03:EST',
  79.82,'N20EB9:27/03/26:79.82:2642804','2642804',
  'Investment Redemption Invest Facil',
  NULL,'03/27','Investment Redemption'),

(@StmtId,@catQD,'DEBIT','20260327000000[-03:EST]','2026-03-27','-03:EST',
  -31.90,'N20ED5:27/03/26:-31.9:0834266','0834266',
  'PIX QR Dynamic Payment To: Emporio Bar e Lanchonete 03/27',
  'Emporio Bar e Lanchonete','03/27','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260327000000[-03:EST]','2026-03-27','-03:EST',
  -42.00,'N20EF3:27/03/26:-42.0:1327199','1327199',
  'PIX QR Dynamic Payment To: Restaurante Bar e Cafe 03/27',
  'Restaurante Bar e Cafe','03/27','PIX QR Dynamic'),

-- ===  03/31/2026  ===
(@StmtId,@catSal,'CREDIT','20260331000000[-03:EST]','2026-03-31','-03:EST',
  4464.99,'N20F11:31/03/26:4464.99:3100449','3100449',
  'Salary Transfer Bco:237 Ag:00449 Acct:0032016-1',
  'Banco Bradesco Ag.00449 Acct:0032016-1','03/31','Salary Transfer'),

(@StmtId,@catApl,'DEBIT','20260331000000[-03:EST]','2026-03-31','-03:EST',
  -1283.91,'N20F2E:31/03/26:-1283.91:8554299','8554299',
  'Investment Application Invest Facil',
  NULL,'03/31','Investment Application'),

(@StmtId,@catPS,'DEBIT','20260331000000[-03:EST]','2026-03-31','-03:EST',
  -250.00,'N20F4B:31/03/26:-250.0:1207334','1207334',
  'PIX Transfer Sent To: Nerinalva Peixoto 03/31',
  'Nerinalva Peixoto','03/31','PIX Transfer Sent'),

(@StmtId,@catPS,'DEBIT','20260331000000[-03:EST]','2026-03-31','-03:EST',
  -1000.00,'N20F69:31/03/26:-1000.0:1245151','1245151',
  'PIX Transfer Sent To: Katia Cristina Piacenzi 03/31',
  'Katia Cristina Piacenzi','03/31','PIX Transfer Sent'),

(@StmtId,@catPS,'DEBIT','20260331000000[-03:EST]','2026-03-31','-03:EST',
  -1587.00,'N20F87:31/03/26:-1587.0:1250036','1250036',
  'PIX Transfer Sent To: Lazaro Augusto Gusmao 03/31',
  'Lazaro Augusto Gusmao','03/31','PIX Transfer Sent'),

(@StmtId,@catPS,'DEBIT','20260331000000[-03:EST]','2026-03-31','-03:EST',
  -350.00,'N20FA5:31/03/26:-350.0:1303020','1303020',
  'PIX Transfer Sent To: Bruno da Silva 03/31',
  'Bruno da Silva','03/31','PIX Transfer Sent'),

-- ===  04/06/2026  ===
(@StmtId,@catRes,'CREDIT','20260406000000[-03:EST]','2026-04-06','-03:EST',
  361.91,'N20FC3:06/04/26:361.91:8554299','8554299',
  'Investment Redemption Invest Facil',
  NULL,'04/06','Investment Redemption'),

(@StmtId,@catPS,'DEBIT','20260406000000[-03:EST]','2026-04-06','-03:EST',
  -300.00,'N20FDF:06/04/26:-300.0:1003051','1003051',
  'PIX Transfer Sent To: Rodrigo Luiz Madeira 04/04',
  'Rodrigo Luiz Madeira','04/04','PIX Transfer Sent'),

(@StmtId,@catQD,'DEBIT','20260406000000[-03:EST]','2026-04-06','-03:EST',
  -41.11,'N20FFD:06/04/26:-41.11:0753263','0753263',
  'PIX QR Dynamic Payment To: Lanchonete e Panificadora 04/05',
  'Lanchonete e Panificadora','04/05','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260406000000[-03:EST]','2026-04-06','-03:EST',
  -20.80,'N2101B:06/04/26:-20.8:0821378','0821378',
  'PIX QR Dynamic Payment To: Rodosnack Uss Guarare 04/03',
  'Rodosnack Uss Guarare','04/03','PIX QR Dynamic'),

-- ===  04/07/2026  ===
(@StmtId,@catRes,'CREDIT','20260407000000[-03:EST]','2026-04-07','-03:EST',
  164.31,'N21039:07/04/26:164.31:8554299','8554299',
  'Investment Redemption Invest Facil',
  NULL,'04/07','Investment Redemption'),

(@StmtId,@catQD,'DEBIT','20260407000000[-03:EST]','2026-04-07','-03:EST',
  -164.31,'N21055:07/04/26:-164.31:0832511','0832511',
  'PIX QR Dynamic Payment To: Telefonica Brasil 04/07',
  'Telefonica Brasil (Vivo)','04/07','PIX QR Dynamic'),

-- ===  04/08/2026  ===
(@StmtId,@catRes,'CREDIT','20260408000000[-03:EST]','2026-04-08','-03:EST',
  100.00,'N21073:08/04/26:100.0:8554299','8554299',
  'Investment Redemption Invest Facil',
  NULL,'04/08','Investment Redemption'),

(@StmtId,@catQD,'DEBIT','20260408000000[-03:EST]','2026-04-08','-03:EST',
  -100.00,'N2108F:08/04/26:-100.0:1850497','1850497',
  'PIX QR Dynamic Payment To: Auto Posto Bandeira 04/08',
  'Auto Posto Bandeira','04/08','PIX QR Dynamic'),

-- ===  04/09/2026  ===
(@StmtId,@catRes,'CREDIT','20260409000000[-03:EST]','2026-04-09','-03:EST',
  144.91,'N210AD:09/04/26:144.91:8554299','8554299',
  'Investment Redemption Invest Facil',
  NULL,'04/09','Investment Redemption'),

(@StmtId,@catCC,'DEBIT','20260409000000[-03:EST]','2026-04-09','-03:EST',
  -15.00,'N210C9:09/04/26:-15.0:0000014','0000014',
  'Elo Credit Card Purchase ls sp Frei Caneca',
  'ls sp Frei Caneca','04/09','Elo Credit Card'),

(@StmtId,@catQS,'DEBIT','20260409000000[-03:EST]','2026-04-09','-03:EST',
  -129.91,'N210E7:09/04/26:-129.91:0906179','0906179',
  'PIX QR Static Payment To: Claro 04/09',
  'Claro','04/09','PIX QR Static'),

-- ===  04/10/2026  ===
(@StmtId,@catRes,'CREDIT','20260410000000[-03:EST]','2026-04-10','-03:EST',
  12.99,'N21105:10/04/26:12.99:8554299','8554299',
  'Investment Redemption Invest Facil',
  NULL,'04/10','Investment Redemption'),

(@StmtId,@catCC,'DEBIT','20260410000000[-03:EST]','2026-04-10','-03:EST',
  -12.99,'N21121:10/04/26:-12.99:0273633','0273633',
  'Elo Credit Card Purchase Horti Frut Haddock',
  'Horti Frut Haddock','04/10','Elo Credit Card'),

-- ===  04/13/2026  ===
(@StmtId,@catRes,'CREDIT','20260413000000[-03:EST]','2026-04-13','-03:EST',
  458.16,'N2113F:13/04/26:458.16:8554299','8554299',
  'Investment Redemption Invest Facil',
  NULL,'04/13','Investment Redemption'),

(@StmtId,@catYld,'CREDIT','20260413000000[-03:EST]','2026-04-13','-03:EST',
  0.02,'N2115B:13/04/26:0.02:8554299','8554299',
  'Investment Yield Invest Facil',
  NULL,'04/13','Investment Yield'),

(@StmtId,@catCC,'DEBIT','20260413000000[-03:EST]','2026-04-13','-03:EST',
  -10.00,'N21177:13/04/26:-10.0:0021305','0021305',
  'Elo Credit Card Purchase mp *mariabar',
  'mp *mariabar','04/13','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260413000000[-03:EST]','2026-04-13','-03:EST',
  -23.00,'N21195:13/04/26:-23.0:0025291','0025291',
  'Elo Credit Card Purchase mp *padariaguaru',
  'mp *padariaguaru','04/13','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260413000000[-03:EST]','2026-04-13','-03:EST',
  -8.00,'N211B3:13/04/26:-8.0:0025878','0025878',
  'Elo Credit Card Purchase mp *mariabar',
  'mp *mariabar','04/13','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260413000000[-03:EST]','2026-04-13','-03:EST',
  -20.00,'N211D1:13/04/26:-20.0:0090235','0090235',
  'Elo Credit Card Purchase Mp*mariabar',
  'Mp*mariabar','04/13','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260413000000[-03:EST]','2026-04-13','-03:EST',
  -67.59,'N211EF:13/04/26:-67.59:0130040','0130040',
  'Elo Credit Card Purchase Drogaria Sao Paulo',
  'Drogaria Sao Paulo','04/13','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260413000000[-03:EST]','2026-04-13','-03:EST',
  -287.61,'N2120D:13/04/26:-287.61:0285167','0285167',
  'Elo Credit Card Purchase Zig*Quinta do Embaixador',
  'Zig*Quinta do Embaixador','04/13','Elo Credit Card'),

(@StmtId,@catQD,'DEBIT','20260413000000[-03:EST]','2026-04-13','-03:EST',
  -41.98,'N2122B:13/04/26:-41.98:2133176','2133176',
  'PIX QR Dynamic Payment To: iFood.com 04/12',
  'iFood.com','04/12','PIX QR Dynamic'),

-- ===  04/15/2026  ===
(@StmtId,@catSal,'CREDIT','20260415000000[-03:EST]','2026-04-15','-03:EST',
  5280.00,'N21249:15/04/26:5280.0:1500449','1500449',
  'Salary Transfer Bco:237 Ag:00449 Acct:0032016-1',
  'Banco Bradesco Ag.00449 Acct:0032016-1','04/15','Salary Transfer'),

(@StmtId,@catBP,'DEBIT','20260415000000[-03:EST]','2026-04-15','-03:EST',
  -1633.22,'N21266:15/04/26:-1633.22:0000004','0000004',
  'Bill Payment Apartment Rent SP',
  'Apartment Rent Sao Paulo','04/15','Bill Payment'),

(@StmtId,@catApl,'DEBIT','20260415000000[-03:EST]','2026-04-15','-03:EST',
  -1439.26,'N21284:15/04/26:-1439.26:6650372','6650372',
  'Investment Application Invest Facil',
  NULL,'04/15','Investment Application'),

(@StmtId,@catPS,'DEBIT','20260415000000[-03:EST]','2026-04-15','-03:EST',
  -1100.00,'N212A1:15/04/26:-1100.0:1347291','1347291',
  'PIX Transfer Sent To: Katia Cristina Piacenzi 04/15',
  'Katia Cristina Piacenzi','04/15','PIX Transfer Sent'),

(@StmtId,@catQD,'DEBIT','20260415000000[-03:EST]','2026-04-15','-03:EST',
  -1107.52,'N212BF:15/04/26:-1107.52:1042304','1042304',
  'PIX QR Dynamic Payment To: Banco Votorantim S.A. 04/15',
  'Banco Votorantim S.A.','04/15','PIX QR Dynamic'),

-- ===  04/16/2026  ===
(@StmtId,@catRes,'CREDIT','20260416000000[-03:EST]','2026-04-16','-03:EST',
  35.47,'N212DD:16/04/26:35.47:6650372','6650372',
  'Investment Redemption Invest Facil',
  NULL,'04/16','Investment Redemption'),

(@StmtId,@catRes,'CREDIT','20260416000000[-03:EST]','2026-04-16','-03:EST',
  41.63,'N212F9:16/04/26:41.63:8554299','8554299',
  'Investment Redemption Invest Facil',
  NULL,'04/16','Investment Redemption'),

(@StmtId,@catCC,'DEBIT','20260416000000[-03:EST]','2026-04-16','-03:EST',
  -32.06,'N21315:16/04/26:-32.06:0160116','0160116',
  'Elo Credit Card Purchase Drogaria Sao Paulo',
  'Drogaria Sao Paulo','04/16','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260416000000[-03:EST]','2026-04-16','-03:EST',
  -15.56,'N21333:16/04/26:-15.56:0500920','0500920',
  'Elo Credit Card Purchase Dany Com Alimentos',
  'Dany Com Alimentos','04/16','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260416000000[-03:EST]','2026-04-16','-03:EST',
  -29.48,'N21351:16/04/26:-29.48:0579900','0579900',
  'Elo Credit Card Purchase Frutaria Bela Vista',
  'Frutaria Bela Vista','04/16','Elo Credit Card'),

-- ===  04/17/2026  ===
(@StmtId,@catRes,'CREDIT','20260417000000[-03:EST]','2026-04-17','-03:EST',
  89.30,'N2136F:17/04/26:89.3:6650372','6650372',
  'Investment Redemption Invest Facil',
  NULL,'04/17','Investment Redemption'),

(@StmtId,@catCC,'DEBIT','20260417000000[-03:EST]','2026-04-17','-03:EST',
  -20.00,'N2138B:17/04/26:-20.0:0193288','0193288',
  'Elo Credit Card Purchase Ursulamatie',
  'Ursulamatie','04/17','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260417000000[-03:EST]','2026-04-17','-03:EST',
  -44.00,'N213A9:17/04/26:-44.0:0510649','0510649',
  'Elo Credit Card Purchase Fox Beer',
  'Fox Beer','04/17','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260417000000[-03:EST]','2026-04-17','-03:EST',
  -25.30,'N213C7:17/04/26:-25.3:0745722','0745722',
  'Elo Credit Card Purchase Eleven Bar e Lanches',
  'Eleven Bar e Lanches','04/17','Elo Credit Card'),

-- ===  04/20/2026  ===
(@StmtId,@catRes,'CREDIT','20260420000000[-03:EST]','2026-04-20','-03:EST',
  325.44,'N213E5:20/04/26:325.44:6650372','6650372',
  'Investment Redemption Invest Facil',
  NULL,'04/20','Investment Redemption'),

(@StmtId,@catCC,'DEBIT','20260420000000[-03:EST]','2026-04-20','-03:EST',
  -34.68,'N21401:20/04/26:-34.68:0023665','0023665',
  'Elo Credit Card Purchase Lanchonete e Panificadora',
  'Lanchonete e Panificadora','04/20','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260420000000[-03:EST]','2026-04-20','-03:EST',
  -19.46,'N2141F:20/04/26:-19.46:0071668','0071668',
  'Elo Credit Card Purchase Farmaconde',
  'Farmaconde','04/20','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260420000000[-03:EST]','2026-04-20','-03:EST',
  -100.00,'N2143D:20/04/26:-100.0:0095313','0095313',
  'Elo Credit Card Purchase Autoposto',
  'Autoposto','04/20','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260420000000[-03:EST]','2026-04-20','-03:EST',
  -27.50,'N2145B:20/04/26:-27.5:0221111','0221111',
  'Elo Credit Card Purchase Panificadora e Confeitaria',
  'Panificadora e Confeitaria','04/20','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260420000000[-03:EST]','2026-04-20','-03:EST',
  -143.80,'N21479:20/04/26:-143.8:0358705','0358705',
  'Elo Credit Card Purchase Portal do Jaguari',
  'Portal do Jaguari','04/20','Elo Credit Card'),

-- ===  04/22/2026  ===
(@StmtId,@catRes,'CREDIT','20260422000000[-03:EST]','2026-04-22','-03:EST',
  703.63,'N21497:22/04/26:703.63:6650372','6650372',
  'Investment Redemption Invest Facil',
  NULL,'04/22','Investment Redemption'),

(@StmtId,@catYld,'CREDIT','20260422000000[-03:EST]','2026-04-22','-03:EST',
  0.01,'N214B3:22/04/26:0.01:6650372','6650372',
  'Investment Yield Invest Facil',
  NULL,'04/22','Investment Yield'),

(@StmtId,@catCC,'DEBIT','20260422000000[-03:EST]','2026-04-22','-03:EST',
  -18.00,'N214CF:22/04/26:-18.0:0024680','0024680',
  'Elo Credit Card Purchase Skina do Bexiga',
  'Skina do Bexiga','04/22','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260422000000[-03:EST]','2026-04-22','-03:EST',
  -49.00,'N214ED:22/04/26:-49.0:0190659','0190659',
  'Elo Credit Card Purchase Cachacaria Salinas',
  'Cachacaria Salinas','04/22','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260422000000[-03:EST]','2026-04-22','-03:EST',
  -80.59,'N2150B:22/04/26:-80.59:0267123','0267123',
  'Elo Credit Card Purchase Extra Hiper',
  'Extra Hiper','04/22','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260422000000[-03:EST]','2026-04-22','-03:EST',
  -31.90,'N21529:22/04/26:-31.9:0445795','0445795',
  'Elo Credit Card Purchase Faccini Point',
  'Faccini Point','04/22','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260422000000[-03:EST]','2026-04-22','-03:EST',
  -20.00,'N21547:22/04/26:-20.0:0452212','0452212',
  'Elo Credit Card Purchase Laundry Service',
  'Laundry Service','04/22','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260422000000[-03:EST]','2026-04-22','-03:EST',
  -110.20,'N21565:22/04/26:-110.2:0541409','0541409',
  'Elo Credit Card Purchase Praia 33 Bar',
  'Praia 33 Bar','04/22','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260422000000[-03:EST]','2026-04-22','-03:EST',
  -27.00,'N21583:22/04/26:-27.0:0589028','0589028',
  'Elo Credit Card Purchase Sant Suco',
  'Sant Suco','04/22','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260422000000[-03:EST]','2026-04-22','-03:EST',
  -8.98,'N215A1:22/04/26:-8.98:0710373','0710373',
  'Elo Credit Card Purchase Helio Yabuta',
  'Helio Yabuta','04/22','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260422000000[-03:EST]','2026-04-22','-03:EST',
  -19.99,'N215BF:22/04/26:-19.99:0915863','0915863',
  'Elo Credit Card Purchase Laundry Service',
  'Laundry Service','04/22','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260422000000[-03:EST]','2026-04-22','-03:EST',
  -40.00,'N215DD:22/04/26:-40.0:0991366','0991366',
  'Elo Credit Card Purchase Praia 33 Bar',
  'Praia 33 Bar','04/22','Elo Credit Card'),

(@StmtId,@catPS,'DEBIT','20260422000000[-03:EST]','2026-04-22','-03:EST',
  -250.00,'N215FB:22/04/26:-250.0:1258093','1258093',
  'PIX Transfer Sent To: Bruno da Silva 04/22',
  'Bruno da Silva','04/22','PIX Transfer Sent'),

(@StmtId,@catQS,'DEBIT','20260422000000[-03:EST]','2026-04-22','-03:EST',
  -20.00,'N21619:22/04/26:-20.0:1926013','1926013',
  'PIX QR Static Payment To: Joao Vitor Caldeira 04/21',
  'Joao Vitor Caldeira','04/21','PIX QR Static'),

(@StmtId,@catQD,'DEBIT','20260422000000[-03:EST]','2026-04-22','-03:EST',
  -27.98,'N21637:22/04/26:-27.98:1830393','1830393',
  'PIX QR Dynamic Payment To: Horti Fruit Haddock 04/22',
  'Horti Fruit Haddock','04/22','PIX QR Dynamic'),

-- ===  04/23/2026  ===
(@StmtId,@catRes,'CREDIT','20260423000000[-03:EST]','2026-04-23','-03:EST',
  131.47,'N21655:23/04/26:131.47:6650372','6650372',
  'Investment Redemption Invest Facil',
  NULL,'04/23','Investment Redemption'),

(@StmtId,@catCC,'DEBIT','20260423000000[-03:EST]','2026-04-23','-03:EST',
  -16.99,'N21671:23/04/26:-16.99:0350466','0350466',
  'Elo Credit Card Purchase Docemar',
  'Docemar','04/23','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260423000000[-03:EST]','2026-04-23','-03:EST',
  -40.00,'N2168F:23/04/26:-40.0:0783864','0783864',
  'Elo Credit Card Purchase Ana Maria da Penha',
  'Ana Maria da Penha','04/23','Elo Credit Card'),

(@StmtId,@catQS,'DEBIT','20260423000000[-03:EST]','2026-04-23','-03:EST',
  -10.00,'N216AD:23/04/26:-10.0:1640222','1640222',
  'PIX QR Static Payment To: Mercado Bitcoin 04/23',
  'Mercado Bitcoin','04/23','PIX QR Static'),

(@StmtId,@catQD,'DEBIT','20260423000000[-03:EST]','2026-04-23','-03:EST',
  -10.00,'N216CB:23/04/26:-10.0:0754027','0754027',
  'PIX QR Dynamic Payment To: Ana Maria da Penha 04/23',
  'Ana Maria da Penha','04/23','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260423000000[-03:EST]','2026-04-23','-03:EST',
  -40.00,'N216E9:23/04/26:-40.0:0818577','0818577',
  'PIX QR Dynamic Payment To: Nic.br 04/23',
  'Nic.br','04/23','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260423000000[-03:EST]','2026-04-23','-03:EST',
  -14.48,'N21707:23/04/26:-14.48:2033460','2033460',
  'PIX QR Dynamic Payment To: Dany Comercial 04/23',
  'Dany Comercial','04/23','PIX QR Dynamic'),

-- ===  04/24/2026  ===
(@StmtId,@catRes,'CREDIT','20260424000000[-03:EST]','2026-04-24','-03:EST',
  35.30,'N21725:24/04/26:35.3:6650372','6650372',
  'Investment Redemption Invest Facil',
  NULL,'04/24','Investment Redemption'),

(@StmtId,@catCC,'DEBIT','20260424000000[-03:EST]','2026-04-24','-03:EST',
  -26.40,'N21741:24/04/26:-26.4:0728965','0728965',
  'Elo Credit Card Purchase Flor da Joana',
  'Flor da Joana','04/24','Elo Credit Card'),

(@StmtId,@catQD,'DEBIT','20260424000000[-03:EST]','2026-04-24','-03:EST',
  -8.90,'N2175F:24/04/26:-8.9:1921047','1921047',
  'PIX QR Dynamic Payment To: Ninai Salgados 04/24',
  'Ninai Salgados','04/24','PIX QR Dynamic'),

-- ===  04/27/2026  ===
(@StmtId,@catRes,'CREDIT','20260427000000[-03:EST]','2026-04-27','-03:EST',
  81.51,'N2177D:27/04/26:81.51:6650372','6650372',
  'Investment Redemption Invest Facil',
  NULL,'04/27','Investment Redemption'),

(@StmtId,@catCC,'DEBIT','20260427000000[-03:EST]','2026-04-27','-03:EST',
  -14.98,'N21799:27/04/26:-14.98:0711187','0711187',
  'Elo Credit Card Purchase Helio Yabuta',
  'Helio Yabuta','04/27','Elo Credit Card'),

(@StmtId,@catQD,'DEBIT','20260427000000[-03:EST]','2026-04-27','-03:EST',
  -17.00,'N217B7:27/04/26:-17.0:0835132','0835132',
  'PIX QR Dynamic Payment To: Rcm Confeitaria Ltda 04/25',
  'Rcm Confeitaria Ltda','04/25','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260427000000[-03:EST]','2026-04-27','-03:EST',
  -13.00,'N217D5:27/04/26:-13.0:0907576','0907576',
  'PIX QR Dynamic Payment To: Bar e Lanches 04/26',
  'Bar e Lanches','04/26','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260427000000[-03:EST]','2026-04-27','-03:EST',
  -4.50,'N217F3:27/04/26:-4.5:0934326','0934326',
  'PIX QR Dynamic Payment To: Aparecido Flauzino 04/26',
  'Aparecido Flauzino','04/26','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260427000000[-03:EST]','2026-04-27','-03:EST',
  -7.90,'N21811:27/04/26:-7.9:1027502','1027502',
  'PIX QR Dynamic Payment To: Mercado Adriata 04/26',
  'Mercado Adriata','04/26','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260427000000[-03:EST]','2026-04-27','-03:EST',
  -10.00,'N2182F:27/04/26:-10.0:1218072','1218072',
  'PIX QR Dynamic Payment To: Pagar.me Pagamentos 04/25',
  'Pagar.me Pagamentos','04/25','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260427000000[-03:EST]','2026-04-27','-03:EST',
  -9.00,'N2184D:27/04/26:-9.0:1222544','1222544',
  'PIX QR Dynamic Payment To: Jorge Fernandes 04/25',
  'Jorge Fernandes','04/25','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260427000000[-03:EST]','2026-04-27','-03:EST',
  -5.13,'N2186B:27/04/26:-5.13:1841323','1841323',
  'PIX QR Dynamic Payment To: Dany Comercial 04/25',
  'Dany Comercial','04/25','PIX QR Dynamic'),

-- ===  04/28/2026  ===
(@StmtId,@catRes,'CREDIT','20260428000000[-03:EST]','2026-04-28','-03:EST',
  37.14,'N21889:28/04/26:37.14:6650372','6650372',
  'Investment Redemption Invest Facil',
  NULL,'04/28','Investment Redemption'),

(@StmtId,@catCC,'DEBIT','20260428000000[-03:EST]','2026-04-28','-03:EST',
  -32.40,'N218A5:28/04/26:-32.4:0154017','0154017',
  'Elo Credit Card Purchase Flor da Joana',
  'Flor da Joana','04/28','Elo Credit Card'),

-- ===  04/30/2026  ===
(@StmtId,@catSal,'CREDIT','20260430000000[-03:EST]','2026-04-30','-03:EST',
  4345.09,'N218C3:30/04/26:4345.09:3000449','3000449',
  'Salary Transfer Bco:237 Ag:00449 Acct:0032016-1',
  'Banco Bradesco Ag.00449 Acct:0032016-1','04/30','Salary Transfer'),

(@StmtId,@catCC,'DEBIT','20260430000000[-03:EST]','2026-04-30','-03:EST',
  -7.99,'N218E0:30/04/26:-7.99:0005616','0005616',
  'Elo Credit Card Purchase Portal Leste',
  'Portal Leste','04/30','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260430000000[-03:EST]','2026-04-30','-03:EST',
  -200.00,'N218FE:30/04/26:-200.0:0016990','0016990',
  'Elo Credit Card Purchase Portal Leste',
  'Portal Leste','04/30','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260430000000[-03:EST]','2026-04-30','-03:EST',
  -9.00,'N2191C:30/04/26:-9.0:0865302','0865302',
  'Elo Credit Card Purchase Rcm Confeitaria',
  'Rcm Confeitaria','04/30','Elo Credit Card'),

(@StmtId,@catApl,'DEBIT','20260430000000[-03:EST]','2026-04-30','-03:EST',
  -1849.21,'N2193A:30/04/26:-1849.21:1373725','1373725',
  'Investment Application Invest Facil',
  NULL,'04/30','Investment Application'),

(@StmtId,@catPS,'DEBIT','20260430000000[-03:EST]','2026-04-30','-03:EST',
  -1000.00,'N21957:30/04/26:-1000.0:0953324','0953324',
  'PIX Transfer Sent To: Lazaro Augusto Gusmao 04/30',
  'Lazaro Augusto Gusmao','04/30','PIX Transfer Sent'),

(@StmtId,@catQS,'DEBIT','20260430000000[-03:EST]','2026-04-30','-03:EST',
  -1000.00,'N21975:30/04/26:-1000.0:1217409','1217409',
  'PIX QR Static Payment To: Mercado Bitcoin 04/30',
  'Mercado Bitcoin','04/30','PIX QR Static'),

(@StmtId,@catQD,'DEBIT','20260430000000[-03:EST]','2026-04-30','-03:EST',
  -119.37,'N21993:30/04/26:-119.37:1614353','1614353',
  'PIX QR Dynamic Payment To: Sem Parar Instituicao 04/30',
  'Sem Parar Instituicao','04/30','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260430000000[-03:EST]','2026-04-30','-03:EST',
  -164.26,'N219B1:30/04/26:-164.26:1750549','1750549',
  'PIX QR Dynamic Payment To: Telefonica Brasil 04/30',
  'Telefonica Brasil (Vivo)','04/30','PIX QR Dynamic'),

-- ===  05/04/2026  ===
(@StmtId,@catRes,'CREDIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  935.09,'N219CF:04/05/26:935.09:1373725','1373725',
  'Investment Redemption Invest Facil',
  NULL,'05/04','Investment Redemption'),

(@StmtId,@catPI,'CREDIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  4464.37,'N219EB:04/05/26:4464.37:0846481','0846481',
  'PIX Transfer Received From: Rodrigo Furlaneti 05/04',
  'Rodrigo Furlaneti','05/04','PIX Transfer Received'),

(@StmtId,@catBP,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -975.28,'N21A08:04/05/26:-975.28:0000005','0000005',
  'Bill Payment Banco Votorantim S/A',
  'Banco Votorantim S/A','05/04','Bill Payment'),

(@StmtId,@catBP,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -1260.88,'N21A26:04/05/26:-1260.88:0000006','0000006',
  'Bill Payment Pefisa S.A.-CFI',
  'Pefisa S.A.-CFI','05/04','Bill Payment'),

(@StmtId,@catBP,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -85.80,'N21A44:04/05/26:-85.8:0000007','0000007',
  'Bill Payment Nu Pagamentos S.A.',
  'Nu Pagamentos S.A.','05/04','Bill Payment'),

(@StmtId,@catBP,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -1523.95,'N21A62:04/05/26:-1523.95:0000008','0000008',
  'Bill Payment GCI Caixa - Housing Loan',
  'GCI Caixa Housing Loan','05/04','Bill Payment'),

(@StmtId,@catCC,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -40.00,'N21A80:04/05/26:-40.0:0025809','0025809',
  'Elo Credit Card Purchase Meeting Point',
  'Meeting Point','05/04','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -17.98,'N21A9E:04/05/26:-17.98:0274800','0274800',
  'Elo Credit Card Purchase Horti Frut Haddock',
  'Horti Frut Haddock','05/04','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -43.00,'N21ABC:04/05/26:-43.0:0400256','0400256',
  'Elo Credit Card Purchase Cachacaria Salinas',
  'Cachacaria Salinas','05/04','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -4.99,'N21ADA:04/05/26:-4.99:0580429','0580429',
  'Elo Credit Card Purchase Frutaria Bela Vista',
  'Frutaria Bela Vista','05/04','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -8.57,'N21AF8:04/05/26:-8.57:0711709','0711709',
  'Elo Credit Card Purchase Helio Yabuta',
  'Helio Yabuta','05/04','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -33.70,'N21B16:04/05/26:-33.7:0939745','0939745',
  'Elo Credit Card Purchase Panificadora e Confeitaria',
  'Panificadora e Confeitaria','05/04','Elo Credit Card'),

(@StmtId,@catPS,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -250.00,'N21B34:04/05/26:-250.0:1049098','1049098',
  'PIX Transfer Sent To: Nerinalva Peixoto 05/01',
  'Nerinalva Peixoto','05/01','PIX Transfer Sent'),

(@StmtId,@catQS,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -240.22,'N21B52:04/05/26:-240.22:0908408','0908408',
  'PIX QR Static Payment To: Pagueveloz 05/04',
  'Pagueveloz','05/04','PIX QR Static'),

(@StmtId,@catQS,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -57.30,'N21B70:04/05/26:-57.3:0912194','0912194',
  'PIX QR Static Payment To: Nu Pagamentos S/A 05/04',
  'Nu Pagamentos S/A','05/04','PIX QR Static'),

(@StmtId,@catQS,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -92.35,'N21B8E:04/05/26:-92.35:0920004','0920004',
  'PIX QR Static Payment To: Nu Pagamentos S/A 05/04',
  'Nu Pagamentos S/A','05/04','PIX QR Static'),

(@StmtId,@catQD,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -191.66,'N21BAC:04/05/26:-191.66:0653055','0653055',
  'PIX QR Dynamic Payment To: FAM Centro Universitario 05/02',
  'FAM Centro Universitario','05/02','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -160.36,'N21BCA:04/05/26:-160.36:0653515','0653515',
  'PIX QR Dynamic Payment To: FAM Centro Universitario 05/02',
  'FAM Centro Universitario','05/02','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -18.11,'N21BE8:04/05/26:-18.11:0850372','0850372',
  'PIX QR Dynamic Payment To: Pamela Nobrega 05/03',
  'Pamela Nobrega','05/03','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -189.92,'N21C06:04/05/26:-189.92:1133324','1133324',
  'PIX QR Dynamic Payment To: FAM Centro Universitario 05/04',
  'FAM Centro Universitario','05/04','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -158.91,'N21C24:04/05/26:-158.91:1134226','1134226',
  'PIX QR Dynamic Payment To: FAM Centro Universitario 05/04',
  'FAM Centro Universitario','05/04','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -12.90,'N21C42:04/05/26:-12.9:1231049','1231049',
  'PIX QR Dynamic Payment To: Pamela Nobrega 05/02',
  'Pamela Nobrega','05/02','PIX QR Dynamic'),

(@StmtId,@catQD,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -33.58,'N21C60:04/05/26:-33.58:1830105','1830105',
  'PIX QR Dynamic Payment To: iFood.com 05/03',
  'iFood.com','05/03','PIX QR Dynamic'),

-- ===  05/05/2026  ===
(@StmtId,@catRes,'CREDIT','20260505000000[-03:EST]','2026-05-05','-03:EST',
  108.99,'N21C7E:05/05/26:108.99:1373725','1373725',
  'Investment Redemption Invest Facil',
  NULL,'05/05','Investment Redemption'),

(@StmtId,@catCC,'DEBIT','20260505000000[-03:EST]','2026-05-05','-03:EST',
  -25.00,'N21C9A:05/05/26:-25.0:0022437','0022437',
  'Elo Credit Card Purchase Skina do Bexiga',
  'Skina do Bexiga','05/05','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260505000000[-03:EST]','2026-05-05','-03:EST',
  -30.00,'N21CB8:05/05/26:-30.0:0126287','0126287',
  'Elo Credit Card Purchase Joia da Vergueiro',
  'Joia da Vergueiro','05/05','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260505000000[-03:EST]','2026-05-05','-03:EST',
  -4.99,'N21CD6:05/05/26:-4.99:0274868','0274868',
  'Elo Credit Card Purchase Horti Frut Haddock',
  'Horti Frut Haddock','05/05','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260505000000[-03:EST]','2026-05-05','-03:EST',
  -49.00,'N21CF4:05/05/26:-49.0:0416836','0416836',
  'Elo Credit Card Purchase Cachacaria Salinas',
  'Cachacaria Salinas','05/05','Elo Credit Card'),

-- ===  05/06/2026  ===
(@StmtId,@catRes,'CREDIT','20260506000000[-03:EST]','2026-05-06','-03:EST',
  755.85,'N21D12:06/05/26:755.85:1373725','1373725',
  'Investment Redemption Invest Facil',
  NULL,'05/06','Investment Redemption'),

(@StmtId,@catYld,'CREDIT','20260506000000[-03:EST]','2026-05-06','-03:EST',
  0.01,'N21D2E:06/05/26:0.01:1373725','1373725',
  'Investment Yield Invest Facil',
  NULL,'05/06','Investment Yield'),

(@StmtId,@catBP,'DEBIT','20260506000000[-03:EST]','2026-05-06','-03:EST',
  -362.50,'N21D4A:06/05/26:-362.5:0000009','0000009',
  'Bill Payment Accountant Ana',
  'Accountant Ana','05/06','Bill Payment'),

(@StmtId,@catCC,'DEBIT','20260506000000[-03:EST]','2026-05-06','-03:EST',
  -35.20,'N21D68:06/05/26:-35.2:0266862','0266862',
  'Elo Credit Card Purchase Flor da Joana',
  'Flor da Joana','05/06','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260506000000[-03:EST]','2026-05-06','-03:EST',
  -10.49,'N21D86:06/05/26:-10.49:0274933','0274933',
  'Elo Credit Card Purchase Horti Frut Haddock',
  'Horti Frut Haddock','05/06','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260506000000[-03:EST]','2026-05-06','-03:EST',
  -3.50,'N21DA4:06/05/26:-3.5:0359710','0359710',
  'Elo Credit Card Purchase Docemar',
  'Docemar','05/06','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260506000000[-03:EST]','2026-05-06','-03:EST',
  -34.26,'N21DC2:06/05/26:-34.26:0501087','0501087',
  'Elo Credit Card Purchase Dany Com Alimentos',
  'Dany Com Alimentos','05/06','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260506000000[-03:EST]','2026-05-06','-03:EST',
  -45.00,'N21DE0:06/05/26:-45.0:0870997','0870997',
  'Elo Credit Card Purchase Noels Bar',
  'Noels Bar','05/06','Elo Credit Card'),

(@StmtId,@catPS,'DEBIT','20260506000000[-03:EST]','2026-05-06','-03:EST',
  -150.00,'N21DFE:06/05/26:-150.0:1035289','1035289',
  'PIX Transfer Sent To: Ana Caroline Dos Reis 05/06',
  'Ana Caroline Dos Reis','05/06','PIX Transfer Sent'),

(@StmtId,@catQS,'DEBIT','20260506000000[-03:EST]','2026-05-06','-03:EST',
  -114.91,'N21E1C:06/05/26:-114.91:0933147','0933147',
  'PIX QR Static Payment To: Enel Distribuicao Sao Paulo 05/06',
  'Enel Distribuicao Sao Paulo','05/06','PIX QR Static'),

-- ===  05/07/2026  ===
(@StmtId,@catRes,'CREDIT','20260507000000[-03:EST]','2026-05-07','-03:EST',
  49.28,'N21E3A:07/05/26:49.28:1373725','1373725',
  'Investment Redemption Invest Facil',
  NULL,'05/07','Investment Redemption'),

(@StmtId,@catCC,'DEBIT','20260507000000[-03:EST]','2026-05-07','-03:EST',
  -16.14,'N21E56:07/05/26:-16.14:0000705','0000705',
  'Elo Credit Card Purchase Atacadao 623',
  'Atacadao 623','05/07','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260507000000[-03:EST]','2026-05-07','-03:EST',
  -4.99,'N21E74:07/05/26:-4.99:0274987','0274987',
  'Elo Credit Card Purchase Horti Frut Haddock',
  'Horti Frut Haddock','05/07','Elo Credit Card'),

(@StmtId,@catCC,'DEBIT','20260507000000[-03:EST]','2026-05-07','-03:EST',
  -18.89,'N21E92:07/05/26:-18.89:0331884','0331884',
  'Elo Credit Card Purchase Gop*Emporio Guarulhos',
  'Gop*Emporio Guarulhos','05/07','Elo Credit Card'),

(@StmtId,@catRes,'CREDIT','20260507000000[-03:EST]','2026-05-07','-03:EST',
  49.28,'N20023:07/05/26:49.28:1373725','1373725',
  'Investment Redemption Invest Facil (duplicate entry in source)',
  NULL,'05/07','Investment Redemption'),

-- ===  05/08/2026  ===
(@StmtId,@catPI,'CREDIT','20260508000000[-03:EST]','2026-05-08','-03:EST',
  400.00,'N2003F:08/05/26:400.0:1330503','1330503',
  'PIX Transfer Received From: Mercado Bitcoin ip lt 05/08',
  'Mercado Bitcoin ip lt','05/08','PIX Transfer Received'),

(@StmtId,@catCD,'DEBIT','20260508000000[-03:EST]','2026-05-08','-03:EST',
  -139.99,'N2005C:08/05/26:-139.99:0027831','0027831',
  'Elo Debit Card Purchase Shopping Guarulhos',
  'Shopping Guarulhos','05/08','Elo Debit Card'),

(@StmtId,@catCD,'DEBIT','20260508000000[-03:EST]','2026-05-08','-03:EST',
  -18.00,'N2007A:08/05/26:-18.0:0080915','0080915',
  'Elo Debit Card Purchase Consorcio Shop Guarulhos',
  'Consorcio Shop Guarulhos','05/08','Elo Debit Card'),

(@StmtId,@catCD,'DEBIT','20260508000000[-03:EST]','2026-05-08','-03:EST',
  -16.00,'N20098:08/05/26:-16.0:0257324','0257324',
  'Elo Debit Card Purchase Budega Acopiara',
  'Budega Acopiara','05/08','Elo Debit Card'),

(@StmtId,@catCD,'DEBIT','20260508000000[-03:EST]','2026-05-08','-03:EST',
  -71.45,'N200B6:08/05/26:-71.45:0947463','0947463',
  'Elo Debit Card Purchase Bar Augusta Ltda',
  'Bar Augusta Ltda','05/08','Elo Debit Card'),

(@StmtId,@catCD,'DEBIT','20260508000000[-03:EST]','2026-05-08','-03:EST',
  -16.99,'N200D4:08/05/26:-16.99:0974791','0974791',
  'Elo Debit Card Purchase Bar Augusta Ltda',
  'Bar Augusta Ltda','05/08','Elo Debit Card'),

(@StmtId,@catAA,'DEBIT','20260508000000[-03:EST]','2026-05-08','-03:EST',
  -146.83,'N200F2:08/05/26:-146.83:0080526','0080526',
  'Automatic Investment Application Invest Facil',
  NULL,'05/08','Automatic Investment Application'),

-- ===  05/11/2026  ===
(@StmtId,@catCD,'DEBIT','20260511000000[-03:EST]','2026-05-11','-03:EST',
  -18.98,'N2010F:11/05/26:-18.98:0000076','0000076',
  'Elo Debit Card Purchase Atacadao 623',
  'Atacadao 623','05/11','Elo Debit Card'),

(@StmtId,@catCD,'DEBIT','20260511000000[-03:EST]','2026-05-11','-03:EST',
  -18.99,'N2012D:11/05/26:-18.99:0102260','0102260',
  'Elo Debit Card Purchase Mini Extra 5087',
  'Mini Extra 5087','05/11','Elo Debit Card'),

(@StmtId,@catCD,'DEBIT','20260511000000[-03:EST]','2026-05-11','-03:EST',
  -12.00,'N2014B:11/05/26:-12.0:0952238','0952238',
  'Elo Debit Card Purchase Sams Guarulhos',
  'Sams Guarulhos','05/11','Elo Debit Card');
GO

COMMIT TRANSACTION;

-- ============================================================
-- QUICK VALIDATION
-- ============================================================
SELECT 'TransactionCategories' AS Entity, COUNT(*) AS Total FROM ofx.OFX_TransactionCategory
UNION ALL SELECT 'Banks',          COUNT(*) FROM ofx.OFX_Bank
UNION ALL SELECT 'Imports',        COUNT(*) FROM ofx.OFX_Import
UNION ALL SELECT 'SignonSessions', COUNT(*) FROM ofx.OFX_SignonSession
UNION ALL SELECT 'Accounts',       COUNT(*) FROM ofx.OFX_Account
UNION ALL SELECT 'Statements',     COUNT(*) FROM ofx.OFX_Statement
UNION ALL SELECT 'LedgerBalances', COUNT(*) FROM ofx.OFX_LedgerBalance
UNION ALL SELECT 'Transactions',   COUNT(*) FROM ofx.OFX_Transaction;
GO

-- Accounting summary by category
SELECT
    cat.Code                    AS Category,
    cat.AccountingNature,
    COUNT(*)                    AS TransactionCount,
    SUM(t.AbsoluteAmount)       AS TotalAmount
FROM ofx.OFX_Transaction t
JOIN ofx.OFX_TransactionCategory cat ON cat.Id = t.CategoryId
GROUP BY cat.Code, cat.AccountingNature
ORDER BY cat.AccountingNature, TotalAmount DESC;
GO

PRINT 'Seed data inserted successfully.';
GO
