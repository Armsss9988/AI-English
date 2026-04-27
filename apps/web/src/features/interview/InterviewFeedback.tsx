"use client";

import React, { useState } from "react";
import { InterviewFeedbackResponse } from "@english-coach/contracts";
import styles from "./interview.module.css";

interface InterviewFeedbackProps {
  feedback: InterviewFeedbackResponse;
  onBack: () => void;
}

function scoreClass(score: number): string {
  if (score >= 75) return styles.scoreHigh;
  if (score >= 50) return styles.scoreMedium;
  return styles.scoreLow;
}

export const InterviewFeedback: React.FC<InterviewFeedbackProps> = ({
  feedback,
  onBack,
}) => {
  const [lang, setLang] = useState<"en" | "vi">("en");

  return (
    <div className={styles.feedbackContainer}>
      <div className={styles.feedbackHeader}>
        <div className={styles.feedbackTitle}>Interview Complete 🎯</div>
        <div className={styles.overallScore}>{feedback.overallScore}/100</div>
      </div>

      <div className={styles.scoreGrid}>
        <div className={styles.scoreCard}>
          <div className={`${styles.scoreValue} ${scoreClass(feedback.communicationScore)}`}>
            {feedback.communicationScore}
          </div>
          <div className={styles.scoreLabel}>Communication</div>
        </div>
        <div className={styles.scoreCard}>
          <div className={`${styles.scoreValue} ${scoreClass(feedback.technicalAccuracyScore)}`}>
            {feedback.technicalAccuracyScore}
          </div>
          <div className={styles.scoreLabel}>Technical Accuracy</div>
        </div>
        <div className={styles.scoreCard}>
          <div className={`${styles.scoreValue} ${scoreClass(feedback.confidenceScore)}`}>
            {feedback.confidenceScore}
          </div>
          <div className={styles.scoreLabel}>Confidence</div>
        </div>
      </div>

      <div className={styles.feedbackSection}>
        <div className={styles.feedbackSectionTitle}>📝 Detailed Feedback</div>
        <div className={styles.langTabs}>
          <button
            className={`${styles.langTab} ${lang === "en" ? styles.active : ""}`}
            onClick={() => setLang("en")}
          >
            🇬🇧 English
          </button>
          <button
            className={`${styles.langTab} ${lang === "vi" ? styles.active : ""}`}
            onClick={() => setLang("vi")}
          >
            🇻🇳 Tiếng Việt
          </button>
        </div>
        <div className={styles.feedbackText}>
          {lang === "en" ? feedback.detailedFeedbackEn : feedback.detailedFeedbackVi}
        </div>
      </div>

      <div className={styles.feedbackSection}>
        <div className={styles.feedbackSectionTitle}>💪 Strengths</div>
        <div className={styles.tagList}>
          {feedback.strengthAreas.map((s, i) => (
            <span key={i} className={styles.tagStrength}>{s}</span>
          ))}
        </div>
      </div>

      <div className={styles.feedbackSection}>
        <div className={styles.feedbackSectionTitle}>📈 Areas to Improve</div>
        <div className={styles.tagList}>
          {feedback.improvementAreas.map((s, i) => (
            <span key={i} className={styles.tagImprovement}>{s}</span>
          ))}
        </div>
      </div>

      <div className={styles.feedbackSection}>
        <div className={styles.feedbackSectionTitle}>💬 Suggested Phrases</div>
        <div className={styles.tagList}>
          {feedback.suggestedPhrases.map((s, i) => (
            <span key={i} className={styles.tagPhrase}>
              &quot;{s}&quot;
            </span>
          ))}
        </div>
      </div>

      <div className={styles.feedbackSection}>
        <div className={styles.feedbackSectionTitle}>🔄 Retry Recommendation</div>
        <div className={styles.feedbackText}>{feedback.retryRecommendation}</div>
      </div>

      <div className={styles.buttonRow}>
        <button className={styles.secondaryBtn} onClick={onBack}>
          ← Back to Setup
        </button>
        <button
          className={styles.primaryBtn}
          onClick={() => window.location.reload()}
        >
          🚀 Practice Again
        </button>
      </div>
    </div>
  );
};
