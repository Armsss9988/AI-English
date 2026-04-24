import { NotebookEntry, NotebookCategory } from '@english-coach/contracts';
import { apiClient } from '../apiClient';

export const getNotebookEntries = async (category?: NotebookCategory): Promise<NotebookEntry[]> => {
  const query = category ? `?category=${category}` : '';
  return apiClient.get<NotebookEntry[]>(`/error-notebook/entries${query}`);
};

export const archiveEntry = async (id: string): Promise<void> => {
  return apiClient.post<void>(`/error-notebook/entries/${id}/archive`, {});
};
