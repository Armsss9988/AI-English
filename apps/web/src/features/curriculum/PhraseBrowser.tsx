"use client";

import React from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Button } from "@english-coach/ui";
import { getPhrases } from "@/lib/api/curriculum";
import { ensureReviewItem } from "@/lib/api/review";
import styles from "./curriculum.module.css";

export const PhraseBrowser: React.FC = () => {
  const queryClient = useQueryClient();
  const { data: phrases, isLoading } = useQuery({
    queryKey: ["phrases"],
    queryFn: () => getPhrases(),
  });
  const reviewMutation = useMutation({
    mutationFn: ensureReviewItem,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["due-reviews"] });
    },
  });

  if (isLoading) {
    return <div className={styles.loading}>Loading phrases...</div>;
  }

  return (
    <div className={styles.phraseGrid}>
      {phrases?.map((phrase) => (
        <div key={phrase.id} className={styles.phraseCard}>
          <span className={styles.functionBadge}>{phrase.function}</span>
          <div className={styles.phraseContent}>{phrase.content}</div>
          <div className={styles.phraseMeaning}>{phrase.meaning}</div>
          <div className={styles.phraseActions}>
            <Button
              size="sm"
              variant="secondary"
              isLoading={
                reviewMutation.isPending &&
                reviewMutation.variables?.itemId === phrase.id
              }
              onClick={() =>
                reviewMutation.mutate({
                  itemId: phrase.id,
                  reviewTrack: "phrase",
                  displayText: phrase.content,
                  displaySubtitle: phrase.meaning,
                })
              }
            >
              Add to review
            </Button>
          </div>
        </div>
      ))}
    </div>
  );
};
