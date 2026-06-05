export interface SignonSession {
  id: string;
  importId: string;
  statusCode: string;
  statusSeverity: string;
  serverDateRaw: string | null;
  serverDate: string | null;
  language: string | null;
  createdAt: string;
}

export interface CreateSignonSessionDto {
  importId: string;
  statusCode: string;
  statusSeverity: string;
  serverDateRaw?: string;
  serverDate?: string;
  language?: string;
}

export interface UpdateSignonSessionDto {
  statusCode: string;
  statusSeverity: string;
  serverDateRaw?: string;
  serverDate?: string;
  language?: string;
}
