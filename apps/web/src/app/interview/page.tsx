"use client";

import React, { useState } from "react";
import Link from "next/link";
import { InterviewSetup } from "@/features/interview/InterviewSetup";
import { InterviewChat } from "@/features/interview/InterviewChat";

type PageState =
  | { phase: "setup" }
  | { phase: "interview"; profileId: string; jdText: string; interviewType: string; interviewMode: string };

export default function InterviewPage() {
  const [state, setState] = useState<PageState>({ phase: "setup" });

  if (state.phase === "interview") {
    return (
      <InterviewChat
        profileId={state.profileId}
        jdText={state.jdText}
        interviewType={state.interviewType}
        interviewMode={state.interviewMode}
        onBack={() => setState({ phase: "setup" })}
      />
    );
  }

  return (
    <>
      <div
        style={{
          maxWidth: "720px",
          margin: "0 auto",
          padding: "0 24px",
          display: "flex",
          justifyContent: "flex-end",
          marginBottom: "-32px",
          paddingTop: "16px",
        }}
      >
        <Link
          href="/interview/history"
          style={{
            padding: "6px 14px",
            borderRadius: "8px",
            border: "1px solid rgba(148, 163, 184, 0.15)",
            background: "rgba(30, 41, 59, 0.3)",
            color: "#94a3b8",
            fontSize: "0.82rem",
            textDecoration: "none",
            transition: "all 0.2s ease",
          }}
        >
          📋 View History
        </Link>
      </div>
      <InterviewSetup
        onReady={(profileId, jdText, interviewType, interviewMode) =>
          setState({ phase: "interview", profileId, jdText, interviewType, interviewMode })
        }
      />
    </>
  );
}
