import { ReadinessSnapshot } from "@english-coach/contracts";
import { apiClient } from "../apiClient";

export const getReadinessSnapshot = async (): Promise<ReadinessSnapshot> => {
  return apiClient.get<ReadinessSnapshot>("/progress/readiness");
};
