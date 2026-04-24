"use client";

import React from "react";
import { UserProfile } from "@english-coach/contracts";
import styles from "./onboarding.module.css";

interface ProfileSummaryProps {
  profile: UserProfile;
}

export const ProfileSummary: React.FC<ProfileSummaryProps> = ({ profile }) => {
  return (
    <div className={styles.formCard}>
      <div className={styles.header}>
        <h1 className={styles.title}>Profile Summary</h1>
        <p className={styles.subtitle}>Your learning path is ready</p>
      </div>

      <div className={styles.summaryList}>
        <div className={styles.summaryItem}>
          <span className={styles.itemLabel}>Role</span>
          <span className={styles.itemValue}>{profile.role}</span>
        </div>
        <div className={styles.summaryItem}>
          <span className={styles.itemLabel}>Level</span>
          <span className={styles.itemValue}>{profile.currentLevel}</span>
        </div>
        <div className={styles.summaryItem}>
          <span className={styles.itemLabel}>Timezone</span>
          <span className={styles.itemValue}>{profile.timezone}</span>
        </div>
        <div className={styles.summaryItem}>
          <span className={styles.itemLabel}>Target Use Case</span>
          <span className={styles.itemValue}>{profile.targetUseCase}</span>
        </div>
      </div>
    </div>
  );
};
