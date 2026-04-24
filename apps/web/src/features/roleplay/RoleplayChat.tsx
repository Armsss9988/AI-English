"use client";

import React, { useState, useEffect, useRef } from "react";
import { useMutation } from "@tanstack/react-query";
import { RoleplayTurn, RoleplaySessionResponse } from "@english-coach/contracts";
import { Button } from "@english-coach/ui";
import {
  recordTurn,
  finalizeRoleplay,
  startRoleplay,
} from "@/lib/api/roleplay";
import styles from "./roleplay.module.css";

export const RoleplayChat: React.FC<{ scenarioId: string }> = ({
  scenarioId,
}) => {
  const [input, setInput] = useState("");
  const [sessionId, setSessionId] = useState<string | null>(null);
  const [scenarioTitle, setScenarioTitle] = useState<string>("");
  const [turns, setTurns] = useState<RoleplayTurn[]>([]);
  const [finalSession, setFinalSession] = useState<RoleplaySessionResponse | null>(null);
  const scrollRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    startRoleplay(scenarioId).then((res) => {
      setSessionId(res.sessionId);
      setScenarioTitle(res.scenarioTitle);
      setTurns([
        {
          id: Date.now().toString(),
          role: "ai",
          content: res.initialMessage,
          timestamp: new Date().toISOString(),
        },
      ]);
    });
  }, [scenarioId]);

  useEffect(() => {
    if (scrollRef.current) {
      scrollRef.current.scrollTop = scrollRef.current.scrollHeight;
    }
  }, [turns]);

  const turnMutation = useMutation({
    mutationFn: ({ sid, content }: { sid: string; content: string }) =>
      recordTurn(sid, { learnerMessage: content }),
    onSuccess: (res) => {
      // Add the AI response turn
      setTurns((prev) => [
        ...prev,
        {
          id: Date.now().toString(),
          role: "ai",
          content: res.clientMessage,
          timestamp: new Date().toISOString(),
        },
      ]);
    },
  });

  const finalizeMutation = useMutation({
    mutationFn: finalizeRoleplay,
    onSuccess: (res) => setFinalSession(res),
  });

  const handleSend = () => {
    if (!input.trim() || !sessionId) return;
    const currentInput = input;
    setInput("");

    // Optimistically add user turn
    setTurns((prev) => [
      ...prev,
      {
        id: Date.now().toString(),
        role: "learner",
        content: currentInput,
        timestamp: new Date().toISOString(),
      },
    ]);

    turnMutation.mutate({ sid: sessionId, content: currentInput });
  };

  if (!sessionId)
    return <div className={styles.loading}>Entering scenario...</div>;

  if (finalSession) {
    return (
      <div className={styles.summaryCard}>
        <h2 className={styles.summaryTitle}>Session Complete</h2>
        <div className={styles.summaryContent}>{finalSession.summary}</div>
        <Button onClick={() => window.location.reload()}>
          Try Another Scenario
        </Button>
      </div>
    );
  }

  return (
    <div className={styles.chatContainer}>
      <header className={styles.chatHeader}>
        <h3>{scenarioTitle}</h3>
        <Button
          variant="ghost"
          size="sm"
          onClick={() => finalizeMutation.mutate(sessionId)}
          isLoading={finalizeMutation.isPending}
        >
          Finish Session
        </Button>
      </header>

      <div className={styles.messageList} ref={scrollRef}>
        {turns.map((turn) => (
          <div
            key={turn.id}
            className={`${styles.message} ${styles[turn.role]}`}
          >
            <div className={styles.messageContent}>{turn.content}</div>
          </div>
        ))}
        {turnMutation.isPending && (
          <div className={`${styles.message} ${styles.ai} ${styles.typing}`}>
            <span className={styles.dot}>.</span>
            <span className={styles.dot}>.</span>
            <span className={styles.dot}>.</span>
          </div>
        )}
      </div>

      <div className={styles.inputBar}>
        <input
          type="text"
          placeholder="Type your response..."
          value={input}
          onChange={(e) => setInput(e.target.value)}
          onKeyPress={(e) => e.key === "Enter" && handleSend()}
          disabled={turnMutation.isPending}
        />
        <Button
          onClick={handleSend}
          disabled={!input.trim()}
          isLoading={turnMutation.isPending}
        >
          Send
        </Button>
      </div>
    </div>
  );
};

