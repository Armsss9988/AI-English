"use client";

import React from "react";
import { useQuery } from "@tanstack/react-query";
import { useParams } from "next/navigation";
import Link from "next/link";
import type { InterviewTurnDto, InterviewFeedbackResponse } from "@english-coach/contracts";
import { getInterviewSession } from "@/lib/api/interview";
import { InterviewFeedback } from "@/features/interview/InterviewFeedback";
import { ScorecardCard } from "@/features/interview/ScorecardCard";
import styles from "@/features/interview/interview.module.css";

function formatTimestamp(iso: string): string {
  return new Date(iso).toLocaleTimeString("en-US", {
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit",
  });
}

function TurnBubble({ turn }: { turn: InterviewTurnDto }) {
  const isInterviewer = turn.role === "interviewer";

  return (
    <div className={`${styles.message} ${styles[turn.role]}`}>
      <div className={styles.messageLabel}>
        {isInterviewer ? "🎙 Interviewer" : "🧑 You"}
        {turn.category && <span className={styles.categoryBadge}>{turn.category}</span>}
        {turn.turnType && <span className={styles.turnTypeBadge}>{turn.turnType}</span>}
        {turn.targetCapability && <span className={styles.capabilityBadge}>{turn.targetCapability}</span>}
        <span style={{ fontSize: "0.7rem", color: "#475569", marginLeft: "auto" }}>
          {formatTimestamp(turn.createdAt)}
        </span>
      </div>
      <div className={styles.messageBubble}>
        {turn.confirmedTranscript || turn.message}
        {turn.transcriptConfidence !== undefined && turn.transcriptConfidence > 0 && (
          <div className={styles.transcriptMeta}>
            🎯 Confidence: {Math.round(turn.transcriptConfidence * 100)}%
          </div>
        )}
        {turn.rawTranscript && turn.confirmedTranscript && turn.rawTranscript !== turn.confirmedTranscript && (
          <div style={{ fontSize: "0.75rem", color: "#64748b", marginTop: "6px", fontStyle: "italic" }}>
            Original transcript: {turn.rawTranscript}
          </div>
        )}
      </div>
      {isInterviewer && turn.coachingHint && (
        <div className={styles.coachingHint} style={{ marginTop: "6px" }}>
          💡 Hint: {turn.coachingHint}
        </div>
      )}
    </div>
  );
}

export default function InterviewSessionDetailPage() {
  const params = useParams();
  const sessionId = params.sessionId as string;

  const sessionQuery = useQuery({
    queryKey: ["interview-session", sessionId],
    queryFn: () => getInterviewSession(sessionId),
    enabled: !!sessionId,
  });

  if (sessionQuery.isLoading) {
    return (
      <div className={styles.loading}>
        <span className={styles.spinner} />
        Loading session...
      </div>
    );
  }

  if (sessionQuery.isError || !sessionQuery.data) {
    return (
      <div className={styles.setupContainer}>
        <div className={styles.errorAlert} role="alert">
          <div className={styles.errorTitle}>Session not found</div>
          <div>
            {sessionQuery.error instanceof Error
              ? sessionQuery.error.message
              : "Could not load session details."}
          </div>
        </div>
        <div className={styles.buttonRow}>
          <Link href="/interview/history" className={styles.secondaryBtn}>
            ← Back to History
          </Link>
        </div>
      </div>
    );
  }

  const session = sessionQuery.data;
  const showFeedback = session.feedback && session.status === "Completed";

  return (
    <div className={styles.setupContainer} style={{ maxWidth: "800px" }}>
      {/* Header */}
      <div
        style={{
          display: "flex",
          alignItems: "center",
          justifyContent: "space-between",
          flexWrap: "wrap",
          gap: "12px",
          marginBottom: "24px",
        }}
      >
        <div>
          <h1 className={styles.setupTitle} style={{ marginBottom: "4px", fontSize: "1.5rem" }}>
            {session.interviewMode === "TrainingInterview" ? "🎓" : "🎤"}{" "}
            {session.interviewType} Interview
          </h1>
          <div
            style={{
              fontSize: "0.85rem",
              color: "#64748b",
              display: "flex",
              gap: "12px",
              flexWrap: "wrap",
            }}
          >
            <span>
              {session.interviewMode === "TrainingInterview" ? "Training" : "Real"} mode
            </span>
            <span>
              {session.answeredCount}/{session.plannedQuestionCount} questions answered
            </span>
            <span
              style={{
                padding: "1px 8px",
                borderRadius: "100px",
                fontSize: "0.7rem",
                fontWeight: 600,
                textTransform: "uppercase",
                background:
                  session.status === "Completed"
                    ? "rgba(34, 197, 94, 0.15)"
                    : "rgba(59, 130, 246, 0.15)",
                color: session.status === "Completed" ? "#22c55e" : "#3b82f6",
              }}
            >
              {session.status}
            </span>
          </div>
        </div>
        <Link href="/interview/history" className={styles.secondaryBtn}>
          ← History
        </Link>
      </div>

      {/* Feedback summary at top if completed */}
      {showFeedback && session.feedback && (
        <InterviewFeedback
          feedback={session.feedback as InterviewFeedbackResponse}
          onBack={() => {}}
        />
      )}

      {/* Conversation replay */}
      <div style={{ marginTop: "24px" }}>
        <h2
          style={{
            fontSize: "1.1rem",
            fontWeight: 700,
            color: "#e2e8f0",
            marginBottom: "16px",
          }}
        >
          💬 Conversation Replay
        </h2>
        <div style={{ display: "flex", flexDirection: "column", gap: "4px" }}>
          {session.turns.map((turn) => (
            <TurnBubble key={turn.turnId} turn={turn} />
          ))}
        </div>

        {session.turns.length === 0 && (
          <div
            style={{
              textAlign: "center",
              padding: "32px",
              color: "#64748b",
              fontSize: "0.9rem",
            }}
          >
            No turns recorded in this session.
          </div>
        )}
      </div>

      {/* Bottom nav */}
      <div className={styles.buttonRow} style={{ marginTop: "32px" }}>
        <Link href="/interview/history" className={styles.secondaryBtn}>
          ← Back to History
        </Link>
        <Link href="/interview" className={styles.primaryBtn}>
          🎤 New Interview
        </Link>
      </div>
    </div>
  );
}
