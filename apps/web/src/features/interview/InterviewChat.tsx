"use client";

import React, { useState, useEffect, useRef, useCallback } from "react";
import { useMutation } from "@tanstack/react-query";
import {
  StartInterviewResponse,
  InterviewTurn,
  InterviewFeedbackResponse,
} from "@english-coach/contracts";
import {
  startInterview,
  answerQuestion,
  finalizeInterview,
} from "@/lib/api/interview";
import { InterviewFeedback } from "./InterviewFeedback";
import styles from "./interview.module.css";

interface InterviewChatProps {
  profileId: string;
  jdText: string;
  interviewType: string;
  onBack: () => void;
}

export const InterviewChat: React.FC<InterviewChatProps> = ({
  profileId,
  jdText,
  interviewType,
  onBack,
}) => {
  const [sessionId, setSessionId] = useState<string | null>(null);
  const [turns, setTurns] = useState<InterviewTurn[]>([]);
  const [input, setInput] = useState("");
  const [totalQuestions, setTotalQuestions] = useState(0);
  const [answeredCount, setAnsweredCount] = useState(0);
  const [isComplete, setIsComplete] = useState(false);
  const [feedback, setFeedback] = useState<InterviewFeedbackResponse | null>(null);
  const [isRecording, setIsRecording] = useState(false);
  const [currentHint, setCurrentHint] = useState<string | null>(null);
  const scrollRef = useRef<HTMLDivElement>(null);
  const mediaRecorderRef = useRef<MediaRecorder | null>(null);
  const chunksRef = useRef<Blob[]>([]);

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
          timestamp: new Date().toISOString(),
        },
      ]);
    },
  });

  const startErrorMessage =
    startMutation.error instanceof Error
      ? startMutation.error.message
      : "Could not prepare the interview. Please try again.";

  // Answer question
  const answerMutation = useMutation({
    mutationFn: ({ sid, answer }: { sid: string; answer: string }) =>
      answerQuestion(sid, { answer }),
    onSuccess: (res) => {
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
            timestamp: new Date().toISOString(),
          },
        ]);
      }
    },
  });

  // Finalize
  const finalizeMutation = useMutation({
    mutationFn: (sid: string) => finalizeInterview(sid),
    onSuccess: (res) => setFeedback(res),
  });

  useEffect(() => {
    startMutation.mutate();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    if (scrollRef.current) {
      scrollRef.current.scrollTop = scrollRef.current.scrollHeight;
    }
  }, [turns]);

  useEffect(() => {
    if (isComplete && sessionId) {
      finalizeMutation.mutate(sessionId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isComplete]);

  const handleSend = useCallback(() => {
    if (!input.trim() || !sessionId) return;
    const currentInput = input;
    setInput("");

    // Add learner turn optimistically
    setTurns((prev) => [
      ...prev,
      {
        id: Date.now().toString(),
        role: "learner",
        content: currentInput,
        timestamp: new Date().toISOString(),
      },
    ]);

    answerMutation.mutate({ sid: sessionId, answer: currentInput });
  }, [input, sessionId, answerMutation]);

  // Voice recording
  const startRecording = useCallback(async () => {
    try {
      const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
      const mediaRecorder = new MediaRecorder(stream);
      mediaRecorderRef.current = mediaRecorder;
      chunksRef.current = [];

      mediaRecorder.ondataavailable = (e) => {
        if (e.data.size > 0) chunksRef.current.push(e.data);
      };

      mediaRecorder.onstop = () => {
        // For now, we use the Web Speech API for transcription
        // The audio blob is available in chunksRef.current if needed
        stream.getTracks().forEach((t) => t.stop());
      };

      mediaRecorder.start();
      setIsRecording(true);

      // Also start speech recognition if available
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const win = window as any;
      if (win.webkitSpeechRecognition || win.SpeechRecognition) {
        const SpeechRecognitionCtor = win.webkitSpeechRecognition || win.SpeechRecognition;
        const recognition = new SpeechRecognitionCtor();
        recognition.continuous = true;
        recognition.interimResults = true;
        recognition.lang = "en-US";

        let finalTranscript = "";

        recognition.onresult = (event: { resultIndex: number; results: { length: number; [key: number]: { isFinal: boolean; 0: { transcript: string } } } }) => {
          let interim = "";
          for (let i = event.resultIndex; i < event.results.length; i++) {
            if (event.results[i].isFinal) {
              finalTranscript += event.results[i][0].transcript + " ";
            } else {
              interim += event.results[i][0].transcript;
            }
          }
          setInput(finalTranscript + interim);
        };

        recognition.onerror = () => {
          // Fallback: user can type manually
        };

        recognition.start();
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        (mediaRecorderRef.current as any)._recognition = recognition;
      }
    } catch {
      // Microphone not available
      alert("Could not access microphone. Please type your answer instead.");
    }
  }, []);

  const stopRecording = useCallback(() => {
    if (mediaRecorderRef.current) {
      mediaRecorderRef.current.stop();
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const rec = mediaRecorderRef.current as any;
      if (rec._recognition) {
        rec._recognition.stop();
      }
    }
    setIsRecording(false);
  }, []);

  // Show feedback
  if (feedback) {
    return <InterviewFeedback feedback={feedback} onBack={onBack} />;
  }

  if (startMutation.isError && !sessionId) {
    return (
      <div className={styles.setupContainer}>
        <div className={styles.errorAlert} role="alert">
          <div className={styles.errorTitle}>Interview setup failed</div>
          <div>{startErrorMessage}</div>
        </div>
        <div className={styles.buttonRow}>
          <button className={styles.secondaryBtn} onClick={onBack}>
            Back
          </button>
        </div>
      </div>
    );
  }

  // Loading state
  if (!sessionId) {
    return (
      <div className={styles.loading}>
        <span className={styles.spinner} />
        Preparing your interview...
      </div>
    );
  }

  // Waiting for finalization
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
          <h3>Mock Interview — {interviewType}</h3>
          <div className={styles.chatHeaderMeta}>
            Question {answeredCount + 1} of {totalQuestions}
          </div>
        </div>
        <button
          className={styles.secondaryBtn}
          onClick={() => {
            if (sessionId && answeredCount > 0) {
              setIsComplete(true);
            } else {
              onBack();
            }
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
          <div
            key={turn.id}
            className={`${styles.message} ${styles[turn.role]}`}
          >
            <div className={styles.messageLabel}>
              {turn.role === "interviewer" ? "🎙 Interviewer" : "🧑 You"}
              {turn.category && (
                <span className={styles.categoryBadge}>{turn.category}</span>
              )}
            </div>
            <div className={styles.messageBubble}>{turn.content}</div>
          </div>
        ))}

        {currentHint && !answerMutation.isPending && turns[turns.length - 1]?.role === "interviewer" && (
          <div className={styles.coachingHint}>💡 Hint: {currentHint}</div>
        )}

        {answerMutation.isPending && (
          <div className={styles.typingIndicator}>
            <span className={styles.typingDot} />
            <span className={styles.typingDot} />
            <span className={styles.typingDot} />
          </div>
        )}
      </div>

      <div className={styles.inputArea}>
        <div className={styles.inputRow}>
          <button
            className={`${styles.voiceBtn} ${isRecording ? styles.recording : styles.idle}`}
            onClick={isRecording ? stopRecording : startRecording}
            disabled={answerMutation.isPending}
            title={isRecording ? "Stop recording" : "Start voice input"}
          >
            {isRecording ? "⏹" : "🎤"}
          </button>

          <textarea
            className={styles.textInput}
            placeholder={isRecording ? "Listening... speak now" : "Type or use voice to answer..."}
            value={input}
            onChange={(e) => setInput(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === "Enter" && !e.shiftKey) {
                e.preventDefault();
                handleSend();
              }
            }}
            disabled={answerMutation.isPending}
            rows={1}
          />

          <button
            className={styles.sendBtn}
            onClick={handleSend}
            disabled={!input.trim() || answerMutation.isPending}
            title="Send answer"
          >
            ➤
          </button>
        </div>
      </div>
    </div>
  );
};
