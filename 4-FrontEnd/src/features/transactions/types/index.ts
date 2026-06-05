export type MovementNature = 'CREDIT' | 'DEBIT';

export interface Transaction {
  id: string;
  statementId: string;
  categoryId: string | null;
  transactionType: string;
  postedDateRaw: string | null;
  postedDate: string | null;
  timeZone: string | null;
  amount: number;
  fitid: string | null;
  checkNumber: string | null;
  memo: string | null;
  absoluteAmount: number;
  movementNature: MovementNature;
  payeeName: string | null;
  transactionDateMemo: string | null;
  operationSubtype: string | null;
  isReconciled: boolean;
  reconciledAt: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateTransactionDto {
  statementId: string;
  categoryId?: string;
  transactionType: string;
  postedDateRaw?: string;
  postedDate?: string;
  timeZone?: string;
  amount: number;
  fitid?: string;
  checkNumber?: string;
  memo?: string;
  absoluteAmount: number;
  movementNature: MovementNature;
  payeeName?: string;
  transactionDateMemo?: string;
  operationSubtype?: string;
}

export interface UpdateTransactionDto {
  statementId: string;
  categoryId?: string;
  transactionType: string;
  postedDateRaw?: string;
  postedDate?: string;
  timeZone?: string;
  amount: number;
  fitid?: string;
  checkNumber?: string;
  memo?: string;
  absoluteAmount: number;
  movementNature: MovementNature;
  payeeName?: string;
  transactionDateMemo?: string;
  operationSubtype?: string;
}

export interface TransactionFilters {
  from?: string;
  to?: string;
  isReconciled?: boolean | null;
  statementId?: string;
  categoryId?: string;
}
