import { AdminPhrase, AdminScenario, UpsertPhraseRequest, UpsertScenarioRequest } from '@english-coach/contracts';
import { apiClient } from '../apiClient';

export const getAdminPhrases = async (): Promise<AdminPhrase[]> => {
  return apiClient.get<AdminPhrase[]>('/admin/content/phrases');
};

export const getAdminScenarios = async (): Promise<AdminScenario[]> => {
  return apiClient.get<AdminScenario[]>('/admin/content/scenarios');
};

export const upsertPhrase = async (data: UpsertPhraseRequest): Promise<void> => {
  return apiClient.post<void>('/admin/content/phrases', data);
};

export const upsertScenario = async (data: UpsertScenarioRequest): Promise<void> => {
  return apiClient.post<void>('/admin/content/scenarios', data);
};
