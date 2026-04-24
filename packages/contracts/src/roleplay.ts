export interface StartRoleplayRequest {
  scenarioId: string;
}

export interface StartRoleplayResponse {
  sessionId: string;
  status: string;
  scenarioTitle: string;
  initialMessage: string;
}

export interface RecordTurnRequest {
  learnerMessage: string;
}

export interface RecordTurnResponse {
  clientMessage: string;
  coachingNote?: string;
  isSessionComplete: boolean;
}

export interface RoleplayTurnResponse {
  speaker: string;
  message: string;
  timestamp: string;
}

export interface RoleplaySessionResponse {
  sessionId: string;
  scenarioId: string;
  status: string;
  turns: RoleplayTurnResponse[];
  summary?: string;
  createdAt: string;
}

// Local frontend state type (not from API)
export interface RoleplayTurn {
  id: string;
  role: "learner" | "ai";
  content: string;
  timestamp: string;
}
