"use client";

import React from "react";
import { useQuery } from "@tanstack/react-query";
import { getScenarios } from "@/lib/api/curriculum";
import styles from "./curriculum.module.css";

export const ScenarioBrowser: React.FC = () => {
  const { data: scenarios, isLoading } = useQuery({
    queryKey: ["scenarios"],
    queryFn: getScenarios,
  });

  if (isLoading) {
    return <div className={styles.loading}>Loading scenarios...</div>;
  }

  return (
    <div className={styles.scenarioGrid}>
      {scenarios?.map((scenario) => (
        <div key={scenario.id} className={styles.scenarioCard}>
          <div className={styles.scenarioHeader}>
            <h3 className={styles.scenarioTitle}>{scenario.title}</h3>
            <span
              className={`${styles.difficultyBadge} ${styles[scenario.difficulty]}`}
            >
              {scenario.difficulty}
            </span>
          </div>
          <div className={styles.scenarioBody}>
            <div className={styles.infoRow}>
              <span className={styles.infoLabel}>Goal</span>
              <span className={styles.infoValue}>{scenario.goal}</span>
            </div>
            <div className={styles.infoRow}>
              <span className={styles.infoLabel}>Persona</span>
              <span className={styles.infoValue}>{scenario.persona}</span>
            </div>
          </div>
        </div>
      ))}
    </div>
  );
};
