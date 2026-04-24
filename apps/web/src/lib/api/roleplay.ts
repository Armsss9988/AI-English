import {
  RoleplaySession,
  RoleplayTurn,
  RecordTurnRequest,
} from "@english-coach/contracts";
import { apiClient } from "../apiClient";

export const startRoleplay = async (
  scenarioId: string
): Promise<RoleplaySession> => {
  return apiClient.post<RoleplaySession>(`/roleplay-sessions/start`, {
    scenarioId,
  });
};

export const recordTurn = async (
  data: RecordTurnRequest
): Promise<RoleplayTurn> => {
  return apiClient.post<RoleplayTurn>(`/roleplay-sessions/turns`, data);
};

export const finalizeRoleplay = async (
  sessionId: string
): Promise<RoleplaySession> => {
  return apiClient.post<RoleplaySession>(
    `/roleplay-sessions/${sessionId}/finalize`,
    {}
  );
};
