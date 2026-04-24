"use client";

import React, { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { NotebookCategory } from "@english-coach/contracts";
import { getNotebookEntries } from "@/lib/api/error-notebook";
import styles from "./notebook.module.css";

const CATEGORIES: NotebookCategory[] = [
  "Grammar",
  "Vocabulary",
  "Pronunciation",
  "Business Context",
];

export const NotebookList: React.FC = () => {
  const [selectedCategory, setSelectedCategory] = useState<
    NotebookCategory | "All"
  >("All");

  const { data: entries, isLoading } = useQuery({
    queryKey: ["notebook", selectedCategory],
    queryFn: () =>
      getNotebookEntries(
        selectedCategory === "All" ? undefined : selectedCategory
      ),
  });

  if (isLoading)
    return <div className={styles.loading}>Loading your notebook...</div>;

  return (
    <div className={styles.container}>
      <div className={styles.filterBar}>
        <button
          className={`${styles.filterBtn} ${selectedCategory === "All" ? styles.filterActive : ""}`}
          onClick={() => setSelectedCategory("All")}
        >
          All
        </button>
        {CATEGORIES.map((cat) => (
          <button
            key={cat}
            className={`${styles.filterBtn} ${selectedCategory === cat ? styles.filterActive : ""}`}
            onClick={() => setSelectedCategory(cat)}
          >
            {cat}
          </button>
        ))}
      </div>

      <div className={styles.list}>
        {entries?.length === 0 ? (
          <div className={styles.empty}>No entries found in this category.</div>
        ) : (
          entries?.map((entry) => (
            <div key={entry.id} className={styles.entryCard}>
              <div className={styles.entryHeader}>
                <span className={styles.categoryBadge}>{entry.category}</span>
                <span className={styles.recurrence}>
                  Seen {entry.recurrenceCount} times
                </span>
              </div>
              <h3 className={styles.pattern}>{entry.pattern}</h3>

              <div className={styles.comparison}>
                <div className={styles.originalLine}>
                  <span className={styles.lineLabel}>Original:</span>
                  <span className={styles.lineText}>{entry.original}</span>
                </div>
                <div className={styles.correctedLine}>
                  <span className={styles.lineLabel}>Correction:</span>
                  <span className={styles.lineText}>{entry.corrected}</span>
                </div>
              </div>

              <div className={styles.explanationBox}>
                <p>{entry.explanation}</p>
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
};
