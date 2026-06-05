export interface Statement {
  id: string;
  accountId: string;
  trnuid: string | null;
  statusCode: string;
  statusSeverity: string;
  startDate: string;
  endDate: string;
  timeZone: string | null;
  createdAt: string;
}

export interface CreateStatementDto {
  accountId: string;
  trnuid?: string;
  statusCode: string;
  statusSeverity: string;
  startDate: string;
  endDate: string;
  timeZone?: string;
}

export interface UpdateStatementDto {
  accountId: string;
  trnuid?: string;
  statusCode: string;
  statusSeverity: string;
  startDate: string;
  endDate: string;
  timeZone?: string;
}
