export interface RoleplayTurn {
  id: string;
  role: "learner" | "ai";
  content: string;
  timestamp: string;
}

export interface RoleplaySession {
  id: string;
  scenarioId: string;
  scenarioTitle: string;
  turns: RoleplayTurn[];
  status: "active" | "completed";
  summary?: string; // Coaching summary after finalization
}

export interface StartRoleplayRequest {
  scenarioId: string;
}

export interface RecordTurnRequest {
  sessionId: string;
  content: string;
}
