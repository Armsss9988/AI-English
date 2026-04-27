"use client";

import React from "react";
import { useQuery } from "@tanstack/react-query";
import Link from "next/link";
import { getInterviewHistory } from "@/lib/api/interview";
import styles from "@/features/interview/interview.module.css";

function formatDate(iso: string): string {
  const d = new Date(iso);
  return d.toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

function scoreColor(score?: number): string {
  if (!score) return "#64748b";
  if (score >= 75) return "#22c55e";
  if (score >= 50) return "#f59e0b";
  return "#ef4444";
}

function statusBadge(status: string) {
  const colors: Record<string, string> = {
    completed: "#22c55e",
    active: "#3b82f6",
    created: "#94a3b8",
    finalizing: "#f59e0b",
  };
  return (
    <span
      style={{
        padding: "2px 10px",
        borderRadius: "100px",
        fontSize: "0.7rem",
        fontWeight: 600,
        textTransform: "uppercase",
        letterSpacing: "0.04em",
        background: `${colors[status.toLowerCase()] ?? "#6b7280"}20`,
        color: colors[status.toLowerCase()] ?? "#6b7280",
      }}
    >
      {status}
    </span>
  );
}

export default function InterviewHistoryPage() {
  const historyQuery = useQuery({
    queryKey: ["interview-history"],
    queryFn: getInterviewHistory,
  });

  return (
    <div className={styles.setupContainer}>
      <div
        style={{
          display: "flex",
          alignItems: "center",
          justifyContent: "space-between",
          flexWrap: "wrap",
          gap: "12px",
          marginBottom: "32px",
        }}
      >
        <div>
          <h1 className={styles.setupTitle} style={{ marginBottom: "4px" }}>
            📋 Interview History
          </h1>
          <p className={styles.setupSubtitle}>
            Review past sessions and track your improvement
          </p>
        </div>
        <Link href="/interview" className={styles.primaryBtn}>
          🎤 New Interview
        </Link>
      </div>

      {historyQuery.isLoading && (
        <div className={styles.loading}>
          <span className={styles.spinner} />
          Loading history...
        </div>
      )}

      {historyQuery.isError && (
        <div className={styles.errorAlert} role="alert">
          <div className={styles.errorTitle}>Failed to load history</div>
          <div>
            {historyQuery.error instanceof Error
              ? historyQuery.error.message
              : "Please try again."}
          </div>
        </div>
      )}

      {historyQuery.data?.sessions?.length === 0 && (
        <div
          style={{
            textAlign: "center",
            padding: "64px 24px",
            color: "#64748b",
            background: "rgba(30, 41, 59, 0.3)",
            borderRadius: "16px",
            border: "1px dashed rgba(148, 163, 184, 0.15)",
          }}
        >
          <div style={{ fontSize: "3rem", marginBottom: "16px" }}>🎤</div>
          <div style={{ fontSize: "1.1rem", fontWeight: 600, color: "#94a3b8", marginBottom: "8px" }}>
            No interviews yet
          </div>
          <div style={{ marginBottom: "20px" }}>
            Start your first mock interview to practice for real opportunities.
          </div>
          <Link href="/interview" className={styles.primaryBtn}>
            Start First Interview →
          </Link>
        </div>
      )}

      {historyQuery.data?.sessions && historyQuery.data.sessions.length > 0 && (
        <div style={{ display: "flex", flexDirection: "column", gap: "8px" }}>
          {historyQuery.data.sessions.map((session) => (
            <Link
              key={session.sessionId}
              href={`/interview/sessions/${session.sessionId}`}
              style={{
                display: "flex",
                alignItems: "center",
                justifyContent: "space-between",
                gap: "16px",
                padding: "16px 20px",
                background: "rgba(30, 41, 59, 0.4)",
                border: "1px solid rgba(148, 163, 184, 0.08)",
                borderRadius: "12px",
                textDecoration: "none",
                color: "inherit",
                transition: "border-color 0.2s ease, transform 0.1s ease",
              }}
            >
              <div style={{ flex: 1, minWidth: 0 }}>
                <div
                  style={{
                    display: "flex",
                    alignItems: "center",
                    gap: "8px",
                    marginBottom: "4px",
                    flexWrap: "wrap",
                  }}
                >
                  <span style={{ fontSize: "0.95rem", fontWeight: 600, color: "#e2e8f0" }}>
                    {session.interviewMode === "TrainingInterview" ? "🎓" : "🎤"}{" "}
                    {session.interviewType} Interview
                  </span>
                  {statusBadge(session.status)}
                </div>
                <div
                  style={{
                    fontSize: "0.8rem",
                    color: "#64748b",
                    display: "flex",
                    gap: "12px",
                    flexWrap: "wrap",
                  }}
                >
                  <span>{formatDate(session.createdAt)}</span>
                  <span>
                    {session.answeredCount}/{session.plannedQuestionCount} questions
                  </span>
                  <span>
                    {session.interviewMode === "TrainingInterview" ? "Training" : "Real"}
                  </span>
                </div>
              </div>

              <div style={{ display: "flex", alignItems: "center", gap: "12px", flexShrink: 0 }}>
                {session.overallScore !== undefined && session.overallScore !== null && (
                  <div
                    style={{
                      fontSize: "1.5rem",
                      fontWeight: 800,
                      color: scoreColor(session.overallScore),
                    }}
                  >
                    {session.overallScore}
                    <span style={{ fontSize: "0.7rem", fontWeight: 400, color: "#64748b" }}>
                      /100
                    </span>
                  </div>
                )}
                <span style={{ color: "#475569", fontSize: "1.2rem" }}>→</span>
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
