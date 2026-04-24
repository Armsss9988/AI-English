'use client';

import React, { useState } from 'react';
import { useQuery, useMutation } from '@tanstack/react-query';
import { Button } from '@english-coach/ui';
import { getDrillPrompt, submitAttempt } from '@/lib/api/speaking';
import styles from './speaking.module.css';

export const SpeakingDrill: React.FC<{ drillId: string }> = ({ drillId }) => {
  const [transcript, setTranscript] = useState('');
  const { data: drill, isLoading: isLoadingPrompt } = useQuery({
    queryKey: ['drill', drillId],
    queryFn: () => getDrillPrompt(drillId),
  });

  const mutation = useMutation({
    mutationFn: submitAttempt,
  });

  if (isLoadingPrompt) return <div className={styles.loading}>Loading drill...</div>;
  if (!drill) return <div>Drill not found.</div>;

  const attempt = mutation.data;

  if (mutation.isSuccess && attempt?.feedback) {
    return (
      <div className={styles.feedbackContainer}>
        <div className={styles.feedbackHeader}>
          <div className={styles.scoreCircle}>
            <span className={styles.score}>{attempt.feedback.overallScore}</span>
            <span className={styles.scoreLabel}>Score</span>
          </div>
          <div className={styles.overallComments}>
            <h3>AI Feedback</h3>
            <p>{attempt.feedback.overallComments}</p>
          </div>
        </div>

        <div className={styles.feedbackSection}>
          <h4>Mistakes & Corrections</h4>
          <div className={styles.mistakeList}>
            {attempt.feedback.mistakes.map((m, i) => (
              <div key={i} className={styles.mistakeItem}>
                <div className={styles.original}>{m.original}</div>
                <div className={styles.correction}>→ {m.correction}</div>
                <div className={styles.explanation}>{m.explanation}</div>
              </div>
            ))}
          </div>
        </div>

        <div className={styles.feedbackSection}>
          <h4>Recommended Answer</h4>
          <div className={styles.improvedAnswer}>{attempt.feedback.improvedAnswer}</div>
        </div>

        <Button onClick={() => mutation.reset()} className={styles.retryBtn}>
          Try Again
        </Button>
      </div>
    );
  }

  return (
    <div className={styles.drillCard}>
      <div className={styles.contextBox}>
        <span className={styles.label}>Context</span>
        <p>{drill.context}</p>
      </div>

      <div className={styles.promptBox}>
        <span className={styles.label}>Prompt</span>
        <h3>{drill.prompt}</h3>
      </div>

      {drill.suggestedPhrases && (
        <div className={styles.phrasesBox}>
          <span className={styles.label}>Suggested Phrases</span>
          <div className={styles.phraseBadges}>
            {drill.suggestedPhrases.map(p => <span key={p} className={styles.badge}>{p}</span>)}
          </div>
        </div>
      )}

      <div className={styles.inputArea}>
        <textarea
          className={styles.textarea}
          placeholder="Type your response here (MVP text-first mode)..."
          value={transcript}
          onChange={(e) => setTranscript(e.target.value)}
          disabled={mutation.isPending}
        />
        <Button
          size="lg"
          onClick={() => mutation.mutate({ drillId, transcript })}
          isLoading={mutation.isPending}
          disabled={!transcript.trim()}
          className={styles.submitBtn}
        >
          Submit for Evaluation
        </Button>
      </div>
    </div>
  );
};
