import {
  StartRoleplayResponse,
  RecordTurnRequest,
  RecordTurnResponse,
  RoleplaySessionResponse,
} from "@english-coach/contracts";
import { apiClient } from "../apiClient";

export const startRoleplay = async (
  scenarioId: string
): Promise<StartRoleplayResponse> => {
  return apiClient.post<StartRoleplayResponse>(`/me/roleplay/start`, {
    scenarioId,
  });
};

export const recordTurn = async (
  sessionId: string,
  data: RecordTurnRequest
): Promise<RecordTurnResponse> => {
  return apiClient.post<RecordTurnResponse>(`/me/roleplay/${sessionId}/turn`, data);
};

export const finalizeRoleplay = async (
  sessionId: string
): Promise<RoleplaySessionResponse> => {
  return apiClient.post<RoleplaySessionResponse>(
    `/me/roleplay/${sessionId}/finalize`,
    {}
  );
};
