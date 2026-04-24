export type ReviewQuality = "again" | "hard" | "good" | "easy";
export type ReviewOutcome = ReviewQuality;

export interface ReviewItem {
  id: string;
  phraseId: string;
  content: string;
  meaning: string;
  masteryLevel: number;
}

export interface CompleteReviewRequest {
  reviewItemId: string;
  outcome: ReviewOutcome;
}

export interface DueReviewItem {
  reviewItemId: string;
  itemId: string;
  reviewTrack: "phrase" | "error" | "scenario";
  displayText: string;
  displaySubtitle?: string | null;
  masteryState:
    | "new"
    | "learning"
    | "weak"
    | "review"
    | "strong"
    | "client_ready";
  repetitionCount: number;
  dueAtUtc: string;
}

export interface GetDueReviewItemsResponse {
  items: DueReviewItem[];
}

export interface CompleteReviewItemRequest {
  quality: ReviewQuality;
}

export interface CompleteReviewItemResponse {
  reviewItemId: string;
  nextMasteryState: DueReviewItem["masteryState"];
  nextDueAtUtc: string;
  repetitionCount: number;
}
