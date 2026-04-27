// ── T01: Domain concept types ──

export type InterviewCapability =
  | "SelfIntroduction"
  | "ProjectDeepDive"
  | "TechnicalTradeoff"
  | "BehavioralStar"
  | "ClientCommunication"
  | "RequirementClarification"
  | "IncidentConflictStory"
  | "WeakSpotRetry"
  | "EnglishClarity"
  | "PronunciationClarity";

export type InterviewTurnType =
  | "OpeningQuestion"
  | "MainQuestion"
  | "FollowUp"
  | "Clarification"
  | "Challenge"
  | "Transition"
  | "Closing";

export type InterviewMode = "RealInterview" | "TrainingInterview";

export type InterviewVerificationStatus =
  | "Verified"
  | "Unverified"
  | "Fallback";

export type InterviewTurnState =
  | "Created"
  | "AudioReady"
  | "LearnerAudioUploaded"
  | "TranscriptReady"
  | "TranscriptConfirmed"
  | "PronunciationAssessed"
  | "AnswerEvaluated"
  | "Superseded";

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
  interviewMode?: InterviewMode;
}

export interface StartInterviewResponse {
  sessionId: string;
  status: string;
  interviewType: string;
  interviewMode: string;
  plannedQuestionCount: number;
  firstQuestion: string;
  questionCategory: string;
  turnType?: InterviewTurnType;
  targetCapability?: InterviewCapability;
  coachingHint?: string;
  audioUrl?: string;
}

// ---- Answer Question ----
export interface AnswerQuestionRequest {
  answer: string;
  audioUrl?: string;
}

export interface AnswerQuestionResponse {
  nextQuestion?: string;
  questionCategory?: string;
  turnType?: InterviewTurnType;
  targetCapability?: InterviewCapability;
  coachingHint?: string;
  isInterviewComplete: boolean;
  answeredCount: number;
  totalQuestions: number;
  audioUrl?: string;
}

// ---- Transcript Confirmation (T06) ----
export interface ConfirmTranscriptRequest {
  confirmedTranscript: string;
  learnerEdited: boolean;
}

export interface TranscriptResponse {
  turnId: string;
  rawTranscript: string;
  confidence: number;
  turnState: InterviewTurnState;
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
  interviewMode: string;
  status: string;
  plannedQuestionCount: number;
  answeredCount: number;
  overallScore?: number;
  createdAt: string;
}

// ---- Session Detail (T10) ----
export interface InterviewSessionDetailResponse {
  sessionId: string;
  interviewType: string;
  interviewMode: string;
  status: string;
  plannedQuestionCount: number;
  answeredCount: number;
  jdSummary: string;
  turns: InterviewTurnDto[];
  feedback?: InterviewFeedbackResponse;
}

export interface InterviewTurnDto {
  turnId: string;
  role: "interviewer" | "learner";
  message: string;
  turnType?: InterviewTurnType;
  targetCapability?: InterviewCapability;
  category?: string;
  audioUrl?: string;
  audioDurationMs?: number;
  rawTranscript?: string;
  confirmedTranscript?: string;
  transcriptConfidence?: number;
  coachingHint?: string;
  turnState: InterviewTurnState;
  verificationStatus: InterviewVerificationStatus;
  createdAt: string;
}

// ---- Local frontend state ----
export interface InterviewTurn {
  id: string;
  role: "interviewer" | "learner";
  content: string;
  category?: string;
  turnType?: InterviewTurnType;
  targetCapability?: InterviewCapability;
  audioUrl?: string;
  turnState?: InterviewTurnState;
  timestamp: string;
}
