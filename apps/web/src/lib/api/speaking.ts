import {
  SpeakingDrillPrompt,
  SpeakingAttempt,
  CreateSpeakingAttemptRequest,
} from "@english-coach/contracts";
import { apiClient } from "../apiClient";

export const getDrillPrompt = async (
  id: string
): Promise<SpeakingDrillPrompt> => {
  return apiClient.get<SpeakingDrillPrompt>(`/speaking-sessions/drills/${id}`);
};

export const submitAttempt = async (
  data: CreateSpeakingAttemptRequest
): Promise<SpeakingAttempt> => {
  return apiClient.post<SpeakingAttempt>("/speaking-sessions/submit", data);
};
