// ─── Shared API envelope ────────────────────────────────────────────────────
export interface ApiError {
  title: string;
  detail: string;
  status: number;
}

// ─── Auth ───────────────────────────────────────────────────────────────────
export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
}

// ─── Financial Institutions ─────────────────────────────────────────────────
export interface FinancialInstitutionResponse {
  id: number;
  bankId: string;
  orgName: string | null;
  fid: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateFinancialInstitutionRequest {
  bankId: string;
  orgName: string | null;
  fid: string | null;
}

export interface UpdateFinancialInstitutionRequest {
  orgName: string | null;
  fid: string | null;
}

// ─── Bank Accounts ──────────────────────────────────────────────────────────
export type AcctType = 'CHECKING' | 'SAVINGS' | 'MONEYMRKT' | 'CREDITLINE' | 'CD' | 'OTHER';

export interface BankAccountResponse {
  id: number;
  financialInstitutionId: number;
  bankId: string;
  branchId: string | null;
  acctId: string;
  acctType: AcctType;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateBankAccountRequest {
  financialInstitutionId: number;
  bankId: string;
  branchId: string | null;
  acctId: string;
  acctType: AcctType;
}

export interface UpdateBankAccountRequest {
  acctType: AcctType;
}

// ─── OFX Imports ────────────────────────────────────────────────────────────
export interface OfxImportResponse {
  id: number;
  fileName: string;
  fileHash: string;
  ofxHeaderVersion: number | null;
  ofxVersion: number | null;
  encoding: string | null;
  charset: string | null;
  importedAt: string;
}

export interface CreateOfxImportRequest {
  bankAccountId: number;
  fileName: string;
  fileHash: string;
  ofxData: string | null;
  ofxHeaderVersion: number | null;
  ofxVersion: number | null;
  encoding: string | null;
  charset: string | null;
  security: string | null;
  compression: string | null;
  oldFileUid: string | null;
  newFileUid: string | null;
}

// ─── OFX Statements ─────────────────────────────────────────────────────────
export interface OfxStatementResponse {
  id: number;
  importId: number;
  bankAccountId: number;
  trnUid: string | null;
  curDef: string;
  dtServer: string | null;
  language: string | null;
  statusCode: number | null;
  statusSeverity: string | null;
  dtStart: string | null;
  dtEnd: string | null;
  createdAt: string;
}

// ─── OFX Transactions ───────────────────────────────────────────────────────
export interface OfxTransactionResponse {
  id: number;
  statementId: number;
  trnType: string;
  dtPosted: string;
  trnAmt: number;
  fitId: string | null;
  name: string | null;
  memo: string | null;
  checkNum: string | null;
  categoryId: number | null;
  categoryName: string | null;
  createdAt: string;
}

export interface AssignCategoryRequest {
  categoryId: number | null;
}

// ─── Categories ─────────────────────────────────────────────────────────────
export type CategoryKind = 'CREDIT' | 'DEBIT' | 'BOTH';

export interface CategoryResponse {
  id: number;
  name: string;
  kind: CategoryKind;
  color: string;
  isSystem: boolean;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateCategoryRequest {
  name: string;
  kind: CategoryKind;
  color: string;
}

export interface UpdateCategoryRequest {
  name: string;
  kind: CategoryKind;
  color: string;
}
