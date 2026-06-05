import { apiClient } from '../../../core/api/client';
import { Import, CreateImportDto, UpdateImportDto } from '../types';

const BASE = '/imports';

export async function getAllImports(): Promise<Import[]> {
  const res = await apiClient.get<Import[]>(BASE);
  return res.data;
}

export async function getImportById(id: string): Promise<Import> {
  const res = await apiClient.get<Import>(`${BASE}/${id}`);
  return res.data;
}

export async function createImport(dto: CreateImportDto): Promise<Import> {
  const res = await apiClient.post<Import>(BASE, dto);
  return res.data;
}

export async function updateImport(id: string, dto: UpdateImportDto): Promise<Import> {
  const res = await apiClient.put<Import>(`${BASE}/${id}`, dto);
  return res.data;
}

export async function deleteImport(id: string): Promise<void> {
  await apiClient.delete(`${BASE}/${id}`);
}
