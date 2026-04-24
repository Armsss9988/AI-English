import { Phrase, RoleplayScenario } from '@english-coach/contracts';
import { apiClient } from '../apiClient';

export const getPhrases = async (category?: string): Promise<Phrase[]> => {
  const query = category ? `?category=${category}` : '';
  return apiClient.get<Phrase[]>(`/learning-content/phrases${query}`);
};

export const getScenarios = async (): Promise<RoleplayScenario[]> => {
  return apiClient.get<RoleplayScenario[]>('/learning-content/scenarios');
};
