export type AccountType = 'CHECKING' | 'SAVINGS' | 'MONEYMRKT' | 'CREDITLINE';

export interface Account {
  id: string;
  importId: string;
  bankId: string;
  branchNumber: string | null;
  accountNumber: string;
  accountType: AccountType;
  defaultCurrency: string;
  createdAt: string;
}

export interface CreateAccountDto {
  importId: string;
  bankId: string;
  branchNumber?: string;
  accountNumber: string;
  accountType: AccountType;
  defaultCurrency: string;
}

export interface UpdateAccountDto {
  importId: string;
  bankId: string;
  branchNumber?: string;
  accountNumber: string;
  accountType: AccountType;
  defaultCurrency: string;
}
