import { UserProfile, UpdateProfileRequest } from '@english-coach/contracts';

// Mock delay
const delay = (ms: number) => new Promise((res) => setTimeout(res, ms));

// Mock database
let mockProfile: UserProfile = {
  id: 'user-123',
  role: 'Software Engineer',
  timezone: 'UTC+7',
  currentLevel: 'B1',
  targetUseCase: 'IT Client Meetings',
};

export const getProfile = async (): Promise<UserProfile> => {
  await delay(800);
  return { ...mockProfile };
};

export const updateProfile = async (data: UpdateProfileRequest): Promise<UserProfile> => {
  await delay(1200);
  // Simulate potential validation error (for testing UI states)
  if (data.role.length < 2) {
    throw new Error('Role must be at least 2 characters long');
  }

  mockProfile = {
    ...mockProfile,
    ...data,
  };
  return { ...mockProfile };
};
