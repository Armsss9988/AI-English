import { Phrase, Scenario } from '@english-coach/contracts';

const delay = (ms: number) => new Promise((res) => setTimeout(res, ms));

const MOCK_PHRASES: Phrase[] = [
  { id: 'p1', content: 'Could you please clarify...', meaning: 'Yêu cầu làm rõ...', function: 'Clarification' },
  { id: 'p2', content: 'In other words, what I mean is...', meaning: 'Nói cách khác...', function: 'Explanation' },
  { id: 'p3', content: 'I see your point, however...', meaning: 'Tôi hiểu ý bạn, tuy nhiên...', function: 'Debating' },
  { id: 'p4', content: 'Let me double check that...', meaning: 'Để tôi kiểm tra lại...', function: 'Verification' },
];

const MOCK_SCENARIOS: Scenario[] = [
  { id: 's1', title: 'Daily Standup', goal: 'Report progress and blockers', difficulty: 'Beginner', persona: 'Scrum Master' },
  { id: 's2', title: 'Technical Design Review', goal: 'Explain architecture decisions', difficulty: 'Advanced', persona: 'Senior Architect' },
  { id: 's3', title: 'Salary Negotiation', goal: 'Discuss compensation', difficulty: 'Intermediate', persona: 'HR Manager' },
];

export const getPhrases = async (): Promise<Phrase[]> => {
  await delay(600);
  return [...MOCK_PHRASES];
};

export const getScenarios = async (): Promise<Scenario[]> => {
  await delay(800);
  return [...MOCK_SCENARIOS];
};
