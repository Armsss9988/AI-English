export type EnglishLevel = "A1" | "A2" | "B1" | "B2" | "C1" | "C2";

export interface UserProfile {
  id: string;
  role: string;
  timezone: string;
  currentLevel: EnglishLevel;
  targetUseCase: string;
}

export interface UpdateProfileRequest {
  role: "dev" | "qa" | "ba" | "pm" | "support" | "other";
  timezone: string;
  currentLevel: EnglishLevel;
  targetUseCase: string;
  displayName?: string;
  nativeLanguage?: string;
  targetTimelineWeeks?: number;
}
