import { DailyMission } from '@english-coach/contracts';
import { apiClient } from '../apiClient';

export const getDailyMission = async (): Promise<DailyMission> => {
  return apiClient.get<DailyMission>('/progress/daily-mission');
};
