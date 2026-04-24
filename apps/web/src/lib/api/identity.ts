import { UpdateProfileRequest, UserProfile } from "@english-coach/contracts";
import { apiClient } from "../apiClient";

export const getProfile = async (): Promise<UserProfile> => {
  return apiClient.get<UserProfile>("/me/profile");
};

export const updateProfile = async (
  data: UpdateProfileRequest
): Promise<UserProfile> => {
  return apiClient.put<UserProfile>("/me/profile", data);
};
