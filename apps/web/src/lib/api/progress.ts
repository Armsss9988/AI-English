import { ReadinessSnapshot } from '@english-coach/contracts';

const delay = (ms: number) => new Promise((res) => setTimeout(res, ms));

const MOCK_SNAPSHOT: ReadinessSnapshot = {
  overallScore: 72,
  date: new Date().toISOString(),
  version: 'v1.2.0',
  trend: 'improving',
  capabilities: [
    { area: 'Technical Explanation', score: 85, explanation: 'Can explain complex architectural decisions with clear logical flow.' },
    { area: 'Meeting Participation', score: 64, explanation: 'Active in syncs but sometimes struggles with clarifying rapid-fire requirements.' },
    { area: 'Written Documentation', score: 78, explanation: 'PR comments and documentation are clear and professional.' },
    { area: 'Social Interaction', score: 55, explanation: 'Still hesitant in non-technical casual conversations.' },
  ],
};

export const getReadinessSnapshot = async (): Promise<ReadinessSnapshot> => {
  await delay(900);
  return { ...MOCK_SNAPSHOT };
};
