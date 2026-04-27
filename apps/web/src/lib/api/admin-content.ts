import type {
  AdminPhrase,
  AdminScenario,
  CreatePhraseRequest,
  UpdatePhraseRequest,
  CreateScenarioRequest,
  UpdateScenarioRequest,
} from "@english-coach/contracts";
import { apiClient } from "../apiClient";

// ── Phrases ──

export const getAdminPhrases = async (): Promise<AdminPhrase[]> => {
  return apiClient.get<AdminPhrase[]>("/admin/content/phrases");
};

export const createPhrase = async (
  data: CreatePhraseRequest
): Promise<AdminPhrase> => {
  return apiClient.post<AdminPhrase>("/admin/content/phrases", data);
};

export const updatePhrase = async (
  phraseId: string,
  data: UpdatePhraseRequest
): Promise<AdminPhrase> => {
  return apiClient.put<AdminPhrase>(
    `/admin/content/phrases/${phraseId}`,
    data
  );
};

export const publishPhrase = async (
  phraseId: string
): Promise<AdminPhrase> => {
  return apiClient.post<AdminPhrase>(
    `/admin/content/phrases/${phraseId}/publish`,
    {}
  );
};

// ── Scenarios ──

export const getAdminScenarios = async (): Promise<AdminScenario[]> => {
  return apiClient.get<AdminScenario[]>("/admin/content/scenarios");
};

export const createScenario = async (
  data: CreateScenarioRequest
): Promise<AdminScenario> => {
  return apiClient.post<AdminScenario>("/admin/content/scenarios", data);
};

export const updateScenario = async (
  scenarioId: string,
  data: UpdateScenarioRequest
): Promise<AdminScenario> => {
  return apiClient.put<AdminScenario>(
    `/admin/content/scenarios/${scenarioId}`,
    data
  );
};

export const publishScenario = async (
  scenarioId: string
): Promise<AdminScenario> => {
  return apiClient.post<AdminScenario>(
    `/admin/content/scenarios/${scenarioId}/publish`,
    {}
  );
};
