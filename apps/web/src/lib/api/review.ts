import { ReviewItem, CompleteReviewRequest } from '@english-coach/contracts';
import { apiClient } from '../apiClient';

export const getDueReviews = async (): Promise<ReviewItem[]> => {
  return apiClient.get<ReviewItem[]>('/srs-reviews/due');
};

export const completeReviewItem = async (data: CompleteReviewRequest): Promise<void> => {
  return apiClient.post<void>('/srs-reviews/complete', data);
};
