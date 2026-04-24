import { RoleplaySession, RoleplayTurn, RecordTurnRequest } from '@english-coach/contracts';

const delay = (ms: number) => new Promise((res) => setTimeout(res, ms));

let mockSession: RoleplaySession = {
  id: 'session-1',
  scenarioId: 's1',
  scenarioTitle: 'Daily Standup',
  status: 'active',
  turns: [
    { id: 't1', role: 'ai', content: "Hi! It's time for our standup. What did you work on yesterday?", timestamp: new Date().toISOString() },
  ],
};

export const startRoleplay = async (scenarioId: string): Promise<RoleplaySession> => {
  await delay(800);
  return { ...mockSession };
};

export const recordTurn = async (data: RecordTurnRequest): Promise<RoleplayTurn> => {
  await delay(1200);
  const learnerTurn: RoleplayTurn = {
    id: 't-' + Math.random(),
    role: 'learner',
    content: data.content,
    timestamp: new Date().toISOString(),
  };
  
  mockSession.turns.push(learnerTurn);

  // Simulate AI response
  await delay(1500);
  const aiTurn: RoleplayTurn = {
    id: 't-' + Math.random(),
    role: 'ai',
    content: "I see. And are there any blockers for the API integration?",
    timestamp: new Date().toISOString(),
  };
  
  mockSession.turns.push(aiTurn);
  return aiTurn;
};

export const finalizeRoleplay = async (sessionId: string): Promise<RoleplaySession> => {
  await delay(2000);
  mockSession.status = 'completed';
  mockSession.summary = "Good session! You explained the technical status clearly. Try to use more active verbs like 'implemented' instead of 'did'.";
  return { ...mockSession };
};
