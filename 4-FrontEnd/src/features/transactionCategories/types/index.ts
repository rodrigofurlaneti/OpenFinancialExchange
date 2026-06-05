export type AccountingNature = 'REVENUE' | 'EXPENSE' | 'TRANSFER';

export interface TransactionCategory {
  id: string;
  code: string;
  description: string;
  operationType: string;
  accountingNature: AccountingNature;
  isActive: boolean;
  createdAt: string;
}

export interface CreateTransactionCategoryDto {
  code: string;
  description: string;
  operationType: string;
  accountingNature: AccountingNature;
  isActive: boolean;
}

export interface UpdateTransactionCategoryDto {
  code: string;
  description: string;
  operationType: string;
  accountingNature: AccountingNature;
  isActive: boolean;
}
