import { DailyMission } from '@english-coach/contracts';

const delay = (ms: number) => new Promise((res) => setTimeout(res, ms));

const MOCK_MISSION: DailyMission = {
  date: new Date().toISOString(),
  missions: [
    { id: 'm1', type: 'review', title: 'Daily Review', description: 'Complete 10 due phrases', isCompleted: false, count: 10 },
    { id: 'm2', type: 'speaking', title: 'Speaking Drill', description: 'Practice 3 phrases aloud', isCompleted: true, count: 3 },
    { id: 'm3', type: 'roleplay', title: 'Client Meeting', goal: 'Practice clarifying requirements', isCompleted: false } as any,
  ],
};

export const getDailyMission = async (): Promise<DailyMission> => {
  await delay(700);
  return { ...MOCK_MISSION };
};
