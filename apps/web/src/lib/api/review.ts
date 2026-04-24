import { ReviewItem, CompleteReviewRequest } from '@english-coach/contracts';

const delay = (ms: number) => new Promise((res) => setTimeout(res, ms));

let MOCK_DUE_QUEUE: ReviewItem[] = [
  { id: 'r1', phraseId: 'p1', content: 'Could you please clarify...', meaning: 'Yêu cầu làm rõ...', masteryLevel: 1 },
  { id: 'r2', phraseId: 'p2', content: 'In other words, what I mean is...', meaning: 'Nói cách khác...', masteryLevel: 2 },
];

export const getDueReviews = async (): Promise<ReviewItem[]> => {
  await delay(800);
  return [...MOCK_DUE_QUEUE];
};

export const completeReviewItem = async (data: CompleteReviewRequest): Promise<void> => {
  await delay(1000);
  console.log(`Review item ${data.reviewItemId} completed with outcome: ${data.outcome}`);
  MOCK_DUE_QUEUE = MOCK_DUE_QUEUE.filter(item => item.id !== data.reviewItemId);
};
