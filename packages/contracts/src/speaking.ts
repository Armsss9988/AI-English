export type SpeakingStatus = 'idle' | 'recording' | 'evaluating' | 'completed' | 'failed';

export interface SpeakingDrillPrompt {
  id: string;
  context: string;
  prompt: string;
  suggestedPhrases?: string[];
}

export interface SpeakingFeedback {
  overallScore: number;
  overallComments: string;
  mistakes: { original: string; correction: string; explanation: string }[];
  improvedAnswer: string;
  phrasesToReview: string[];
}

export interface SpeakingAttempt {
  id: string;
  drillId: string;
  transcript: string;
  feedback?: SpeakingFeedback;
  status: SpeakingStatus;
}

export interface CreateSpeakingAttemptRequest {
  drillId: string;
  transcript: string;
}
