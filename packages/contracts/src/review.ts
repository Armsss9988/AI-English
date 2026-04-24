export type ReviewOutcome = 'again' | 'hard' | 'good' | 'easy';

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
