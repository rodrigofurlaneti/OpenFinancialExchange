export type BalanceType = 'LEDGER' | 'AVAIL';

export interface LedgerBalance {
  id: string;
  statementId: string;
  balanceType: BalanceType;
  amount: number;
  asOfDate: string;
  createdAt: string;
}

export interface CreateLedgerBalanceDto {
  statementId: string;
  balanceType: BalanceType;
  amount: number;
  asOfDate: string;
}

export interface UpdateLedgerBalanceDto {
  statementId: string;
  balanceType: BalanceType;
  amount: number;
  asOfDate: string;
}
