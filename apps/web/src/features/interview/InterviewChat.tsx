"use client";

import React, { useState, useEffect, useRef, useCallback } from "react";
import { useMutation } from "@tanstack/react-query";
import type {
  StartInterviewResponse,
  InterviewFeedbackResponse,
  AnswerQuestionResponse,
  TranscriptResponse,
} from "@english-coach/contracts";
import {
  startInterview,
  answerQuestion,
  finalizeInterview,
  uploadLearnerAudio,
  confirmTranscript,
  evaluateAnswer,
  generateNextTurn,
} from "@/lib/api/interview";
import { InterviewFeedback } from "./InterviewFeedback";
import { ScorecardCard } from "./ScorecardCard";
import styles from "./interview.module.css";

interface InterviewChatProps {
  profileId: string;
  jdText: string;
  interviewType: string;
  interviewMode: string;
  onBack: () => void;
}

interface TurnDisplay {
  id: string;
  role: "interviewer" | "learner";
  content: string;
  category?: string;
  turnType?: string;
  capability?: string;
  hint?: string;
  audioUrl?: string;
  transcript?: string;
  confidence?: number;
  scorecard?: unknown;
  turnState?: string;
}

type VoicePhase =
  | "idle"
  | "recording"
  | "transcribing"
  | "confirming"
  | "evaluating";

function formatElapsed(seconds: number): string {
  const m = Math.floor(seconds / 60);
  const s = seconds % 60;
  return `${m.toString().padStart(2, "0")}:${s.toString().padStart(2, "0")}`;
}

export const InterviewChat: React.FC<InterviewChatProps> = ({
  profileId,
  jdText,
  interviewType,
  interviewMode,
  onBack,
}) => {
  const [sessionId, setSessionId] = useState<string | null>(null);
  const [turns, setTurns] = useState<TurnDisplay[]>([]);
  const [input, setInput] = useState("");
  const [totalQuestions, setTotalQuestions] = useState(0);
  const [answeredCount, setAnsweredCount] = useState(0);
  const [isComplete, setIsComplete] = useState(false);
  const [feedback, setFeedback] = useState<InterviewFeedbackResponse | null>(null);
  const [currentHint, setCurrentHint] = useState<string | null>(null);
  const [voicePhase, setVoicePhase] = useState<VoicePhase>("idle");
  const [pendingTranscript, setPendingTranscript] = useState<TranscriptResponse | null>(null);
  const [editedTranscript, setEditedTranscript] = useState("");
  const [scorecardResult, setScorecardResult] = useState<unknown>(null);
  const [recordingDuration, setRecordingDuration] = useState(0);
  const [answerError, setAnswerError] = useState<string | null>(null);
  // Interview timer
  const [elapsedSeconds, setElapsedSeconds] = useState(0);
  const scrollRef = useRef<HTMLDivElement>(null);
  const mediaRecorderRef = useRef<MediaRecorder | null>(null);
  const chunksRef = useRef<Blob[]>([]);
  const recordingStartRef = useRef<number>(0);
  const timerRef = useRef<ReturnType<typeof setInterval> | null>(null);
  const interviewTimerRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const isTraining = interviewMode === "TrainingInterview";

  // ======== Interview Timer ========
  useEffect(() => {
    if (sessionId && !isComplete && !feedback) {
      interviewTimerRef.current = setInterval(() => {
        setElapsedSeconds((s) => s + 1);
      }, 1000);
    }
    return () => {
      if (interviewTimerRef.current) clearInterval(interviewTimerRef.current);
    };
  }, [sessionId, isComplete, feedback]);

  // ======== Keyboard Shortcuts ========
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      // Space to toggle recording (when not typing)
      if (
        e.code === "Space" &&
        !e.shiftKey &&
        !e.ctrlKey &&
        document.activeElement?.tagName !== "TEXTAREA" &&
        document.activeElement?.tagName !== "INPUT"
      ) {
        e.preventDefault();
        if (voicePhase === "idle") {
          startRecording();
        } else if (voicePhase === "recording") {
          stopRecording();
        }
      }
      // Enter to confirm transcript
      if (e.code === "Enter" && !e.shiftKey && voicePhase === "confirming") {
        if (document.activeElement?.tagName !== "TEXTAREA") {
          e.preventDefault();
          handleConfirmTranscript();
        }
      }
      // Escape to cancel transcript
      if (e.code === "Escape" && voicePhase === "confirming") {
        handleCancelTranscript();
      }
    };

    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [voicePhase]);

  // Start session
  const startMutation = useMutation({
    mutationFn: () =>
      startInterview({
        profileId,
        jdText,
        interviewType: interviewType as "Mixed" | "Behavioral" | "Technical" | "Situational",
      }),
    onSuccess: (res: StartInterviewResponse) => {
      setSessionId(res.sessionId);
      setTotalQuestions(res.plannedQuestionCount);
      setCurrentHint(res.coachingHint || null);
      setTurns([
        {
          id: Date.now().toString(),
          role: "interviewer",
          content: res.firstQuestion,
          category: res.questionCategory,
          turnType: res.turnType || undefined,
          capability: res.targetCapability || undefined,
          hint: res.coachingHint || undefined,
          audioUrl: res.audioUrl || undefined,
        },
      ]);
    },
  });

  // Text-based answer
  const answerMutation = useMutation({
    mutationFn: ({ sid, answer }: { sid: string; answer: string }) =>
      answerQuestion(sid, { answer }),
    onSuccess: (res: AnswerQuestionResponse) => {
      setAnswerError(null);
      setAnsweredCount(res.answeredCount);
      setTotalQuestions(res.totalQuestions);
      if (res.isInterviewComplete) {
        setIsComplete(true);
      } else if (res.nextQuestion) {
        setCurrentHint(res.coachingHint || null);
        setTurns((prev) => [
          ...prev,
          {
            id: Date.now().toString(),
            role: "interviewer",
            content: res.nextQuestion!,
            category: res.questionCategory || undefined,
            turnType: res.turnType || undefined,
            capability: res.targetCapability || undefined,
            hint: res.coachingHint || undefined,
            audioUrl: res.audioUrl || undefined,
          },
        ]);
      }
    },
    onError: (err: Error) => {
      setAnswerError(err.message || "Failed to process your answer. Please try again.");
    },
  });

  // Finalize
  const finalizeMutation = useMutation({
    mutationFn: (sid: string) => finalizeInterview(sid),
    onSuccess: (res) => setFeedback(res),
    onError: (err: Error) => {
      setAnswerError(`Finalization failed: ${err.message}. Try ending the interview again.`);
      setIsComplete(false);
    },
  });

  useEffect(() => {
    startMutation.mutate();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    if (scrollRef.current) {
      scrollRef.current.scrollTop = scrollRef.current.scrollHeight;
    }
  }, [turns, voicePhase, scorecardResult]);

  useEffect(() => {
    if (isComplete && sessionId) {
      finalizeMutation.mutate(sessionId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isComplete]);

  // ======== Text answer ========
  const handleSend = useCallback(() => {
    if (!input.trim() || !sessionId) return;
    const currentInput = input;
    setInput("");
    setAnswerError(null);
    setTurns((prev) => [...prev, {
      id: Date.now().toString(), role: "learner", content: currentInput,
    }]);
    answerMutation.mutate({ sid: sessionId, answer: currentInput });
  }, [input, sessionId, answerMutation]);

  // ======== Voice recording ========
  const startRecording = useCallback(async () => {
    // Check browser compatibility
    if (!navigator.mediaDevices?.getUserMedia) {
      setAnswerError("Voice recording is not supported in this browser. Please type your answer instead.");
      return;
    }

    try {
      const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
      const mimeType = MediaRecorder.isTypeSupported("audio/webm;codecs=opus")
        ? "audio/webm;codecs=opus"
        : MediaRecorder.isTypeSupported("audio/webm")
          ? "audio/webm"
          : "";

      if (!mimeType) {
        stream.getTracks().forEach((t) => t.stop());
        setAnswerError("Audio recording format not supported. Please type your answer.");
        return;
      }

      const mediaRecorder = new MediaRecorder(stream, { mimeType });
      mediaRecorderRef.current = mediaRecorder;
      chunksRef.current = [];
      recordingStartRef.current = Date.now();
      setRecordingDuration(0);
      setAnswerError(null);

      timerRef.current = setInterval(() => {
        setRecordingDuration(Math.floor((Date.now() - recordingStartRef.current) / 1000));
      }, 1000);

      mediaRecorder.ondataavailable = (e) => {
        if (e.data.size > 0) chunksRef.current.push(e.data);
      };

      mediaRecorder.onstop = async () => {
        stream.getTracks().forEach((t) => t.stop());
        if (timerRef.current) clearInterval(timerRef.current);

        const audioBlob = new Blob(chunksRef.current, { type: mimeType });
        const durationMs = Date.now() - recordingStartRef.current;

        if (!sessionId || audioBlob.size < 100) {
          setVoicePhase("idle");
          return;
        }

        setVoicePhase("transcribing");
        try {
          const result = await uploadLearnerAudio(sessionId, audioBlob, durationMs);
          setPendingTranscript(result);
          setEditedTranscript(result.rawTranscript);
          setVoicePhase("confirming");
        } catch (err) {
          setVoicePhase("idle");
          setAnswerError(
            err instanceof Error
              ? `Transcription failed: ${err.message}`
              : "Failed to transcribe audio. Try again or type your answer."
          );
        }
      };

      mediaRecorder.start(500);
      setVoicePhase("recording");
    } catch {
      setAnswerError("Could not access microphone. Please check permissions or type your answer.");
    }
  }, [sessionId]);

  const stopRecording = useCallback(() => {
    if (mediaRecorderRef.current && mediaRecorderRef.current.state !== "inactive") {
      mediaRecorderRef.current.stop();
    }
  }, []);

  // ======== Confirm transcript ========
  const handleConfirmTranscript = useCallback(async () => {
    if (!sessionId || !pendingTranscript) return;
    const wasEdited = editedTranscript !== pendingTranscript.rawTranscript;
    setVoicePhase("evaluating");
    setAnswerError(null);

    try {
      setTurns((prev) => [...prev, {
        id: pendingTranscript.turnId,
        role: "learner",
        content: editedTranscript,
        transcript: editedTranscript,
        confidence: pendingTranscript.confidence,
        turnState: "Confirmed",
      }]);

      await confirmTranscript(sessionId, pendingTranscript.turnId, {
        confirmedTranscript: editedTranscript,
        learnerEdited: wasEdited,
      });

      const evalResult = await evaluateAnswer(sessionId, pendingTranscript.turnId);
      setScorecardResult(evalResult);

      const nextTurnRes = await generateNextTurn(sessionId);
      setAnsweredCount(nextTurnRes.answeredCount);
      setTotalQuestions(nextTurnRes.totalQuestions);

      if (nextTurnRes.isInterviewComplete) {
        setIsComplete(true);
      } else if (nextTurnRes.nextQuestion) {
        setCurrentHint(nextTurnRes.coachingHint || null);
        setTurns((prev) => [...prev, {
          id: Date.now().toString(),
          role: "interviewer",
          content: nextTurnRes.nextQuestion!,
          category: nextTurnRes.questionCategory || undefined,
          turnType: nextTurnRes.turnType || undefined,
          capability: nextTurnRes.targetCapability || undefined,
          hint: nextTurnRes.coachingHint || undefined,
          audioUrl: nextTurnRes.audioUrl || undefined,
        }]);
      }
    } catch (err) {
      setAnswerError(
        err instanceof Error
          ? `Evaluation failed: ${err.message}`
          : "Evaluation failed. Moving to next question."
      );
    }

    setPendingTranscript(null);
    setEditedTranscript("");
    setScorecardResult(null);
    setVoicePhase("idle");
  }, [sessionId, pendingTranscript, editedTranscript]);

  const handleCancelTranscript = useCallback(() => {
    setPendingTranscript(null);
    setEditedTranscript("");
    setVoicePhase("idle");
  }, []);

  // ======== Render ========
  if (feedback) {
    return (
      <InterviewFeedback
        feedback={feedback}
        onBack={onBack}
        sessionId={sessionId ?? undefined}
        elapsedSeconds={elapsedSeconds}
      />
    );
  }

  if (startMutation.isError && !sessionId) {
    const startErrorMessage = startMutation.error instanceof Error
      ? startMutation.error.message
      : "Could not prepare the interview. Please try again.";
    return (
      <div className={styles.setupContainer}>
        <div className={styles.errorAlert} role="alert">
          <div className={styles.errorTitle}>Interview setup failed</div>
          <div>{startErrorMessage}</div>
        </div>
        <div className={styles.buttonRow}>
          <button className={styles.secondaryBtn} onClick={onBack}>Back</button>
          <button className={styles.primaryBtn} onClick={() => startMutation.mutate()}>
            🔄 Retry
          </button>
        </div>
      </div>
    );
  }

  if (!sessionId) {
    return (
      <div className={styles.loading}>
        <span className={styles.spinner} />
        Preparing your interview...
      </div>
    );
  }

  if (isComplete && !feedback) {
    return (
      <div className={styles.loading}>
        <span className={styles.spinner} />
        Evaluating your performance...
      </div>
    );
  }

  const progress = totalQuestions > 0 ? (answeredCount / totalQuestions) * 100 : 0;

  return (
    <div className={styles.chatContainer}>
      <header className={styles.chatHeader}>
        <div className={styles.chatHeaderInfo}>
          <h3>
            {isTraining ? "🎓" : "🎤"} Interview — {interviewType}
            <span className={styles.modeBadge}>
              {isTraining ? "Training" : "Real"}
            </span>
          </h3>
          <div className={styles.chatHeaderMeta}>
            Question {answeredCount + 1} of {totalQuestions}
            <span style={{ marginLeft: "12px", color: "#64748b", fontFamily: "monospace" }}>
              ⏱ {formatElapsed(elapsedSeconds)}
            </span>
          </div>
        </div>
        <button
          className={styles.secondaryBtn}
          onClick={() => {
            if (sessionId && answeredCount > 0) setIsComplete(true);
            else onBack();
          }}
        >
          {answeredCount > 0 ? "End Interview" : "Cancel"}
        </button>
      </header>

      <div className={styles.progressBar}>
        <div className={styles.progressFill} style={{ width: `${progress}%` }} />
      </div>

      <div className={styles.messageList} ref={scrollRef}>
        {turns.map((turn) => (
          <div key={turn.id} className={`${styles.message} ${styles[turn.role]}`}>
            <div className={styles.messageLabel}>
              {turn.role === "interviewer" ? "🎙 Interviewer" : "🧑 You"}
              {turn.category && <span className={styles.categoryBadge}>{turn.category}</span>}
              {turn.turnType && <span className={styles.turnTypeBadge}>{turn.turnType}</span>}
              {turn.capability && <span className={styles.capabilityBadge}>{turn.capability}</span>}
            </div>
            <div className={styles.messageBubble}>
              {turn.content}
              {turn.confidence !== undefined && turn.confidence > 0 && (
                <div className={styles.transcriptMeta}>
                  🎯 Confidence: {Math.round(turn.confidence * 100)}%
                </div>
              )}
            </div>
          </div>
        ))}

        {/* Coaching hint */}
        {isTraining && currentHint && !answerMutation.isPending && voicePhase === "idle" &&
          turns[turns.length - 1]?.role === "interviewer" ? (
          <div className={styles.coachingHint}>💡 Hint: {currentHint}</div>
        ) : null}

        {/* Scorecard inline (Training mode only — visual component) */}
        {isTraining && scorecardResult ? (
          <ScorecardCard data={scorecardResult} />
        ) : null}

        {/* Error message */}
        {answerError && (
          <div
            style={{
              padding: "10px 16px",
              background: "rgba(239, 68, 68, 0.1)",
              border: "1px solid rgba(239, 68, 68, 0.2)",
              borderRadius: "10px",
              color: "#f87171",
              fontSize: "0.82rem",
              margin: "8px 0",
            }}
          >
            ⚠️ {answerError}
            <button
              style={{
                marginLeft: "8px",
                padding: "2px 8px",
                border: "none",
                borderRadius: "4px",
                background: "rgba(239, 68, 68, 0.15)",
                color: "#f87171",
                cursor: "pointer",
                fontSize: "0.75rem",
              }}
              onClick={() => setAnswerError(null)}
            >
              Dismiss
            </button>
          </div>
        )}

        {/* Loading indicators */}
        {(answerMutation.isPending || voicePhase === "transcribing" || voicePhase === "evaluating") && (
          <div className={styles.typingIndicator}>
            <span className={styles.typingDot} />
            <span className={styles.typingDot} />
            <span className={styles.typingDot} />
            <span className={styles.typingLabel}>
              {voicePhase === "transcribing" ? "Transcribing..." :
               voicePhase === "evaluating" ? "Evaluating your answer..." :
               "Thinking..."}
            </span>
          </div>
        )}
      </div>

      {/* ===== Transcript confirmation overlay ===== */}
      {voicePhase === "confirming" && pendingTranscript && (
        <div className={styles.transcriptOverlay}>
          <div className={styles.transcriptCard}>
            <div className={styles.transcriptCardTitle}>
              ✏️ Review Your Transcript
            </div>
            <div className={styles.transcriptMeta}>
              Confidence: {Math.round(pendingTranscript.confidence * 100)}%
            </div>
            <textarea
              className={styles.transcriptEdit}
              value={editedTranscript}
              onChange={(e) => setEditedTranscript(e.target.value)}
              rows={4}
            />
            <div style={{ fontSize: "0.7rem", color: "#475569", marginBottom: "8px" }}>
              Press <kbd style={{ padding: "1px 4px", borderRadius: "3px", background: "rgba(148,163,184,0.15)", fontSize: "0.65rem" }}>Enter</kbd> to confirm · <kbd style={{ padding: "1px 4px", borderRadius: "3px", background: "rgba(148,163,184,0.15)", fontSize: "0.65rem" }}>Esc</kbd> to discard
            </div>
            <div className={styles.transcriptActions}>
              <button className={styles.secondaryBtn} onClick={handleCancelTranscript}>
                ✖ Discard
              </button>
              <button className={styles.primaryBtn} onClick={handleConfirmTranscript}
                disabled={!editedTranscript.trim()}>
                ✓ Confirm & Evaluate
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ===== Input area ===== */}
      <div className={styles.inputArea}>
        <div className={styles.inputRow}>
          {/* Voice button */}
          <button
            className={`${styles.voiceBtn} ${
              voicePhase === "recording" ? styles.recording : styles.idle
            }`}
            onClick={voicePhase === "recording" ? stopRecording : startRecording}
            disabled={answerMutation.isPending || voicePhase === "transcribing" ||
                      voicePhase === "evaluating" || voicePhase === "confirming"}
            title={voicePhase === "recording" ? `Recording... ${recordingDuration}s — click or press Space to stop` : "Start voice input (Space)"}
          >
            {voicePhase === "recording" ? `⏹ ${recordingDuration}s` : "🎤"}
          </button>

          <textarea
            className={styles.textInput}
            placeholder={
              voicePhase === "recording" ? `🔴 Recording... ${recordingDuration}s` :
              voicePhase === "confirming" ? "Review transcript above..." :
              "Type or use voice to answer... (Space = record)"
            }
            value={input}
            onChange={(e) => setInput(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === "Enter" && !e.shiftKey) {
                e.preventDefault();
                handleSend();
              }
            }}
            disabled={answerMutation.isPending || voicePhase !== "idle"}
            rows={1}
          />

          <button
            className={styles.sendBtn}
            onClick={handleSend}
            disabled={!input.trim() || answerMutation.isPending || voicePhase !== "idle"}
            title="Send answer"
          >
            ➤
          </button>
        </div>
      </div>
    </div>
  );
};
