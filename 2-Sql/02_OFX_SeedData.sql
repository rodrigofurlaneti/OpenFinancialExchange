-- ============================================================
-- OPEN FINANCIAL EXCHANGE - SEED DATA
-- File        : 02_OFX_SeedData.sql
-- Description : Full data population extracted from
--               Bradesco.ofx (statement Jan-May/2026).
--               Run AFTER 01_OFX_Schema.sql
--
-- NOTE: Memo, PayeeName and OperationSubtype fields contain
--       the original OFX file text exactly as-is.
--       Table and column names are in English.
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
  ('SALARY_TRANSFER',   'Salary Transfer to Checking Account',   'SALARY_TRANSFER',        'REVENUE'),
  ('INVEST_APPLICATION','Investment Application - Invest Facil', 'INVESTMENT_APPLICATION',  'EXPENSE'),
  ('INVEST_REDEMPTION', 'Investment Redemption - Invest Facil',  'INVESTMENT_REDEMPTION',   'REVENUE'),
  ('INVEST_YIELD',      'Investment Yield - Invest Facil',       'INVESTMENT_YIELD',        'REVENUE'),
  ('PIX_SENT',          'PIX Transfer Sent',                     'PIX_SENT',                'EXPENSE'),
  ('PIX_RECEIVED',      'PIX Transfer Received',                 'PIX_RECEIVED',            'REVENUE'),
  ('PIX_REFUND',        'PIX Refund Received',                   'PIX_REFUND',              'REVENUE'),
  ('PIX_QR_DYNAMIC',    'PIX Dynamic QR Code Payment',           'PIX_QRCODE_DYNAMIC',      'EXPENSE'),
  ('PIX_QR_STATIC',     'PIX Static QR Code Payment',            'PIX_QRCODE_STATIC',       'EXPENSE'),
  ('BILL_PAYMENT',      'Bill / Boleto Payment',                 'BILL_PAYMENT',            'EXPENSE'),
  ('CARD_CREDIT_ELO',   'Elo Credit Card Purchase',              'CARD_PURCHASE_CREDIT',    'EXPENSE'),
  ('CARD_DEBIT_ELO',    'Elo Debit Card Purchase',               'CARD_PURCHASE_DEBIT',     'EXPENSE'),
  ('INVEST_AUTO_APP',   'Automatic Investment Application',       'INVESTMENT_APPLICATION',  'EXPENSE'),
  ('PIX_TRANSFER_IN',   'Incoming PIX Transfer (Remitter)',       'PIX_RECEIVED',            'REVENUE');
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
    (SELECT TOP 1 Id FROM ofx.OFX_Import   ORDER BY Id DESC),
    (SELECT Id        FROM ofx.OFX_Bank    WHERE COMPECode = '0237'),
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
-- 8. TRANSACTIONS  (all 227 entries from Bradesco.ofx)
--    Memo          = original OFX <MEMO> value
--    OperationSubtype = operation prefix as found in MEMO
--    PayeeName     = beneficiary extracted from MEMO Des:/Rem:
--    TransactionDateMemo = date in MEMO (dd/mm as in source)
-- ============================================================

DECLARE @StmtId  INT = (SELECT TOP 1 Id FROM ofx.OFX_Statement ORDER BY Id DESC);

DECLARE @catSal  INT = (SELECT Id FROM ofx.OFX_TransactionCategory WHERE Code = 'SALARY_TRANSFER');
DECLARE @catApl  INT = (SELECT Id FROM ofx.OFX_TransactionCategory WHERE Code = 'INVEST_APPLICATION');
DECLARE @catRes  INT = (SELECT Id FROM ofx.OFX_TransactionCategory WHERE Code = 'INVEST_REDEMPTION');
DECLARE @catYld  INT = (SELECT Id FROM ofx.OFX_TransactionCategory WHERE Code = 'INVEST_YIELD');
DECLARE @catPS   INT = (SELECT Id FROM ofx.OFX_TransactionCategory WHERE Code = 'PIX_SENT');
DECLARE @catPR   INT = (SELECT Id FROM ofx.OFX_TransactionCategory WHERE Code = 'PIX_RECEIVED');
DECLARE @catPF   INT = (SELECT Id FROM ofx.OFX_TransactionCategory WHERE Code = 'PIX_REFUND');
DECLARE @catQD   INT = (SELECT Id FROM ofx.OFX_TransactionCategory WHERE Code = 'PIX_QR_DYNAMIC');
DECLARE @catQS   INT = (SELECT Id FROM ofx.OFX_TransactionCategory WHERE Code = 'PIX_QR_STATIC');
DECLARE @catBP   INT = (SELECT Id FROM ofx.OFX_TransactionCategory WHERE Code = 'BILL_PAYMENT');
DECLARE @catCC   INT = (SELECT Id FROM ofx.OFX_TransactionCategory WHERE Code = 'CARD_CREDIT_ELO');
DECLARE @catCD   INT = (SELECT Id FROM ofx.OFX_TransactionCategory WHERE Code = 'CARD_DEBIT_ELO');
DECLARE @catAA   INT = (SELECT Id FROM ofx.OFX_TransactionCategory WHERE Code = 'INVEST_AUTO_APP');
DECLARE @catPI   INT = (SELECT Id FROM ofx.OFX_TransactionCategory WHERE Code = 'PIX_TRANSFER_IN');

INSERT INTO ofx.OFX_Transaction
    (StatementId, CategoryId, TransactionType, PostedDateRaw, PostedDate,
     TimeZone, Amount, FITID, CheckNumber, Memo,
     PayeeName, TransactionDateMemo, OperationSubtype)
VALUES

-- ===  13/02/2026  ===
(@StmtId,@catSal,'CREDIT','20260213000000[-03:EST]','2026-02-13','-03:EST',
  733.42,'N205C4:13/02/26:733.42:1300449','1300449',
  'Trans Sal p/c/c Bco:237 Age:00449 Cta:0032016-1',
  'Bco:237 Age:00449 Cta:0032016-1','13/02','Trans Sal p/c/c'),

(@StmtId,@catApl,'DEBIT','20260213000000[-03:EST]','2026-02-13','-03:EST',
  -732.42,'N205E1:13/02/26:-732.42:3125571','3125571',
  'Apl.invest Fac',NULL,'13/02','Apl.invest Fac'),

-- ===  19/02/2026  ===
(@StmtId,@catRes,'CREDIT','20260219000000[-03:EST]','2026-02-19','-03:EST',
  413.15,'N205FE:19/02/26:413.15:3125571','3125571',
  'Resgate Inv Fac',NULL,'19/02','Resgate Inv Fac'),

(@StmtId,@catYld,'CREDIT','20260219000000[-03:EST]','2026-02-19','-03:EST',
  0.01,'N2061A:19/02/26:0.01:3125571','3125571',
  'Rent.inv.facil',NULL,'19/02','Rent.inv.facil'),

(@StmtId,@catPS,'DEBIT','20260219000000[-03:EST]','2026-02-19','-03:EST',
  -250.00,'N20636:19/02/26:-250.0:1156144','1156144',
  'Transfe Pix Des: Bruno da Silva 19/02',
  'Bruno da Silva','19/02','Transfe Pix'),

(@StmtId,@catQD,'DEBIT','20260219000000[-03:EST]','2026-02-19','-03:EST',
  -5.00,'N20654:19/02/26:-5.0:1307020','1307020',
  'Pix Qrcode Din Des: Geovan Pereira de Alm 19/02',
  'Geovan Pereira de Alm','19/02','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260219000000[-03:EST]','2026-02-19','-03:EST',
  -158.16,'N20672:19/02/26:-158.16:1434319','1434319',
  'Pix Qrcode Din Des: Fam Centro Universita 19/02',
  'Fam Centro Universita','19/02','Pix Qrcode Din'),

-- ===  20/02/2026  ===
(@StmtId,@catRes,'CREDIT','20260220000000[-03:EST]','2026-02-20','-03:EST',
  139.12,'N20690:20/02/26:139.12:3125571','3125571',
  'Resgate Inv Fac',NULL,'20/02','Resgate Inv Fac'),

(@StmtId,@catQD,'DEBIT','20260220000000[-03:EST]','2026-02-20','-03:EST',
  -20.00,'N206AC:20/02/26:-20.0:0700522','0700522',
  'Pix Qrcode Din Des: af Line.com 20/02',
  'af Line.com','20/02','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260220000000[-03:EST]','2026-02-20','-03:EST',
  -23.00,'N206CA:20/02/26:-23.0:0718443','0718443',
  'Pix Qrcode Din Des: Sant Sucos e Lanches 20/02',
  'Sant Sucos e Lanches','20/02','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260220000000[-03:EST]','2026-02-20','-03:EST',
  -19.99,'N206E8:20/02/26:-19.99:0747278','0747278',
  'Pix Qrcode Din Des: af Line.com 20/02',
  'af Line.com','20/02','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260220000000[-03:EST]','2026-02-20','-03:EST',
  -76.13,'N20706:20/02/26:-76.13:0803008','0803008',
  'Pix Qrcode Din Des: Companhia Brasileira 20/02',
  'Companhia Brasileira','20/02','Pix Qrcode Din'),

-- ===  23/02/2026  ===
(@StmtId,@catRes,'CREDIT','20260223000000[-03:EST]','2026-02-23','-03:EST',
  50.00,'N20724:23/02/26:50.0:3125571','3125571',
  'Resgate Inv Fac',NULL,'23/02','Resgate Inv Fac'),

(@StmtId,@catQD,'DEBIT','20260223000000[-03:EST]','2026-02-23','-03:EST',
  -50.00,'N20740:23/02/26:-50.0:1847352','1847352',
  'Pix Qrcode Din Des: Speedy Auto Posto Ltd 22/02',
  'Speedy Auto Posto Ltd','22/02','Pix Qrcode Din'),

-- ===  24/02/2026  ===
(@StmtId,@catRes,'CREDIT','20260224000000[-03:EST]','2026-02-24','-03:EST',
  100.49,'N2075E:24/02/26:100.49:3125571','3125571',
  'Resgate Inv Fac',NULL,'24/02','Resgate Inv Fac'),

(@StmtId,@catYld,'CREDIT','20260224000000[-03:EST]','2026-02-24','-03:EST',
  0.01,'N2077A:24/02/26:0.01:3125571','3125571',
  'Rent.inv.facil',NULL,'24/02','Rent.inv.facil'),

(@StmtId,@catQD,'DEBIT','20260224000000[-03:EST]','2026-02-24','-03:EST',
  -20.00,'N20796:24/02/26:-20.0:0640557','0640557',
  'Pix Qrcode Din Des: af Line.com 24/02',
  'af Line.com','24/02','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260224000000[-03:EST]','2026-02-24','-03:EST',
  -23.00,'N207B4:24/02/26:-23.0:0659216','0659216',
  'Pix Qrcode Din Des: Sant Sucos e Lanches 24/02',
  'Sant Sucos e Lanches','24/02','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260224000000[-03:EST]','2026-02-24','-03:EST',
  -49.00,'N207D2:24/02/26:-49.0:1304103','1304103',
  'Pix Qrcode Din Des: Restaurante, Bar e ca 24/02',
  'Restaurante, Bar e ca','24/02','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260224000000[-03:EST]','2026-02-24','-03:EST',
  -8.50,'N207F0:24/02/26:-8.5:1308129','1308129',
  'Pix Qrcode Din Des: Horti Fruit Haddock 24/02',
  'Horti Fruit Haddock','24/02','Pix Qrcode Din'),

-- ===  25/02/2026  ===
(@StmtId,@catRes,'CREDIT','20260225000000[-03:EST]','2026-02-25','-03:EST',
  29.66,'N2080E:25/02/26:29.66:3125571','3125571',
  'Resgate Inv Fac',NULL,'25/02','Resgate Inv Fac'),

(@StmtId,@catPS,'DEBIT','20260225000000[-03:EST]','2026-02-25','-03:EST',
  -30.66,'N2082A:25/02/26:-30.66:0622124','0622124',
  'Transfe Pix Des: Rodrigo Luiz Madeira 25/02',
  'Rodrigo Luiz Madeira','25/02','Transfe Pix'),

-- ===  27/02/2026  ===
(@StmtId,@catSal,'CREDIT','20260227000000[-03:EST]','2026-02-27','-03:EST',
  6624.72,'N20848:27/02/26:6624.72:2700449','2700449',
  'Trans Sal p/c/c Bco:237 Age:00449 Cta:0032016-1',
  'Bco:237 Age:00449 Cta:0032016-1','27/02','Trans Sal p/c/c'),

(@StmtId,@catBP,'DEBIT','20260227000000[-03:EST]','2026-02-27','-03:EST',
  -1513.39,'N20865:27/02/26:-1513.39:0000001','0000001',
  'Pagto Cobranca Gci Caixa - Habitacao',
  'Gci Caixa - Habitacao','27/02','Pagto Cobranca'),

(@StmtId,@catApl,'DEBIT','20260227000000[-03:EST]','2026-02-27','-03:EST',
  -3317.34,'N20883:27/02/26:-3317.34:4608848','4608848',
  'Apl.invest Fac',NULL,'27/02','Apl.invest Fac'),

(@StmtId,@catPS,'DEBIT','20260227000000[-03:EST]','2026-02-27','-03:EST',
  -1650.00,'N208A0:27/02/26:-1650.0:1527204','1527204',
  'Transfe Pix Des: Lazaro Augusto Gusmao 27/02',
  'Lazaro Augusto Gusmao','27/02','Transfe Pix'),

(@StmtId,@catQS,'DEBIT','20260227000000[-03:EST]','2026-02-27','-03:EST',
  -142.99,'N208BE:27/02/26:-142.99:1421017','1421017',
  'Pix Qrcode Est Des: Pix Marketplace 27/02',
  'Pix Marketplace','27/02','Pix Qrcode Est'),

-- ===  02/03/2026  ===
(@StmtId,@catRes,'CREDIT','20260302000000[-03:EST]','2026-03-02','-03:EST',
  3317.34,'N208DC:02/03/26:3317.34:4608848','4608848',
  'Resgate Inv Fac',NULL,'02/03','Resgate Inv Fac'),

(@StmtId,@catYld,'CREDIT','20260302000000[-03:EST]','2026-03-02','-03:EST',
  0.01,'N208F8:02/03/26:0.01:4608848','4608848',
  'Rent.inv.facil',NULL,'02/03','Rent.inv.facil'),

(@StmtId,@catPS,'DEBIT','20260302000000[-03:EST]','2026-03-02','-03:EST',
  -2582.35,'N20914:02/03/26:-2582.35:2106407','2106407',
  'Transfe Pix Des: Rodrigo Luiz Madeira 01/03',
  'Rodrigo Luiz Madeira','01/03','Transfe Pix'),

(@StmtId,@catQD,'DEBIT','20260302000000[-03:EST]','2026-03-02','-03:EST',
  -736.00,'N20932:02/03/26:-736.0:1601480','1601480',
  'Pix Qrcode Din Des: Galpao Fortes Distrib 28/02',
  'Galpao Fortes Distrib','28/02','Pix Qrcode Din'),

-- ===  13/03/2026  ===
(@StmtId,@catSal,'CREDIT','20260313000000[-03:EST]','2026-03-13','-03:EST',
  5280.00,'N20950:13/03/26:5280.0:1300449','1300449',
  'Trans Sal p/c/c Bco:237 Age:00449 Cta:0032016-1',
  'Bco:237 Age:00449 Cta:0032016-1','13/03','Trans Sal p/c/c'),

(@StmtId,@catBP,'DEBIT','20260313000000[-03:EST]','2026-03-13','-03:EST',
  -3441.97,'N2096D:13/03/26:-3441.97:0000002','0000002',
  'Pagto Cobranca Grpqa',
  'Grpqa','13/03','Pagto Cobranca'),

(@StmtId,@catApl,'DEBIT','20260313000000[-03:EST]','2026-03-13','-03:EST',
  -1793.05,'N2098B:13/03/26:-1793.05:2832416','2832416',
  'Apl.invest Fac',NULL,'13/03','Apl.invest Fac'),

(@StmtId,@catQD,'DEBIT','20260313000000[-03:EST]','2026-03-13','-03:EST',
  -43.98,'N209A8:13/03/26:-43.98:2332122','2332122',
  'Pix Qrcode Din Des: Bar Vila Augusta 13/03',
  'Bar Vila Augusta','13/03','Pix Qrcode Din'),

-- ===  16/03/2026  ===
(@StmtId,@catRes,'CREDIT','20260316000000[-03:EST]','2026-03-16','-03:EST',
  1248.35,'N209C6:16/03/26:1248.35:2832416','2832416',
  'Resgate Inv Fac',NULL,'16/03','Resgate Inv Fac'),

(@StmtId,@catYld,'CREDIT','20260316000000[-03:EST]','2026-03-16','-03:EST',
  0.01,'N209E2:16/03/26:0.01:2832416','2832416',
  'Rent.inv.facil',NULL,'16/03','Rent.inv.facil'),

(@StmtId,@catBP,'DEBIT','20260316000000[-03:EST]','2026-03-16','-03:EST',
  -284.46,'N209FE:16/03/26:-284.46:0000003','0000003',
  'Pagto Cobranca Itau Unibanco S.a.',
  'Itau Unibanco S.a.','16/03','Pagto Cobranca'),

(@StmtId,@catPS,'DEBIT','20260316000000[-03:EST]','2026-03-16','-03:EST',
  -200.00,'N20A1C:16/03/26:-200.0:0749195','0749195',
  'Transfe Pix Des: Rodrigo Luiz Madeira 14/03',
  'Rodrigo Luiz Madeira','14/03','Transfe Pix'),

(@StmtId,@catPS,'DEBIT','20260316000000[-03:EST]','2026-03-16','-03:EST',
  -300.00,'N20A3A:16/03/26:-300.0:1333040','1333040',
  'Transfe Pix Des: Katia Cristina Piacen 16/03',
  'Katia Cristina Piacen','16/03','Transfe Pix'),

(@StmtId,@catPS,'DEBIT','20260316000000[-03:EST]','2026-03-16','-03:EST',
  -40.00,'N20A58:16/03/26:-40.0:1345472','1345472',
  'Transfe Pix Des: Emerson Anizio Das ne 14/03',
  'Emerson Anizio Das ne','14/03','Transfe Pix'),

(@StmtId,@catQS,'DEBIT','20260316000000[-03:EST]','2026-03-16','-03:EST',
  -150.00,'N20A76:16/03/26:-150.0:1127087','1127087',
  'Pix Qrcode Est Des: Rodrigo Luiz Madeira 15/03',
  'Rodrigo Luiz Madeira','15/03','Pix Qrcode Est'),

(@StmtId,@catQD,'DEBIT','20260316000000[-03:EST]','2026-03-16','-03:EST',
  -190.90,'N20A94:16/03/26:-190.9:0922035','0922035',
  'Pix Qrcode Din Des: Sem Parar Instituicao 14/03',
  'Sem Parar Instituicao','14/03','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260316000000[-03:EST]','2026-03-16','-03:EST',
  -22.00,'N20AB2:16/03/26:-22.0:1016384','1016384',
  'Pix Qrcode Din Des: Rede lp Park Estacion 14/03',
  'Rede lp Park Estacion','14/03','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260316000000[-03:EST]','2026-03-16','-03:EST',
  -61.00,'N20AD0:16/03/26:-61.0:1035368','1035368',
  'Pix Qrcode Din Des: Deocleciano Macedo de 14/03',
  'Deocleciano Macedo de','14/03','Pix Qrcode Din'),

-- ===  17/03/2026  ===
(@StmtId,@catRes,'CREDIT','20260317000000[-03:EST]','2026-03-17','-03:EST',
  94.43,'N20AEE:17/03/26:94.43:2832416','2832416',
  'Resgate Inv Fac',NULL,'17/03','Resgate Inv Fac'),

(@StmtId,@catQD,'DEBIT','20260317000000[-03:EST]','2026-03-17','-03:EST',
  -94.43,'N20B0A:17/03/26:-94.43:0754067','0754067',
  'Pix Qrcode Din Des: Companhia Brasileira 17/03',
  'Companhia Brasileira','17/03','Pix Qrcode Din'),

-- ===  18/03/2026  ===
(@StmtId,@catRes,'CREDIT','20260318000000[-03:EST]','2026-03-18','-03:EST',
  67.99,'N20B28:18/03/26:67.99:2832416','2832416',
  'Resgate Inv Fac',NULL,'18/03','Resgate Inv Fac'),

(@StmtId,@catQD,'DEBIT','20260318000000[-03:EST]','2026-03-18','-03:EST',
  -32.99,'N20B44:18/03/26:-32.99:0743328','0743328',
  'Pix Qrcode Din Des: Smoov 010 Ibirapuera 18/03',
  'Smoov 010 Ibirapuera','18/03','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260318000000[-03:EST]','2026-03-18','-03:EST',
  -35.00,'N20B62:18/03/26:-35.0:1755084','1755084',
  'Pix Qrcode Din Des: Ana Maria da Penha si 18/03',
  'Ana Maria da Penha si','18/03','Pix Qrcode Din'),

-- ===  19/03/2026  ===
(@StmtId,@catRes,'CREDIT','20260319000000[-03:EST]','2026-03-19','-03:EST',
  85.58,'N20B80:19/03/26:85.58:2832416','2832416',
  'Resgate Inv Fac',NULL,'19/03','Resgate Inv Fac'),

(@StmtId,@catQD,'DEBIT','20260319000000[-03:EST]','2026-03-19','-03:EST',
  -85.58,'N20B9C:19/03/26:-85.58:1332318','1332318',
  'Pix Qrcode Din Des: Jhow Jhow 19/03',
  'Jhow Jhow','19/03','Pix Qrcode Din'),

-- ===  20/03/2026  ===
(@StmtId,@catRes,'CREDIT','20260320000000[-03:EST]','2026-03-20','-03:EST',
  39.77,'N20BBA:20/03/26:39.77:2832416','2832416',
  'Resgate Inv Fac',NULL,'20/03','Resgate Inv Fac'),

(@StmtId,@catPI,'CREDIT','20260320000000[-03:EST]','2026-03-20','-03:EST',
  75.57,'N20BD6:20/03/26:75.57:2033148','2033148',
  'Transfe Pix Rem: Rodrigo Furlaneti 20/03',
  'Rodrigo Furlaneti','20/03','Transfe Pix Rem'),

(@StmtId,@catQD,'DEBIT','20260320000000[-03:EST]','2026-03-20','-03:EST',
  -28.00,'N20BF3:20/03/26:-28.0:0752280','0752280',
  'Pix Qrcode Din Des: Ana Maria da Penha si 20/03',
  'Ana Maria da Penha si','20/03','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260320000000[-03:EST]','2026-03-20','-03:EST',
  -61.85,'N20C11:20/03/26:-61.85:1312508','1312508',
  'Pix Qrcode Din Des: Galeria Paulista 20/03',
  'Galeria Paulista','20/03','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260320000000[-03:EST]','2026-03-20','-03:EST',
  -25.49,'N20C2F:20/03/26:-25.49:1339421','1339421',
  'Pix Qrcode Din Des: Rocha Mini Mercado 20/03',
  'Rocha Mini Mercado','20/03','Pix Qrcode Din'),

-- ===  23/03/2026  ===
(@StmtId,@catRes,'CREDIT','20260323000000[-03:EST]','2026-03-23','-03:EST',
  224.02,'N20C4D:23/03/26:224.02:2832416','2832416',
  'Resgate Inv Fac',NULL,'23/03','Resgate Inv Fac'),

(@StmtId,@catYld,'CREDIT','20260323000000[-03:EST]','2026-03-23','-03:EST',
  0.01,'N20C69:23/03/26:0.01:2832416','2832416',
  'Rent.inv.facil',NULL,'23/03','Rent.inv.facil'),

(@StmtId,@catQS,'DEBIT','20260323000000[-03:EST]','2026-03-23','-03:EST',
  -39.60,'N20C85:23/03/26:-39.6:0750198','0750198',
  'Pix Qrcode Est Des: Lanchonete e Restaura 22/03',
  'Lanchonete e Restaura','22/03','Pix Qrcode Est'),

(@StmtId,@catQS,'DEBIT','20260323000000[-03:EST]','2026-03-23','-03:EST',
  -14.56,'N20CA3:23/03/26:-14.56:1547178','1547178',
  'Pix Qrcode Est Des: nu Pagamentos S/a 22/03',
  'nu Pagamentos S/a','22/03','Pix Qrcode Est'),

(@StmtId,@catQD,'DEBIT','20260323000000[-03:EST]','2026-03-23','-03:EST',
  -23.00,'N20CC1:23/03/26:-23.0:0826283','0826283',
  'Pix Qrcode Din Des: Bar e Lanches 23/03',
  'Bar e Lanches','23/03','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260323000000[-03:EST]','2026-03-23','-03:EST',
  -19.00,'N20CDF:23/03/26:-19.0:1120104','1120104',
  'Pix Qrcode Din Des: Ana Maria da Penha si 21/03',
  'Ana Maria da Penha si','21/03','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260323000000[-03:EST]','2026-03-23','-03:EST',
  -4.49,'N20CFD:23/03/26:-4.49:1125505','1125505',
  'Pix Qrcode Din Des: Mercado Saga Ltda Epp 21/03',
  'Mercado Saga Ltda Epp','21/03','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260323000000[-03:EST]','2026-03-23','-03:EST',
  -12.99,'N20D1B:23/03/26:-12.99:1130536','1130536',
  'Pix Qrcode Din Des: Horti Fruit Haddock 21/03',
  'Horti Fruit Haddock','21/03','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260323000000[-03:EST]','2026-03-23','-03:EST',
  -5.00,'N20D39:23/03/26:-5.0:1254146','1254146',
  'Pix Qrcode Din Des: Auto Posto Grana Ltda 22/03',
  'Auto Posto Grana Ltda','22/03','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260323000000[-03:EST]','2026-03-23','-03:EST',
  -29.90,'N20D57:23/03/26:-29.9:1319118','1319118',
  'Pix Qrcode Din Des: Imifarma 22/03',
  'Imifarma','22/03','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260323000000[-03:EST]','2026-03-23','-03:EST',
  -5.49,'N20D75:23/03/26:-5.49:1342111','1342111',
  'Pix Qrcode Din Des: Companhia Brasileira 22/03',
  'Companhia Brasileira','22/03','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260323000000[-03:EST]','2026-03-23','-03:EST',
  -59.00,'N20D93:23/03/26:-59.0:1608316','1608316',
  'Pix Qrcode Din Des: Boteco-seu Joao ii 22/03',
  'Boteco-seu Joao ii','22/03','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260323000000[-03:EST]','2026-03-23','-03:EST',
  -5.00,'N20DB1:23/03/26:-5.0:2123324','2123324',
  'Pix Qrcode Din Des: Luxo Companhia Das fr 21/03',
  'Luxo Companhia Das fr','21/03','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260323000000[-03:EST]','2026-03-23','-03:EST',
  -6.00,'N20DCF:23/03/26:-6.0:2157223','2157223',
  'Pix Qrcode Din Des: Espaco Moraes 21/03',
  'Espaco Moraes','21/03','Pix Qrcode Din'),

-- ===  25/03/2026  ===
(@StmtId,@catPF,'CREDIT','20260325000000[-03:EST]','2026-03-25','-03:EST',
  142.99,'N20DED:25/03/26:142.99:0951163','0951163',
  'Devolucao Pix Rem: Pix Marketplace 25/03',
  'Pix Marketplace','25/03','Devolucao Pix'),

(@StmtId,@catApl,'DEBIT','20260325000000[-03:EST]','2026-03-25','-03:EST',
  -100.98,'N20E0A:25/03/26:-100.98:2642804','2642804',
  'Apl.invest Fac',NULL,'25/03','Apl.invest Fac'),

(@StmtId,@catQD,'DEBIT','20260325000000[-03:EST]','2026-03-25','-03:EST',
  -42.01,'N20E27:25/03/26:-42.01:1343269','1343269',
  'Pix Qrcode Din Des: Dany Comercial 25/03',
  'Dany Comercial','25/03','Pix Qrcode Din'),

-- ===  26/03/2026  ===
(@StmtId,@catRes,'CREDIT','20260326000000[-03:EST]','2026-03-26','-03:EST',
  21.16,'N20E45:26/03/26:21.16:2642804','2642804',
  'Resgate Inv Fac',NULL,'26/03','Resgate Inv Fac'),

(@StmtId,@catRes,'CREDIT','20260326000000[-03:EST]','2026-03-26','-03:EST',
  32.91,'N20E61:26/03/26:32.91:2832416','2832416',
  'Resgate Inv Fac',NULL,'26/03','Resgate Inv Fac'),

(@StmtId,@catQD,'DEBIT','20260326000000[-03:EST]','2026-03-26','-03:EST',
  -44.00,'N20E7D:26/03/26:-44.0:1749429','1749429',
  'Pix Qrcode Din Des: Geovan Pereira de Alm 26/03',
  'Geovan Pereira de Alm','26/03','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260326000000[-03:EST]','2026-03-26','-03:EST',
  -10.07,'N20E9B:26/03/26:-10.07:1756097','1756097',
  'Pix Qrcode Din Des: Mercado Yab s 26/03',
  'Mercado Yab s','26/03','Pix Qrcode Din'),

-- ===  27/03/2026  ===
(@StmtId,@catRes,'CREDIT','20260327000000[-03:EST]','2026-03-27','-03:EST',
  79.82,'N20EB9:27/03/26:79.82:2642804','2642804',
  'Resgate Inv Fac',NULL,'27/03','Resgate Inv Fac'),

(@StmtId,@catQD,'DEBIT','20260327000000[-03:EST]','2026-03-27','-03:EST',
  -31.90,'N20ED5:27/03/26:-31.9:0834266','0834266',
  'Pix Qrcode Din Des: Emporio Bar e Lanchon 27/03',
  'Emporio Bar e Lanchon','27/03','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260327000000[-03:EST]','2026-03-27','-03:EST',
  -42.00,'N20EF3:27/03/26:-42.0:1327199','1327199',
  'Pix Qrcode Din Des: Restaurante, Bar e ca 27/03',
  'Restaurante, Bar e ca','27/03','Pix Qrcode Din'),

-- ===  31/03/2026  ===
(@StmtId,@catSal,'CREDIT','20260331000000[-03:EST]','2026-03-31','-03:EST',
  4464.99,'N20F11:31/03/26:4464.99:3100449','3100449',
  'Trans Sal p/c/c Bco:237 Age:00449 Cta:0032016-1',
  'Bco:237 Age:00449 Cta:0032016-1','31/03','Trans Sal p/c/c'),

(@StmtId,@catApl,'DEBIT','20260331000000[-03:EST]','2026-03-31','-03:EST',
  -1283.91,'N20F2E:31/03/26:-1283.91:8554299','8554299',
  'Apl.invest Fac',NULL,'31/03','Apl.invest Fac'),

(@StmtId,@catPS,'DEBIT','20260331000000[-03:EST]','2026-03-31','-03:EST',
  -250.00,'N20F4B:31/03/26:-250.0:1207334','1207334',
  'Transfe Pix Des: Nerinalva Peixoto de 31/03',
  'Nerinalva Peixoto de','31/03','Transfe Pix'),

(@StmtId,@catPS,'DEBIT','20260331000000[-03:EST]','2026-03-31','-03:EST',
  -1000.00,'N20F69:31/03/26:-1000.0:1245151','1245151',
  'Transfe Pix Des: Katia Cristina Piacen 31/03',
  'Katia Cristina Piacen','31/03','Transfe Pix'),

(@StmtId,@catPS,'DEBIT','20260331000000[-03:EST]','2026-03-31','-03:EST',
  -1587.00,'N20F87:31/03/26:-1587.0:1250036','1250036',
  'Transfe Pix Des: Lazaro Augusto Gusmao 31/03',
  'Lazaro Augusto Gusmao','31/03','Transfe Pix'),

(@StmtId,@catPS,'DEBIT','20260331000000[-03:EST]','2026-03-31','-03:EST',
  -350.00,'N20FA5:31/03/26:-350.0:1303020','1303020',
  'Transfe Pix Des: Bruno da Silva 31/03',
  'Bruno da Silva','31/03','Transfe Pix'),

-- ===  06/04/2026  ===
(@StmtId,@catRes,'CREDIT','20260406000000[-03:EST]','2026-04-06','-03:EST',
  361.91,'N20FC3:06/04/26:361.91:8554299','8554299',
  'Resgate Inv Fac',NULL,'06/04','Resgate Inv Fac'),

(@StmtId,@catPS,'DEBIT','20260406000000[-03:EST]','2026-04-06','-03:EST',
  -300.00,'N20FDF:06/04/26:-300.0:1003051','1003051',
  'Transfe Pix Des: Rodrigo Luiz Madeira 04/04',
  'Rodrigo Luiz Madeira','04/04','Transfe Pix'),

(@StmtId,@catQD,'DEBIT','20260406000000[-03:EST]','2026-04-06','-03:EST',
  -41.11,'N20FFD:06/04/26:-41.11:0753263','0753263',
  'Pix Qrcode Din Des: Lanchonete e Panifica 05/04',
  'Lanchonete e Panifica','05/04','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260406000000[-03:EST]','2026-04-06','-03:EST',
  -20.80,'N2101B:06/04/26:-20.8:0821378','0821378',
  'Pix Qrcode Din Des: Rodosnack Uss Guarare 03/04',
  'Rodosnack Uss Guarare','03/04','Pix Qrcode Din'),

-- ===  07/04/2026  ===
(@StmtId,@catRes,'CREDIT','20260407000000[-03:EST]','2026-04-07','-03:EST',
  164.31,'N21039:07/04/26:164.31:8554299','8554299',
  'Resgate Inv Fac',NULL,'07/04','Resgate Inv Fac'),

(@StmtId,@catQD,'DEBIT','20260407000000[-03:EST]','2026-04-07','-03:EST',
  -164.31,'N21055:07/04/26:-164.31:0832511','0832511',
  'Pix Qrcode Din Des: Telefonica Bras 07/04',
  'Telefonica Bras','07/04','Pix Qrcode Din'),

-- ===  08/04/2026  ===
(@StmtId,@catRes,'CREDIT','20260408000000[-03:EST]','2026-04-08','-03:EST',
  100.00,'N21073:08/04/26:100.0:8554299','8554299',
  'Resgate Inv Fac',NULL,'08/04','Resgate Inv Fac'),

(@StmtId,@catQD,'DEBIT','20260408000000[-03:EST]','2026-04-08','-03:EST',
  -100.00,'N2108F:08/04/26:-100.0:1850497','1850497',
  'Pix Qrcode Din Des: Auto Posto Bandeira p 08/04',
  'Auto Posto Bandeira p','08/04','Pix Qrcode Din'),

-- ===  09/04/2026  ===
(@StmtId,@catRes,'CREDIT','20260409000000[-03:EST]','2026-04-09','-03:EST',
  144.91,'N210AD:09/04/26:144.91:8554299','8554299',
  'Resgate Inv Fac',NULL,'09/04','Resgate Inv Fac'),

(@StmtId,@catCC,'DEBIT','20260409000000[-03:EST]','2026-04-09','-03:EST',
  -15.00,'N210C9:09/04/26:-15.0:0000014','0000014',
  'Compra Cart Elo ls sp Frei Caneca sa',
  'ls sp Frei Caneca sa','09/04','Compra Cart Elo'),

(@StmtId,@catQS,'DEBIT','20260409000000[-03:EST]','2026-04-09','-03:EST',
  -129.91,'N210E7:09/04/26:-129.91:0906179','0906179',
  'Pix Qrcode Est Des: Claro 09/04',
  'Claro','09/04','Pix Qrcode Est'),

-- ===  10/04/2026  ===
(@StmtId,@catRes,'CREDIT','20260410000000[-03:EST]','2026-04-10','-03:EST',
  12.99,'N21105:10/04/26:12.99:8554299','8554299',
  'Resgate Inv Fac',NULL,'10/04','Resgate Inv Fac'),

(@StmtId,@catCC,'DEBIT','20260410000000[-03:EST]','2026-04-10','-03:EST',
  -12.99,'N21121:10/04/26:-12.99:0273633','0273633',
  'Compra Cart Elo Horti Frut Haddock e',
  'Horti Frut Haddock e','10/04','Compra Cart Elo'),

-- ===  13/04/2026  ===
(@StmtId,@catRes,'CREDIT','20260413000000[-03:EST]','2026-04-13','-03:EST',
  458.16,'N2113F:13/04/26:458.16:8554299','8554299',
  'Resgate Inv Fac',NULL,'13/04','Resgate Inv Fac'),

(@StmtId,@catYld,'CREDIT','20260413000000[-03:EST]','2026-04-13','-03:EST',
  0.02,'N2115B:13/04/26:0.02:8554299','8554299',
  'Rent.inv.facil',NULL,'13/04','Rent.inv.facil'),

(@StmtId,@catCC,'DEBIT','20260413000000[-03:EST]','2026-04-13','-03:EST',
  -10.00,'N21177:13/04/26:-10.0:0021305','0021305',
  'Compra Cart Elo mp *mariabar',
  'mp *mariabar','13/04','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260413000000[-03:EST]','2026-04-13','-03:EST',
  -23.00,'N21195:13/04/26:-23.0:0025291','0025291',
  'Compra Cart Elo mp *padariaguaru',
  'mp *padariaguaru','13/04','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260413000000[-03:EST]','2026-04-13','-03:EST',
  -8.00,'N211B3:13/04/26:-8.0:0025878','0025878',
  'Compra Cart Elo mp *mariabar',
  'mp *mariabar','13/04','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260413000000[-03:EST]','2026-04-13','-03:EST',
  -20.00,'N211D1:13/04/26:-20.0:0090235','0090235',
  'Compra Cart Elo Mp*mariabar',
  'Mp*mariabar','13/04','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260413000000[-03:EST]','2026-04-13','-03:EST',
  -67.59,'N211EF:13/04/26:-67.59:0130040','0130040',
  'Compra Cart Elo Drogaria Sao Paulo 0',
  'Drogaria Sao Paulo 0','13/04','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260413000000[-03:EST]','2026-04-13','-03:EST',
  -287.61,'N2120D:13/04/26:-287.61:0285167','0285167',
  'Compra Cart Elo Zig*quinta do Embaix',
  'Zig*quinta do Embaix','13/04','Compra Cart Elo'),

(@StmtId,@catQD,'DEBIT','20260413000000[-03:EST]','2026-04-13','-03:EST',
  -41.98,'N2122B:13/04/26:-41.98:2133176','2133176',
  'Pix Qrcode Din Des: Ifood.com Agencia de 12/04',
  'Ifood.com Agencia de','12/04','Pix Qrcode Din'),

-- ===  15/04/2026  ===
(@StmtId,@catSal,'CREDIT','20260415000000[-03:EST]','2026-04-15','-03:EST',
  5280.00,'N21249:15/04/26:5280.0:1500449','1500449',
  'Trans Sal p/c/c Bco:237 Age:00449 Cta:0032016-1',
  'Bco:237 Age:00449 Cta:0032016-1','15/04','Trans Sal p/c/c'),

(@StmtId,@catBP,'DEBIT','20260415000000[-03:EST]','2026-04-15','-03:EST',
  -1633.22,'N21266:15/04/26:-1633.22:0000004','0000004',
  'Pagto Cobranca Aluguel Apto sp',
  'Aluguel Apto sp','15/04','Pagto Cobranca'),

(@StmtId,@catApl,'DEBIT','20260415000000[-03:EST]','2026-04-15','-03:EST',
  -1439.26,'N21284:15/04/26:-1439.26:6650372','6650372',
  'Apl.invest Fac',NULL,'15/04','Apl.invest Fac'),

(@StmtId,@catPS,'DEBIT','20260415000000[-03:EST]','2026-04-15','-03:EST',
  -1100.00,'N212A1:15/04/26:-1100.0:1347291','1347291',
  'Transfe Pix Des: Katia Cristina Piacen 15/04',
  'Katia Cristina Piacen','15/04','Transfe Pix'),

(@StmtId,@catQD,'DEBIT','20260415000000[-03:EST]','2026-04-15','-03:EST',
  -1107.52,'N212BF:15/04/26:-1107.52:1042304','1042304',
  'Pix Qrcode Din Des: Banco Votorantim S.a. 15/04',
  'Banco Votorantim S.a.','15/04','Pix Qrcode Din'),

-- ===  16/04/2026  ===
(@StmtId,@catRes,'CREDIT','20260416000000[-03:EST]','2026-04-16','-03:EST',
  35.47,'N212DD:16/04/26:35.47:6650372','6650372',
  'Resgate Inv Fac',NULL,'16/04','Resgate Inv Fac'),

(@StmtId,@catRes,'CREDIT','20260416000000[-03:EST]','2026-04-16','-03:EST',
  41.63,'N212F9:16/04/26:41.63:8554299','8554299',
  'Resgate Inv Fac',NULL,'16/04','Resgate Inv Fac'),

(@StmtId,@catCC,'DEBIT','20260416000000[-03:EST]','2026-04-16','-03:EST',
  -32.06,'N21315:16/04/26:-32.06:0160116','0160116',
  'Compra Cart Elo Drogaria Sao Paulo 0',
  'Drogaria Sao Paulo 0','16/04','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260416000000[-03:EST]','2026-04-16','-03:EST',
  -15.56,'N21333:16/04/26:-15.56:0500920','0500920',
  'Compra Cart Elo Dany Com Alimentos l',
  'Dany Com Alimentos l','16/04','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260416000000[-03:EST]','2026-04-16','-03:EST',
  -29.48,'N21351:16/04/26:-29.48:0579900','0579900',
  'Compra Cart Elo Frutaria Bela Vista',
  'Frutaria Bela Vista','16/04','Compra Cart Elo'),

-- ===  17/04/2026  ===
(@StmtId,@catRes,'CREDIT','20260417000000[-03:EST]','2026-04-17','-03:EST',
  89.30,'N2136F:17/04/26:89.3:6650372','6650372',
  'Resgate Inv Fac',NULL,'17/04','Resgate Inv Fac'),

(@StmtId,@catCC,'DEBIT','20260417000000[-03:EST]','2026-04-17','-03:EST',
  -20.00,'N2138B:17/04/26:-20.0:0193288','0193288',
  'Compra Cart Elo Ursulamatie',
  'Ursulamatie','17/04','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260417000000[-03:EST]','2026-04-17','-03:EST',
  -44.00,'N213A9:17/04/26:-44.0:0510649','0510649',
  'Compra Cart Elo Fox Beer',
  'Fox Beer','17/04','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260417000000[-03:EST]','2026-04-17','-03:EST',
  -25.30,'N213C7:17/04/26:-25.3:0745722','0745722',
  'Compra Cart Elo Elevenbarelanches',
  'Elevenbarelanches','17/04','Compra Cart Elo'),

-- ===  20/04/2026  ===
(@StmtId,@catRes,'CREDIT','20260420000000[-03:EST]','2026-04-20','-03:EST',
  325.44,'N213E5:20/04/26:325.44:6650372','6650372',
  'Resgate Inv Fac',NULL,'20/04','Resgate Inv Fac'),

(@StmtId,@catCC,'DEBIT','20260420000000[-03:EST]','2026-04-20','-03:EST',
  -34.68,'N21401:20/04/26:-34.68:0023665','0023665',
  'Compra Cart Elo Lanchonete e Panific',
  'Lanchonete e Panific','20/04','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260420000000[-03:EST]','2026-04-20','-03:EST',
  -19.46,'N2141F:20/04/26:-19.46:0071668','0071668',
  'Compra Cart Elo Farmaconde',
  'Farmaconde','20/04','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260420000000[-03:EST]','2026-04-20','-03:EST',
  -100.00,'N2143D:20/04/26:-100.0:0095313','0095313',
  'Compra Cart Elo Autoposto',
  'Autoposto','20/04','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260420000000[-03:EST]','2026-04-20','-03:EST',
  -27.50,'N2145B:20/04/26:-27.5:0221111','0221111',
  'Compra Cart Elo Panificadora e Confe',
  'Panificadora e Confe','20/04','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260420000000[-03:EST]','2026-04-20','-03:EST',
  -143.80,'N21479:20/04/26:-143.8:0358705','0358705',
  'Compra Cart Elo Portal do Jaguari',
  'Portal do Jaguari','20/04','Compra Cart Elo'),

-- ===  22/04/2026  ===
(@StmtId,@catRes,'CREDIT','20260422000000[-03:EST]','2026-04-22','-03:EST',
  703.63,'N21497:22/04/26:703.63:6650372','6650372',
  'Resgate Inv Fac',NULL,'22/04','Resgate Inv Fac'),

(@StmtId,@catYld,'CREDIT','20260422000000[-03:EST]','2026-04-22','-03:EST',
  0.01,'N214B3:22/04/26:0.01:6650372','6650372',
  'Rent.inv.facil',NULL,'22/04','Rent.inv.facil'),

(@StmtId,@catCC,'DEBIT','20260422000000[-03:EST]','2026-04-22','-03:EST',
  -18.00,'N214CF:22/04/26:-18.0:0024680','0024680',
  'Compra Cart Elo Skina do Bexiga',
  'Skina do Bexiga','22/04','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260422000000[-03:EST]','2026-04-22','-03:EST',
  -49.00,'N214ED:22/04/26:-49.0:0190659','0190659',
  'Compra Cart Elo Cachacaria Salinas',
  'Cachacaria Salinas','22/04','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260422000000[-03:EST]','2026-04-22','-03:EST',
  -80.59,'N2150B:22/04/26:-80.59:0267123','0267123',
  'Compra Cart Elo Extra Hiper-1302',
  'Extra Hiper-1302','22/04','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260422000000[-03:EST]','2026-04-22','-03:EST',
  -31.90,'N21529:22/04/26:-31.9:0445795','0445795',
  'Compra Cart Elo Faccini Point',
  'Faccini Point','22/04','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260422000000[-03:EST]','2026-04-22','-03:EST',
  -20.00,'N21547:22/04/26:-20.0:0452212','0452212',
  'Compra Cart Elo Lavanderia',
  'Lavanderia','22/04','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260422000000[-03:EST]','2026-04-22','-03:EST',
  -110.20,'N21565:22/04/26:-110.2:0541409','0541409',
  'Compra Cart Elo Praia33bare',
  'Praia33bare','22/04','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260422000000[-03:EST]','2026-04-22','-03:EST',
  -27.00,'N21583:22/04/26:-27.0:0589028','0589028',
  'Compra Cart Elo Sant Suco',
  'Sant Suco','22/04','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260422000000[-03:EST]','2026-04-22','-03:EST',
  -8.98,'N215A1:22/04/26:-8.98:0710373','0710373',
  'Compra Cart Elo Helio Yabuta',
  'Helio Yabuta','22/04','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260422000000[-03:EST]','2026-04-22','-03:EST',
  -19.99,'N215BF:22/04/26:-19.99:0915863','0915863',
  'Compra Cart Elo Lavanderia',
  'Lavanderia','22/04','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260422000000[-03:EST]','2026-04-22','-03:EST',
  -40.00,'N215DD:22/04/26:-40.0:0991366','0991366',
  'Compra Cart Elo Praia33bare',
  'Praia33bare','22/04','Compra Cart Elo'),

(@StmtId,@catPS,'DEBIT','20260422000000[-03:EST]','2026-04-22','-03:EST',
  -250.00,'N215FB:22/04/26:-250.0:1258093','1258093',
  'Transfe Pix Des: Bruno da Silva 22/04',
  'Bruno da Silva','22/04','Transfe Pix'),

(@StmtId,@catQS,'DEBIT','20260422000000[-03:EST]','2026-04-22','-03:EST',
  -20.00,'N21619:22/04/26:-20.0:1926013','1926013',
  'Pix Qrcode Est Des: Joao Vitor Caldeira 21/04',
  'Joao Vitor Caldeira','21/04','Pix Qrcode Est'),

(@StmtId,@catQD,'DEBIT','20260422000000[-03:EST]','2026-04-22','-03:EST',
  -27.98,'N21637:22/04/26:-27.98:1830393','1830393',
  'Pix Qrcode Din Des: Horti Fruit Haddock 22/04',
  'Horti Fruit Haddock','22/04','Pix Qrcode Din'),

-- ===  23/04/2026  ===
(@StmtId,@catRes,'CREDIT','20260423000000[-03:EST]','2026-04-23','-03:EST',
  131.47,'N21655:23/04/26:131.47:6650372','6650372',
  'Resgate Inv Fac',NULL,'23/04','Resgate Inv Fac'),

(@StmtId,@catCC,'DEBIT','20260423000000[-03:EST]','2026-04-23','-03:EST',
  -16.99,'N21671:23/04/26:-16.99:0350466','0350466',
  'Compra Cart Elo Docemar',
  'Docemar','23/04','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260423000000[-03:EST]','2026-04-23','-03:EST',
  -40.00,'N2168F:23/04/26:-40.0:0783864','0783864',
  'Compra Cart Elo Ana Maria da Penha s',
  'Ana Maria da Penha s','23/04','Compra Cart Elo'),

(@StmtId,@catQS,'DEBIT','20260423000000[-03:EST]','2026-04-23','-03:EST',
  -10.00,'N216AD:23/04/26:-10.0:1640222','1640222',
  'Pix Qrcode Est Des: Mercado Bitcoin Servi 23/04',
  'Mercado Bitcoin Servi','23/04','Pix Qrcode Est'),

(@StmtId,@catQD,'DEBIT','20260423000000[-03:EST]','2026-04-23','-03:EST',
  -10.00,'N216CB:23/04/26:-10.0:0754027','0754027',
  'Pix Qrcode Din Des: Ana Maria da Penha si 23/04',
  'Ana Maria da Penha si','23/04','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260423000000[-03:EST]','2026-04-23','-03:EST',
  -40.00,'N216E9:23/04/26:-40.0:0818577','0818577',
  'Pix Qrcode Din Des: Nic. br 23/04',
  'Nic. br','23/04','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260423000000[-03:EST]','2026-04-23','-03:EST',
  -14.48,'N21707:23/04/26:-14.48:2033460','2033460',
  'Pix Qrcode Din Des: Dany Comercial 23/04',
  'Dany Comercial','23/04','Pix Qrcode Din'),

-- ===  24/04/2026  ===
(@StmtId,@catRes,'CREDIT','20260424000000[-03:EST]','2026-04-24','-03:EST',
  35.30,'N21725:24/04/26:35.3:6650372','6650372',
  'Resgate Inv Fac',NULL,'24/04','Resgate Inv Fac'),

(@StmtId,@catCC,'DEBIT','20260424000000[-03:EST]','2026-04-24','-03:EST',
  -26.40,'N21741:24/04/26:-26.4:0728965','0728965',
  'Compra Cart Elo Flor da Joana',
  'Flor da Joana','24/04','Compra Cart Elo'),

(@StmtId,@catQD,'DEBIT','20260424000000[-03:EST]','2026-04-24','-03:EST',
  -8.90,'N2175F:24/04/26:-8.9:1921047','1921047',
  'Pix Qrcode Din Des: Ninai Salgados 24/04',
  'Ninai Salgados','24/04','Pix Qrcode Din'),

-- ===  27/04/2026  ===
(@StmtId,@catRes,'CREDIT','20260427000000[-03:EST]','2026-04-27','-03:EST',
  81.51,'N2177D:27/04/26:81.51:6650372','6650372',
  'Resgate Inv Fac',NULL,'27/04','Resgate Inv Fac'),

(@StmtId,@catCC,'DEBIT','20260427000000[-03:EST]','2026-04-27','-03:EST',
  -14.98,'N21799:27/04/26:-14.98:0711187','0711187',
  'Compra Cart Elo Helio Yabuta',
  'Helio Yabuta','27/04','Compra Cart Elo'),

(@StmtId,@catQD,'DEBIT','20260427000000[-03:EST]','2026-04-27','-03:EST',
  -17.00,'N217B7:27/04/26:-17.0:0835132','0835132',
  'Pix Qrcode Din Des: Rcm Confeitaria Ltda 25/04',
  'Rcm Confeitaria Ltda','25/04','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260427000000[-03:EST]','2026-04-27','-03:EST',
  -13.00,'N217D5:27/04/26:-13.0:0907576','0907576',
  'Pix Qrcode Din Des: Bar e Lanches 26/04',
  'Bar e Lanches','26/04','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260427000000[-03:EST]','2026-04-27','-03:EST',
  -4.50,'N217F3:27/04/26:-4.5:0934326','0934326',
  'Pix Qrcode Din Des: Aparecido Flauzino da 26/04',
  'Aparecido Flauzino da','26/04','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260427000000[-03:EST]','2026-04-27','-03:EST',
  -7.90,'N21811:27/04/26:-7.9:1027502','1027502',
  'Pix Qrcode Din Des: Mercado Adriata 26/04',
  'Mercado Adriata','26/04','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260427000000[-03:EST]','2026-04-27','-03:EST',
  -10.00,'N2182F:27/04/26:-10.0:1218072','1218072',
  'Pix Qrcode Din Des: Pagar me Pagamentos 25/04',
  'Pagar me Pagamentos','25/04','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260427000000[-03:EST]','2026-04-27','-03:EST',
  -9.00,'N2184D:27/04/26:-9.0:1222544','1222544',
  'Pix Qrcode Din Des: 61 012 794 Jorge Fern 25/04',
  '61 012 794 Jorge Fern','25/04','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260427000000[-03:EST]','2026-04-27','-03:EST',
  -5.13,'N2186B:27/04/26:-5.13:1841323','1841323',
  'Pix Qrcode Din Des: Dany Comercial 25/04',
  'Dany Comercial','25/04','Pix Qrcode Din'),

-- ===  28/04/2026  ===
(@StmtId,@catRes,'CREDIT','20260428000000[-03:EST]','2026-04-28','-03:EST',
  37.14,'N21889:28/04/26:37.14:6650372','6650372',
  'Resgate Inv Fac',NULL,'28/04','Resgate Inv Fac'),

(@StmtId,@catCC,'DEBIT','20260428000000[-03:EST]','2026-04-28','-03:EST',
  -32.40,'N218A5:28/04/26:-32.4:0154017','0154017',
  'Compra Cart Elo Flor da Joana',
  'Flor da Joana','28/04','Compra Cart Elo'),

-- ===  30/04/2026  ===
(@StmtId,@catSal,'CREDIT','20260430000000[-03:EST]','2026-04-30','-03:EST',
  4345.09,'N218C3:30/04/26:4345.09:3000449','3000449',
  'Trans Sal p/c/c Bco:237 Age:00449 Cta:0032016-1',
  'Bco:237 Age:00449 Cta:0032016-1','30/04','Trans Sal p/c/c'),

(@StmtId,@catCC,'DEBIT','20260430000000[-03:EST]','2026-04-30','-03:EST',
  -7.99,'N218E0:30/04/26:-7.99:0005616','0005616',
  'Compra Cart Elo Portal Leste',
  'Portal Leste','30/04','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260430000000[-03:EST]','2026-04-30','-03:EST',
  -200.00,'N218FE:30/04/26:-200.0:0016990','0016990',
  'Compra Cart Elo Portal Leste',
  'Portal Leste','30/04','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260430000000[-03:EST]','2026-04-30','-03:EST',
  -9.00,'N2191C:30/04/26:-9.0:0865302','0865302',
  'Compra Cart Elo Rcmconfeitaria',
  'Rcmconfeitaria','30/04','Compra Cart Elo'),

(@StmtId,@catApl,'DEBIT','20260430000000[-03:EST]','2026-04-30','-03:EST',
  -1849.21,'N2193A:30/04/26:-1849.21:1373725','1373725',
  'Apl.invest Fac',NULL,'30/04','Apl.invest Fac'),

(@StmtId,@catPS,'DEBIT','20260430000000[-03:EST]','2026-04-30','-03:EST',
  -1000.00,'N21957:30/04/26:-1000.0:0953324','0953324',
  'Transfe Pix Des: Lazaro Augusto Gusmao 30/04',
  'Lazaro Augusto Gusmao','30/04','Transfe Pix'),

(@StmtId,@catQS,'DEBIT','20260430000000[-03:EST]','2026-04-30','-03:EST',
  -1000.00,'N21975:30/04/26:-1000.0:1217409','1217409',
  'Pix Qrcode Est Des: Mercado Bitcoin Servi 30/04',
  'Mercado Bitcoin Servi','30/04','Pix Qrcode Est'),

(@StmtId,@catQD,'DEBIT','20260430000000[-03:EST]','2026-04-30','-03:EST',
  -119.37,'N21993:30/04/26:-119.37:1614353','1614353',
  'Pix Qrcode Din Des: Sem Parar Instituicao 30/04',
  'Sem Parar Instituicao','30/04','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260430000000[-03:EST]','2026-04-30','-03:EST',
  -164.26,'N219B1:30/04/26:-164.26:1750549','1750549',
  'Pix Qrcode Din Des: Telefonica Bras 30/04',
  'Telefonica Bras','30/04','Pix Qrcode Din'),

-- ===  04/05/2026  ===
(@StmtId,@catRes,'CREDIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  935.09,'N219CF:04/05/26:935.09:1373725','1373725',
  'Resgate Inv Fac',NULL,'04/05','Resgate Inv Fac'),

(@StmtId,@catPI,'CREDIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  4464.37,'N219EB:04/05/26:4464.37:0846481','0846481',
  'Transfe Pix Rem: Rodrigo Furlaneti 04/05',
  'Rodrigo Furlaneti','04/05','Transfe Pix Rem'),

(@StmtId,@catBP,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -975.28,'N21A08:04/05/26:-975.28:0000005','0000005',
  'Pagto Cobranca Banco Votorantim S/a',
  'Banco Votorantim S/a','04/05','Pagto Cobranca'),

(@StmtId,@catBP,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -1260.88,'N21A26:04/05/26:-1260.88:0000006','0000006',
  'Pagto Cobranca Pefisa S.a-cfi',
  'Pefisa S.a-cfi','04/05','Pagto Cobranca'),

(@StmtId,@catBP,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -85.80,'N21A44:04/05/26:-85.8:0000007','0000007',
  'Pagto Cobranca nu Pagamentos sa',
  'nu Pagamentos sa','04/05','Pagto Cobranca'),

(@StmtId,@catBP,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -1523.95,'N21A62:04/05/26:-1523.95:0000008','0000008',
  'Pagto Cobranca Gci Caixa - Habitacao',
  'Gci Caixa - Habitacao','04/05','Pagto Cobranca'),

(@StmtId,@catCC,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -40.00,'N21A80:04/05/26:-40.0:0025809','0025809',
  'Compra Cart Elo Meeting Point',
  'Meeting Point','04/05','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -17.98,'N21A9E:04/05/26:-17.98:0274800','0274800',
  'Compra Cart Elo Horti Frut Haddock e',
  'Horti Frut Haddock e','04/05','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -43.00,'N21ABC:04/05/26:-43.0:0400256','0400256',
  'Compra Cart Elo Cachacaria Salinas',
  'Cachacaria Salinas','04/05','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -4.99,'N21ADA:04/05/26:-4.99:0580429','0580429',
  'Compra Cart Elo Frutaria Bela Vista',
  'Frutaria Bela Vista','04/05','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -8.57,'N21AF8:04/05/26:-8.57:0711709','0711709',
  'Compra Cart Elo Helio Yabuta',
  'Helio Yabuta','04/05','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -33.70,'N21B16:04/05/26:-33.7:0939745','0939745',
  'Compra Cart Elo Panificadora e Confe',
  'Panificadora e Confe','04/05','Compra Cart Elo'),

(@StmtId,@catPS,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -250.00,'N21B34:04/05/26:-250.0:1049098','1049098',
  'Transfe Pix Des: Nerinalva Peixoto de 01/05',
  'Nerinalva Peixoto de','01/05','Transfe Pix'),

(@StmtId,@catQS,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -240.22,'N21B52:04/05/26:-240.22:0908408','0908408',
  'Pix Qrcode Est Des: Pagueveloz 04/05',
  'Pagueveloz','04/05','Pix Qrcode Est'),

(@StmtId,@catQS,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -57.30,'N21B70:04/05/26:-57.3:0912194','0912194',
  'Pix Qrcode Est Des: nu Pagamentos S/a 04/05',
  'nu Pagamentos S/a','04/05','Pix Qrcode Est'),

(@StmtId,@catQS,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -92.35,'N21B8E:04/05/26:-92.35:0920004','0920004',
  'Pix Qrcode Est Des: nu Pagamentos S/a 04/05',
  'nu Pagamentos S/a','04/05','Pix Qrcode Est'),

(@StmtId,@catQD,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -191.66,'N21BAC:04/05/26:-191.66:0653055','0653055',
  'Pix Qrcode Din Des: Fam Centro Universita 02/05',
  'Fam Centro Universita','02/05','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -160.36,'N21BCA:04/05/26:-160.36:0653515','0653515',
  'Pix Qrcode Din Des: Fam Centro Universita 02/05',
  'Fam Centro Universita','02/05','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -18.11,'N21BE8:04/05/26:-18.11:0850372','0850372',
  'Pix Qrcode Din Des: 57.768.599 Pamela Nob 03/05',
  '57.768.599 Pamela Nob','03/05','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -189.92,'N21C06:04/05/26:-189.92:1133324','1133324',
  'Pix Qrcode Din Des: Fam Centro Universita 04/05',
  'Fam Centro Universita','04/05','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -158.91,'N21C24:04/05/26:-158.91:1134226','1134226',
  'Pix Qrcode Din Des: Fam Centro Universita 04/05',
  'Fam Centro Universita','04/05','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -12.90,'N21C42:04/05/26:-12.9:1231049','1231049',
  'Pix Qrcode Din Des: 57.768.599 Pamela Nob 02/05',
  '57.768.599 Pamela Nob','02/05','Pix Qrcode Din'),

(@StmtId,@catQD,'DEBIT','20260504000000[-03:EST]','2026-05-04','-03:EST',
  -33.58,'N21C60:04/05/26:-33.58:1830105','1830105',
  'Pix Qrcode Din Des: Ifood.com Agencia de 03/05',
  'Ifood.com Agencia de','03/05','Pix Qrcode Din'),

-- ===  05/05/2026  ===
(@StmtId,@catRes,'CREDIT','20260505000000[-03:EST]','2026-05-05','-03:EST',
  108.99,'N21C7E:05/05/26:108.99:1373725','1373725',
  'Resgate Inv Fac',NULL,'05/05','Resgate Inv Fac'),

(@StmtId,@catCC,'DEBIT','20260505000000[-03:EST]','2026-05-05','-03:EST',
  -25.00,'N21C9A:05/05/26:-25.0:0022437','0022437',
  'Compra Cart Elo Skina do Bexiga',
  'Skina do Bexiga','05/05','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260505000000[-03:EST]','2026-05-05','-03:EST',
  -30.00,'N21CB8:05/05/26:-30.0:0126287','0126287',
  'Compra Cart Elo Joia da Vergueiro',
  'Joia da Vergueiro','05/05','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260505000000[-03:EST]','2026-05-05','-03:EST',
  -4.99,'N21CD6:05/05/26:-4.99:0274868','0274868',
  'Compra Cart Elo Horti Frut Haddock e',
  'Horti Frut Haddock e','05/05','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260505000000[-03:EST]','2026-05-05','-03:EST',
  -49.00,'N21CF4:05/05/26:-49.0:0416836','0416836',
  'Compra Cart Elo Cachacaria Salinas',
  'Cachacaria Salinas','05/05','Compra Cart Elo'),

-- ===  06/05/2026  ===
(@StmtId,@catRes,'CREDIT','20260506000000[-03:EST]','2026-05-06','-03:EST',
  755.85,'N21D12:06/05/26:755.85:1373725','1373725',
  'Resgate Inv Fac',NULL,'06/05','Resgate Inv Fac'),

(@StmtId,@catYld,'CREDIT','20260506000000[-03:EST]','2026-05-06','-03:EST',
  0.01,'N21D2E:06/05/26:0.01:1373725','1373725',
  'Rent.inv.facil',NULL,'06/05','Rent.inv.facil'),

(@StmtId,@catBP,'DEBIT','20260506000000[-03:EST]','2026-05-06','-03:EST',
  -362.50,'N21D4A:06/05/26:-362.5:0000009','0000009',
  'Pagto Cobranca Contadora Ana',
  'Contadora Ana','06/05','Pagto Cobranca'),

(@StmtId,@catCC,'DEBIT','20260506000000[-03:EST]','2026-05-06','-03:EST',
  -35.20,'N21D68:06/05/26:-35.2:0266862','0266862',
  'Compra Cart Elo Flor da Joana',
  'Flor da Joana','06/05','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260506000000[-03:EST]','2026-05-06','-03:EST',
  -10.49,'N21D86:06/05/26:-10.49:0274933','0274933',
  'Compra Cart Elo Horti Frut Haddock e',
  'Horti Frut Haddock e','06/05','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260506000000[-03:EST]','2026-05-06','-03:EST',
  -3.50,'N21DA4:06/05/26:-3.5:0359710','0359710',
  'Compra Cart Elo Docemar',
  'Docemar','06/05','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260506000000[-03:EST]','2026-05-06','-03:EST',
  -34.26,'N21DC2:06/05/26:-34.26:0501087','0501087',
  'Compra Cart Elo Dany Com Alimentos l',
  'Dany Com Alimentos l','06/05','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260506000000[-03:EST]','2026-05-06','-03:EST',
  -45.00,'N21DE0:06/05/26:-45.0:0870997','0870997',
  'Compra Cart Elo Noelsbar',
  'Noelsbar','06/05','Compra Cart Elo'),

(@StmtId,@catPS,'DEBIT','20260506000000[-03:EST]','2026-05-06','-03:EST',
  -150.00,'N21DFE:06/05/26:-150.0:1035289','1035289',
  'Transfe Pix Des: Ana Caroline Dos Reis 06/05',
  'Ana Caroline Dos Reis','06/05','Transfe Pix'),

(@StmtId,@catQS,'DEBIT','20260506000000[-03:EST]','2026-05-06','-03:EST',
  -114.91,'N21E1C:06/05/26:-114.91:0933147','0933147',
  'Pix Qrcode Est Des: Enel Distribuicao Sao 06/05',
  'Enel Distribuicao Sao','06/05','Pix Qrcode Est'),

-- ===  07/05/2026  ===
(@StmtId,@catRes,'CREDIT','20260507000000[-03:EST]','2026-05-07','-03:EST',
  49.28,'N21E3A:07/05/26:49.28:1373725','1373725',
  'Resgate Inv Fac',NULL,'07/05','Resgate Inv Fac'),

(@StmtId,@catCC,'DEBIT','20260507000000[-03:EST]','2026-05-07','-03:EST',
  -16.14,'N21E56:07/05/26:-16.14:0000705','0000705',
  'Compra Cart Elo Atacadao 623 as',
  'Atacadao 623 as','07/05','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260507000000[-03:EST]','2026-05-07','-03:EST',
  -4.99,'N21E74:07/05/26:-4.99:0274987','0274987',
  'Compra Cart Elo Horti Frut Haddock e',
  'Horti Frut Haddock e','07/05','Compra Cart Elo'),

(@StmtId,@catCC,'DEBIT','20260507000000[-03:EST]','2026-05-07','-03:EST',
  -18.89,'N21E92:07/05/26:-18.89:0331884','0331884',
  'Compra Cart Elo Gop*emporio Guarulho',
  'Gop*emporio Guarulho','07/05','Compra Cart Elo'),

(@StmtId,@catRes,'CREDIT','20260507000000[-03:EST]','2026-05-07','-03:EST',
  49.28,'N20023:07/05/26:49.28:1373725','1373725',
  'Resgate Invest Facil',NULL,'07/05','Resgate Invest Facil'),

-- ===  08/05/2026  ===
(@StmtId,@catPI,'CREDIT','20260508000000[-03:EST]','2026-05-08','-03:EST',
  400.00,'N2003F:08/05/26:400.0:1330503','1330503',
  'Transferencia Pix Rem: Mercado Bitcoin ip lt 08/05',
  'Mercado Bitcoin ip lt','08/05','Transferencia Pix Rem'),

(@StmtId,@catCD,'DEBIT','20260508000000[-03:EST]','2026-05-08','-03:EST',
  -139.99,'N2005C:08/05/26:-139.99:0027831','0027831',
  'Compra Elo Debito Vista Shopping Guarulhos',
  'Shopping Guarulhos','08/05','Compra Elo Debito Vista'),

(@StmtId,@catCD,'DEBIT','20260508000000[-03:EST]','2026-05-08','-03:EST',
  -18.00,'N2007A:08/05/26:-18.0:0080915','0080915',
  'Compra Elo Debito Vista Consorcio Shop Guaru',
  'Consorcio Shop Guaru','08/05','Compra Elo Debito Vista'),

(@StmtId,@catCD,'DEBIT','20260508000000[-03:EST]','2026-05-08','-03:EST',
  -16.00,'N20098:08/05/26:-16.0:0257324','0257324',
  'Compra Elo Debito Vista Budega Acopiara',
  'Budega Acopiara','08/05','Compra Elo Debito Vista'),

(@StmtId,@catCD,'DEBIT','20260508000000[-03:EST]','2026-05-08','-03:EST',
  -71.45,'N200B6:08/05/26:-71.45:0947463','0947463',
  'Compra Elo Debito Vista Bar Augusta Ltda',
  'Bar Augusta Ltda','08/05','Compra Elo Debito Vista'),

(@StmtId,@catCD,'DEBIT','20260508000000[-03:EST]','2026-05-08','-03:EST',
  -16.99,'N200D4:08/05/26:-16.99:0974791','0974791',
  'Compra Elo Debito Vista Bar Augusta Ltda',
  'Bar Augusta Ltda','08/05','Compra Elo Debito Vista'),

(@StmtId,@catAA,'DEBIT','20260508000000[-03:EST]','2026-05-08','-03:EST',
  -146.83,'N200F2:08/05/26:-146.83:0080526','0080526',
  'Aplic.autom.investfacil*',NULL,'08/05','Aplic.autom.investfacil*'),

-- ===  11/05/2026  ===
(@StmtId,@catCD,'DEBIT','20260511000000[-03:EST]','2026-05-11','-03:EST',
  -18.98,'N2010F:11/05/26:-18.98:0000076','0000076',
  'Compra Elo Debito Vista Atacadao 623 as',
  'Atacadao 623 as','11/05','Compra Elo Debito Vista'),

(@StmtId,@catCD,'DEBIT','20260511000000[-03:EST]','2026-05-11','-03:EST',
  -18.99,'N2012D:11/05/26:-18.99:0102260','0102260',
  'Compra Elo Debito Vista Mini Extra 5087',
  'Mini Extra 5087','11/05','Compra Elo Debito Vista'),

(@StmtId,@catCD,'DEBIT','20260511000000[-03:EST]','2026-05-11','-03:EST',
  -12.00,'N2014B:11/05/26:-12.0:0952238','0952238',
  'Compra Elo Debito Vista Sams Guarulhos',
  'Sams Guarulhos','11/05','Compra Elo Debito Vista');
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
    cat.Code                AS Category,
    cat.AccountingNature,
    COUNT(*)                AS TransactionCount,
    SUM(t.AbsoluteAmount)   AS TotalAmount
FROM ofx.OFX_Transaction t
JOIN ofx.OFX_TransactionCategory cat ON cat.Id = t.CategoryId
GROUP BY cat.Code, cat.AccountingNature
ORDER BY cat.AccountingNature, TotalAmount DESC;
GO

PRINT 'Seed data inserted successfully.';
GO
