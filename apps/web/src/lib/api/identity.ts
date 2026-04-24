import { UserProfile, UpdateProfileRequest } from '@english-coach/contracts';
import { apiClient } from '../apiClient';

export const getProfile = async (): Promise<UserProfile> => {
  return apiClient.get<UserProfile>('/identity/profile');
};

export const updateProfile = async (data: UpdateProfileRequest): Promise<UserProfile> => {
  return apiClient.post<UserProfile>('/identity/profile', data);
};
