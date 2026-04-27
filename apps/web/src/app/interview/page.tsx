"use client";

import React, { useState } from "react";
import { InterviewSetup } from "@/features/interview/InterviewSetup";
import { InterviewChat } from "@/features/interview/InterviewChat";

type PageState =
  | { phase: "setup" }
  | { phase: "interview"; profileId: string; jdText: string; interviewType: string };

export default function InterviewPage() {
  const [state, setState] = useState<PageState>({ phase: "setup" });

  if (state.phase === "interview") {
    return (
      <InterviewChat
        profileId={state.profileId}
        jdText={state.jdText}
        interviewType={state.interviewType}
        onBack={() => setState({ phase: "setup" })}
      />
    );
  }

  return (
    <InterviewSetup
      onReady={(profileId, jdText, interviewType) =>
        setState({ phase: "interview", profileId, jdText, interviewType })
      }
    />
  );
}
