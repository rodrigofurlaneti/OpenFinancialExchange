export interface Bank {
  id: string;
  compeCode: string;
  bankName: string;
  ispb: string;
  isActive: boolean;
  createdAt: string;
}

export interface CreateBankDto {
  compeCode: string;
  bankName: string;
  ispb: string;
  isActive: boolean;
}

export interface UpdateBankDto {
  compeCode: string;
  bankName: string;
  ispb: string;
  isActive: boolean;
}
