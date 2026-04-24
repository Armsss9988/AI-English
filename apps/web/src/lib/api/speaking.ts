import { SpeakingDrillPrompt, SpeakingAttempt, CreateSpeakingAttemptRequest, SpeakingFeedback } from '@english-coach/contracts';

const delay = (ms: number) => new Promise((res) => setTimeout(res, ms));

const MOCK_DRILL: SpeakingDrillPrompt = {
  id: 'd1',
  context: 'A client asks about the project timeline during a weekly sync.',
  prompt: 'How would you explain that the frontend work is 80% done but waiting for API integration?',
  suggestedPhrases: ['on track', 'blocked by', 'integration phase'],
};

const MOCK_FEEDBACK: SpeakingFeedback = {
  overallScore: 85,
  overallComments: 'Clear explanation of status, but could use more professional transitional phrases.',
  mistakes: [
    { original: 'we are waiting API', correction: 'we are waiting for the API', explanation: 'Wait is followed by "for" when indicating the object of the wait.' },
  ],
  improvedAnswer: 'The frontend development is currently 80% complete and on track. We are now entering the integration phase, though we are currently waiting for the final API endpoints to be released.',
  phrasesToReview: ['integration phase', 'on track'],
};

export const getDrillPrompt = async (id: string): Promise<SpeakingDrillPrompt> => {
  await delay(600);
  return { ...MOCK_DRILL };
};

export const submitAttempt = async (data: CreateSpeakingAttemptRequest): Promise<SpeakingAttempt> => {
  await delay(1500); // Simulate evaluation time
  return {
    id: 'a-' + Math.random().toString(36).substr(2, 5),
    drillId: data.drillId,
    transcript: data.transcript,
    status: 'completed',
    feedback: MOCK_FEEDBACK,
  };
};
