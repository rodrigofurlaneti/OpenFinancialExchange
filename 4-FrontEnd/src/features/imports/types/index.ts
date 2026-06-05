export interface Import {
  id: string;
  fileName: string;
  importedAt: string;
  ofxHeader: string | null;
  ofxData: string | null;
  ofxVersion: string | null;
  ofxSecurity: string | null;
  ofxEncoding: string | null;
  ofxCharset: string | null;
  ofxCompression: string | null;
  ofxOldFileUID: string | null;
  ofxNewFileUID: string | null;
  notes: string | null;
  importedBy: string | null;
  createdAt: string;
}

export interface CreateImportDto {
  fileName: string;
  importedAt: string;
  ofxHeader?: string;
  ofxData?: string;
  ofxVersion?: string;
  ofxSecurity?: string;
  ofxEncoding?: string;
  ofxCharset?: string;
  ofxCompression?: string;
  ofxOldFileUID?: string;
  ofxNewFileUID?: string;
  notes?: string;
  importedBy?: string;
}

export interface UpdateImportDto {
  notes: string | null;
}
