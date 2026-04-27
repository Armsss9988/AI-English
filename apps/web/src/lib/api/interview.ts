import {
  UploadCvResponse,
  StartInterviewRequest,
  StartInterviewResponse,
  AnswerQuestionRequest,
  AnswerQuestionResponse,
  InterviewFeedbackResponse,
  InterviewHistoryResponse,
} from "@english-coach/contracts";
import { apiClient } from "../apiClient";

const BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://127.0.0.1:5237";

function getUploadErrorMessage(status: number, bodyText: string): string {
  if (!bodyText) {
    return `API error: ${status}`;
  }

  try {
    const errorBody = JSON.parse(bodyText) as {
      message?: unknown;
      detail?: unknown;
      title?: unknown;
    };

    if (typeof errorBody.message === "string") return errorBody.message;
    if (typeof errorBody.detail === "string") return errorBody.detail;
    if (typeof errorBody.title === "string") return errorBody.title;
  } catch {
    return bodyText;
  }

  return `API error: ${status}`;
}

export const uploadCv = async (cvText: string): Promise<UploadCvResponse> => {
  return apiClient.post<UploadCvResponse>("/me/interview/upload-cv", {
    cvText,
  });
};

export const getLatestCvProfile =
  async (): Promise<UploadCvResponse | null> => {
    const response = await fetch(`${BASE_URL}/me/interview/profile/latest`, {
      method: "GET",
      headers: {
        "X-User-Id": "00000000-0000-0000-0000-000000000001",
        "X-User-Role": "Admin",
      },
    });

    if (response.status === 404) {
      return null;
    }

    const responseText = await response.text();

    if (!response.ok) {
      throw new Error(getUploadErrorMessage(response.status, responseText));
    }

    return JSON.parse(responseText) as UploadCvResponse;
  };

export const uploadCvFile = async (file: File): Promise<UploadCvResponse> => {
  const formData = new FormData();
  formData.append("file", file);

  const response = await fetch(`${BASE_URL}/me/interview/upload-cv-file`, {
    method: "POST",
    headers: {
      "X-User-Id": "00000000-0000-0000-0000-000000000001",
      "X-User-Role": "Admin",
    },
    body: formData,
  });

  const responseText = await response.text();

  if (!response.ok) {
    throw new Error(getUploadErrorMessage(response.status, responseText));
  }

  return JSON.parse(responseText) as UploadCvResponse;
};

export const startInterview = async (
  data: StartInterviewRequest
): Promise<StartInterviewResponse> => {
  return apiClient.post<StartInterviewResponse>(
    "/me/interview/sessions",
    data
  );
};

export const answerQuestion = async (
  sessionId: string,
  data: AnswerQuestionRequest
): Promise<AnswerQuestionResponse> => {
  return apiClient.post<AnswerQuestionResponse>(
    `/me/interview/sessions/${sessionId}/answer`,
    data
  );
};

export const finalizeInterview = async (
  sessionId: string
): Promise<InterviewFeedbackResponse> => {
  return apiClient.post<InterviewFeedbackResponse>(
    `/me/interview/sessions/${sessionId}/finalize`,
    {}
  );
};

export const getInterviewHistory =
  async (): Promise<InterviewHistoryResponse> => {
    return apiClient.get<InterviewHistoryResponse>("/me/interview/sessions");
  };
