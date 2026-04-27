// ---- CV Upload ----
export interface UploadCvRequest {
  cvText: string;
}

export interface UploadCvResponse {
  profileId: string;
  cvAnalysis: string;
}

// ---- Start Interview ----
export interface StartInterviewRequest {
  profileId: string;
  jdText: string;
  interviewType: "Mixed" | "Behavioral" | "Technical" | "Situational";
}

export interface StartInterviewResponse {
  sessionId: string;
  status: string;
  interviewType: string;
  plannedQuestionCount: number;
  firstQuestion: string;
  questionCategory: string;
  coachingHint?: string;
}

// ---- Answer Question ----
export interface AnswerQuestionRequest {
  answer: string;
  audioUrl?: string;
}

export interface AnswerQuestionResponse {
  nextQuestion?: string;
  questionCategory?: string;
  coachingHint?: string;
  isInterviewComplete: boolean;
  answeredCount: number;
  totalQuestions: number;
}

// ---- Feedback ----
export interface InterviewFeedbackResponse {
  sessionId: string;
  overallScore: number;
  communicationScore: number;
  technicalAccuracyScore: number;
  confidenceScore: number;
  detailedFeedbackEn: string;
  detailedFeedbackVi: string;
  strengthAreas: string[];
  improvementAreas: string[];
  suggestedPhrases: string[];
  retryRecommendation: string;
}

// ---- History ----
export interface InterviewHistoryResponse {
  sessions: InterviewHistoryItem[];
}

export interface InterviewHistoryItem {
  sessionId: string;
  interviewType: string;
  status: string;
  plannedQuestionCount: number;
  answeredCount: number;
  overallScore?: number;
  createdAt: string;
}

// ---- Local frontend state ----
export interface InterviewTurn {
  id: string;
  role: "interviewer" | "learner";
  content: string;
  category?: string;
  timestamp: string;
}
