export type ContentStatus = "draft" | "review" | "published" | "deprecated" | "archived";

export type CommunicationFunction =
  | "Standup"
  | "Issue"
  | "Clarification"
  | "Eta"
  | "Recommendation"
  | "Summary";

export type ContentLevel = "Survival" | "Core" | "ClientReady";

export interface AdminPhrase {
  id: string;
  content: string;
  meaning: string;
  category: string;
  difficulty: string;
  example: string;
  status: ContentStatus;
  contentVersion: number;
}

export interface AdminScenario {
  id: string;
  title: string;
  goal: string;
  workplaceContext: string;
  userRole: string;
  persona: string;
  mustCoverPoints: string[];
  passCriteria: string[];
  difficulty: number;
  status: ContentStatus;
  contentVersion: number;
}

export interface CreatePhraseRequest {
  content: string;
  meaning: string;
  category: CommunicationFunction;
  difficulty: ContentLevel;
  example: string;
}

export interface UpdatePhraseRequest {
  content: string;
  meaning: string;
  category: CommunicationFunction;
  difficulty: ContentLevel;
  example: string;
}

export interface CreateScenarioRequest {
  title: string;
  goal: string;
  workplaceContext: string;
  userRole: string;
  persona: string;
  mustCoverPoints: string[];
  passCriteria: string[];
  difficulty: number;
}

export interface UpdateScenarioRequest {
  title: string;
  goal: string;
  workplaceContext: string;
  userRole: string;
  persona: string;
  mustCoverPoints: string[];
  passCriteria: string[];
  difficulty: number;
}
