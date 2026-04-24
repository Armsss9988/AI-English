export interface CreateSpeakingAttemptRequest {
  contentItemId: string;
  initialTranscript?: string;
}

export interface CreateSpeakingAttemptResponse {
  attemptId: string;
}

export interface SubmitSpeakingEvaluationResponse {
  topMistakes: string;
  improvedAnswer: string;
  phrasesToReview: string;
  retryPrompt: string;
}

// Local mock type for drill UI context
export interface SpeakingDrillPrompt {
  id: string;
  context: string;
  prompt: string;
  suggestedPhrases?: string[];
}
