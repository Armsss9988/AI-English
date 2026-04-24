import {
  SpeakingDrillPrompt,
  CreateSpeakingAttemptRequest,
  CreateSpeakingAttemptResponse,
  SubmitSpeakingEvaluationResponse,
} from "@english-coach/contracts";
import { apiClient } from "../apiClient";

export const getDrillPrompt = async (
  id: string
): Promise<SpeakingDrillPrompt> => {
  // Mocking the drill prompt since backend doesn't have an explicit drill endpoint yet
  // It relies on learning-content/scenarios
  return {
    id,
    context: "You are answering a question from a client.",
    prompt: "Can you explain the timeline for the next milestone?",
    suggestedPhrases: ["We expect to...", "By the end of..."],
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
