'use client';

import React from 'react';
import { useQuery } from '@tanstack/react-query';
import { getPhrases } from '@/lib/api/curriculum';
import styles from './curriculum.module.css';

export const PhraseBrowser: React.FC = () => {
  const { data: phrases, isLoading } = useQuery({
    queryKey: ['phrases'],
    queryFn: getPhrases,
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
        </div>
      ))}
    </div>
  );
};
