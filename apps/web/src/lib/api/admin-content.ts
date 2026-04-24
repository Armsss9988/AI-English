import { AdminPhrase, AdminScenario, UpsertPhraseRequest, UpsertScenarioRequest } from '@english-coach/contracts';

const delay = (ms: number) => new Promise((res) => setTimeout(res, ms));

const MOCK_ADMIN_PHRASES: AdminPhrase[] = [
  { id: 'p1', content: 'Could you please clarify...', meaning: 'Yêu cầu làm rõ...', category: 'Meetings', difficulty: 'Beginner', status: 'published' },
];

const MOCK_ADMIN_SCENARIOS: AdminScenario[] = [
  { id: 's1', title: 'Daily Standup', description: 'Briefly explain your status.', difficulty: 'Beginner', category: 'General', status: 'published' },
];

export const getAdminPhrases = async (): Promise<AdminPhrase[]> => {
  await delay(500);
  return [...MOCK_ADMIN_PHRASES];
};

export const getAdminScenarios = async (): Promise<AdminScenario[]> => {
  await delay(500);
  return [...MOCK_ADMIN_SCENARIOS];
};

export const upsertPhrase = async (data: UpsertPhraseRequest): Promise<void> => {
  await delay(1000);
  console.log('Upserted phrase:', data);
};

export const upsertScenario = async (data: UpsertScenarioRequest): Promise<void> => {
  await delay(1000);
  console.log('Upserted scenario:', data);
};
