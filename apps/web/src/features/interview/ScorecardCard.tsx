"use client";

import React, { useState } from "react";
import styles from "./interview.module.css";

interface ScorecardData {
  contentFitScore?: number;
  jdRelevanceScore?: number;
  cvEvidenceScore?: number;
  structureScore?: number;
  technicalCredibilityScore?: number;
  englishClarityScore?: number;
  professionalToneScore?: number;
  pronunciationClarityScore?: number;
  fluencyScore?: number;
  overallScore?: number;
  evidence?: string;
  missingEvidence?: string;
  betterAnswer?: string;
  corrections?: Array<{ original: string; corrected: string; explanationVi: string }>;
  retryDrillPrompt?: string;
  phraseCandidates?: string[];
  mistakeCandidates?: string[];
  requiresRetry?: boolean;
}

interface ScorecardCardProps {
  data: unknown;
  compact?: boolean;
}

function ScoreBar({ label, score, maxScore = 10 }: { label: string; score: number; maxScore?: number }) {
  const pct = Math.min(100, (score / maxScore) * 100);
  const color = pct >= 70 ? "#22c55e" : pct >= 40 ? "#f59e0b" : "#ef4444";

  return (
    <div style={{ marginBottom: "8px" }}>
      <div
        style={{
          display: "flex",
          justifyContent: "space-between",
          fontSize: "0.75rem",
          color: "#94a3b8",
          marginBottom: "3px",
        }}
      >
        <span>{label}</span>
        <span style={{ fontWeight: 600, color }}>
          {score}/{maxScore}
        </span>
      </div>
      <div
        style={{
          height: "6px",
          background: "rgba(148, 163, 184, 0.1)",
          borderRadius: "3px",
          overflow: "hidden",
        }}
      >
        <div
          style={{
            height: "100%",
            width: `${pct}%`,
            background: color,
            borderRadius: "3px",
            transition: "width 0.5s ease",
          }}
        />
      </div>
    </div>
  );
}

export const ScorecardCard: React.FC<ScorecardCardProps> = ({ data, compact = false }) => {
  const [expanded, setExpanded] = useState(!compact);

  if (!data || typeof data !== "object") {
    return null;
  }

  const scorecard = data as ScorecardData;
  const hasScores = scorecard.overallScore !== undefined;

  if (!hasScores) {
    return null;
  }

  const dimensions = [
    { label: "Content Fit", score: scorecard.contentFitScore },
    { label: "JD Relevance", score: scorecard.jdRelevanceScore },
    { label: "CV Evidence", score: scorecard.cvEvidenceScore },
    { label: "Structure", score: scorecard.structureScore },
    { label: "Technical Credibility", score: scorecard.technicalCredibilityScore },
    { label: "English Clarity", score: scorecard.englishClarityScore },
    { label: "Professional Tone", score: scorecard.professionalToneScore },
    { label: "Pronunciation", score: scorecard.pronunciationClarityScore },
    { label: "Fluency", score: scorecard.fluencyScore },
  ].filter((d) => d.score !== undefined && d.score !== null);

  const overallPct = Math.min(100, (scorecard.overallScore ?? 0));
  const overallColor = overallPct >= 70 ? "#22c55e" : overallPct >= 40 ? "#f59e0b" : "#ef4444";

  return (
    <div className={styles.scorecardInline}>
      <div
        style={{
          display: "flex",
          alignItems: "center",
          justifyContent: "space-between",
          marginBottom: expanded ? "12px" : 0,
          cursor: compact ? "pointer" : undefined,
        }}
        onClick={() => compact && setExpanded(!expanded)}
      >
        <div className={styles.scorecardTitle}>
          📊 Answer Scorecard
          {scorecard.requiresRetry && (
            <span
              style={{
                marginLeft: "8px",
                padding: "2px 8px",
                borderRadius: "100px",
                fontSize: "0.65rem",
                fontWeight: 600,
                background: "rgba(239, 68, 68, 0.15)",
                color: "#ef4444",
              }}
            >
              RETRY SUGGESTED
            </span>
          )}
        </div>
        <div
          style={{
            fontSize: "1.3rem",
            fontWeight: 800,
            color: overallColor,
          }}
        >
          {scorecard.overallScore}
          <span style={{ fontSize: "0.6rem", fontWeight: 400, color: "#64748b" }}>/100</span>
        </div>
      </div>

      {expanded && (
        <>
          {/* Score bars */}
          <div style={{ marginBottom: "16px" }}>
            {dimensions.map((d) => (
              <ScoreBar key={d.label} label={d.label} score={d.score!} />
            ))}
          </div>

          {/* Corrections */}
          {scorecard.corrections && scorecard.corrections.length > 0 && (
            <div style={{ marginBottom: "12px" }}>
              <div
                style={{
                  fontSize: "0.8rem",
                  fontWeight: 600,
                  color: "#e2e8f0",
                  marginBottom: "6px",
                }}
              >
                ✏️ Corrections
              </div>
              {scorecard.corrections.map((c, i) => (
                <div
                  key={i}
                  style={{
                    padding: "8px 12px",
                    background: "rgba(239, 68, 68, 0.06)",
                    borderRadius: "8px",
                    marginBottom: "6px",
                    fontSize: "0.8rem",
                  }}
                >
                  <div style={{ color: "#ef4444", textDecoration: "line-through" }}>
                    {c.original}
                  </div>
                  <div style={{ color: "#22c55e", fontWeight: 500 }}>→ {c.corrected}</div>
                  {c.explanationVi && (
                    <div style={{ color: "#94a3b8", fontStyle: "italic", marginTop: "2px" }}>
                      {c.explanationVi}
                    </div>
                  )}
                </div>
              ))}
            </div>
          )}

          {/* Better Answer */}
          {scorecard.betterAnswer && (
            <div style={{ marginBottom: "12px" }}>
              <div
                style={{
                  fontSize: "0.8rem",
                  fontWeight: 600,
                  color: "#e2e8f0",
                  marginBottom: "6px",
                }}
              >
                💡 Better Answer
              </div>
              <div
                style={{
                  padding: "10px 14px",
                  background: "rgba(34, 197, 94, 0.06)",
                  borderRadius: "8px",
                  fontSize: "0.8rem",
                  color: "#94a3b8",
                  lineHeight: 1.5,
                }}
              >
                {scorecard.betterAnswer}
              </div>
            </div>
          )}

          {/* Evidence */}
          {scorecard.evidence && (
            <div style={{ marginBottom: "12px" }}>
              <div style={{ fontSize: "0.8rem", fontWeight: 600, color: "#e2e8f0", marginBottom: "4px" }}>
                ✅ Evidence Used
              </div>
              <div style={{ fontSize: "0.78rem", color: "#94a3b8" }}>{scorecard.evidence}</div>
            </div>
          )}

          {scorecard.missingEvidence && (
            <div style={{ marginBottom: "12px" }}>
              <div style={{ fontSize: "0.8rem", fontWeight: 600, color: "#e2e8f0", marginBottom: "4px" }}>
                ❌ Missing Evidence
              </div>
              <div style={{ fontSize: "0.78rem", color: "#94a3b8" }}>{scorecard.missingEvidence}</div>
            </div>
          )}

          {/* Phrase candidates */}
          {scorecard.phraseCandidates && scorecard.phraseCandidates.length > 0 && (
            <div style={{ marginBottom: "8px" }}>
              <div style={{ fontSize: "0.8rem", fontWeight: 600, color: "#e2e8f0", marginBottom: "6px" }}>
                💬 Suggested Phrases
              </div>
              <div style={{ display: "flex", flexWrap: "wrap", gap: "6px" }}>
                {scorecard.phraseCandidates.map((p, i) => (
                  <span
                    key={i}
                    style={{
                      padding: "3px 10px",
                      borderRadius: "100px",
                      fontSize: "0.72rem",
                      background: "rgba(99, 102, 241, 0.1)",
                      color: "#a5b4fc",
                    }}
                  >
                    &quot;{p}&quot;
                  </span>
                ))}
              </div>
            </div>
          )}

          {/* Retry drill */}
          {scorecard.retryDrillPrompt && (
            <div
              style={{
                marginTop: "12px",
                padding: "10px 14px",
                background: "rgba(245, 158, 11, 0.08)",
                borderRadius: "8px",
                border: "1px solid rgba(245, 158, 11, 0.2)",
              }}
            >
              <div style={{ fontSize: "0.8rem", fontWeight: 600, color: "#f59e0b", marginBottom: "4px" }}>
                🔄 Retry Drill
              </div>
              <div style={{ fontSize: "0.78rem", color: "#94a3b8" }}>
                {scorecard.retryDrillPrompt}
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
};
