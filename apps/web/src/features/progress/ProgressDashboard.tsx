"use client";

import React from "react";
import { useQuery } from "@tanstack/react-query";
import { getReadinessSnapshot } from "@/lib/api/progress";
import styles from "./progress.module.css";

export const ProgressDashboard: React.FC = () => {
  const { data: snapshot, isLoading } = useQuery({
    queryKey: ["readiness"],
    queryFn: getReadinessSnapshot,
  });

  if (isLoading)
    return <div className={styles.loading}>Analyzing your progress...</div>;
  if (!snapshot) return <div>No data available.</div>;

  return (
    <div className={styles.dashboard}>
      <div className={styles.topSection}>
        <div className={styles.readinessCard}>
          <div className={styles.scoreCircle}>
            <span className={styles.scoreNumber}>{snapshot.overallScore}</span>
            <span className={styles.scoreLabel}>Readiness</span>
          </div>
          <div className={styles.scoreMeta}>
            <span className={`${styles.trend} ${styles[snapshot.trend]}`}>
              {snapshot.trend.charAt(0).toUpperCase() + snapshot.trend.slice(1)}
            </span>
            <span className={styles.version}>Engine {snapshot.version}</span>
            <span className={styles.date}>
              Last updated: {new Date(snapshot.date).toLocaleDateString()}
            </span>
          </div>
        </div>
      </div>

      <div className={styles.matrixSection}>
        <h3>Capability Matrix</h3>
        <div className={styles.matrixGrid}>
          {snapshot.capabilities.map((cap, i) => (
            <div key={i} className={styles.capabilityCard}>
              <div className={styles.capHeader}>
                <span className={styles.capArea}>{cap.area}</span>
                <span className={styles.capScore}>{cap.score}%</span>
              </div>
              <div className={styles.capBar}>
                <div
                  className={styles.capFill}
                  style={{ width: `${cap.score}%` }}
                />
              </div>
              <p className={styles.capExplanation}>{cap.explanation}</p>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};
