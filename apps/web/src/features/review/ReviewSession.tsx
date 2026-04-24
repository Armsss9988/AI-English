'use client';

import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { ReviewOutcome } from '@english-coach/contracts';
import { Button } from '@english-coach/ui';
import { getDueReviews, completeReviewItem } from '@/lib/api/review';
import styles from './review.module.css';

export const ReviewSession: React.FC = () => {
  const queryClient = useQueryClient();
  const [currentIndex, setCurrentIndex] = useState(0);
  const [showMeaning, setShowMeaning] = useState(false);

  const { data: queue, isLoading } = useQuery({
    queryKey: ['due-reviews'],
    queryFn: getDueReviews,
  });

  const mutation = useMutation({
    mutationFn: completeReviewItem,
    onSuccess: () => {
      setShowMeaning(false);
      // We don't necessarily need to invalidate if we're just moving through the local state,
      // but it's good practice for when we finish.
      if (queue && currentIndex >= queue.length - 1) {
        queryClient.invalidateQueries({ queryKey: ['due-reviews'] });
      } else {
        setCurrentIndex(prev => prev + 1);
      }
    },
  });

  const handleOutcome = (outcome: ReviewOutcome) => {
    if (!queue) return;
    mutation.mutate({
      reviewItemId: queue[currentIndex].id,
      outcome,
    });
  };

  if (isLoading) return <div className={styles.loading}>Loading your review queue...</div>;

  if (!queue || queue.length === 0 || currentIndex >= queue.length) {
    return (
      <div className={styles.emptyState}>
        <div className={styles.checkIcon}>✓</div>
        <h2>All caught up!</h2>
        <p>You have no due reviews at the moment. Keep learning!</p>
      </div>
    );
  }

  const currentItem = queue[currentIndex];

  return (
    <div className={styles.reviewContainer}>
      <div className={styles.progressBar}>
        <div 
          className={styles.progressFill} 
          style={{ width: `${((currentIndex) / queue.length) * 100}%` }} 
        />
      </div>

      <div className={styles.card}>
        <div className={styles.content}>{currentItem.content}</div>
        
        {showMeaning ? (
          <div className={styles.meaning}>{currentItem.meaning}</div>
        ) : (
          <button 
            className={styles.revealBtn}
            onClick={() => setShowMeaning(true)}
          >
            Show Meaning
          </button>
        )}
      </div>

      {showMeaning && (
        <div className={styles.actions}>
          <Button 
            variant="ghost" 
            className={styles.outcomeBtn} 
            onClick={() => handleOutcome('again')}
            disabled={mutation.isPending}
          >
            Again
          </Button>
          <Button 
            variant="secondary" 
            className={styles.outcomeBtn} 
            onClick={() => handleOutcome('hard')}
            disabled={mutation.isPending}
          >
            Hard
          </Button>
          <Button 
            variant="primary" 
            className={styles.outcomeBtn} 
            onClick={() => handleOutcome('good')}
            disabled={mutation.isPending}
          >
            Good
          </Button>
          <Button 
            variant="primary" 
            className={styles.outcomeBtn} 
            onClick={() => handleOutcome('easy')}
            disabled={mutation.isPending}
          >
            Easy
          </Button>
        </div>
      )}
    </div>
  );
};
