import {
  SpeakingDrillPrompt,
  CreateSpeakingAttemptRequest,
  CreateSpeakingAttemptResponse,
  SubmitSpeakingEvaluationResponse,
} from "@english-coach/contracts";
import { apiClient } from "../apiClient";
import { getPhrases } from "./curriculum";

export const getDrillPrompt = async (
  id: string
): Promise<SpeakingDrillPrompt> => {
  const phrases = await getPhrases();
  const phrase = phrases.find((item) => item.id === id) ?? phrases[0];

  if (phrase) {
    return {
      id: phrase.id,
      context: phrase.meaning,
      prompt: `Write a client-ready response that naturally uses: "${phrase.content}"`,
      suggestedPhrases: [phrase.content],
    };
  }

  return {
    id,
    context: "You are answering a question from a client.",
    prompt: "Write a clear update for an English-speaking client.",
    suggestedPhrases: [],
  };
};

export const submitAttempt = async (
  data: CreateSpeakingAttemptRequest
): Promise<SubmitSpeakingEvaluationResponse> => {
  // Step 1: Create attempt
  const { attemptId } = await apiClient.post<CreateSpeakingAttemptResponse>(
    "/me/speaking/attempt",
    data
  );

  // Step 2: Evaluate attempt
  const feedback = await apiClient.post<SubmitSpeakingEvaluationResponse>(
    `/me/speaking/attempt/${attemptId}/evaluate`,
    {}
  );

  return feedback;
};
