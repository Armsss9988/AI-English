'use client';

import React, { useState, useEffect, useRef } from 'react';
import { useMutation } from '@tanstack/react-query';
import { RoleplayTurn, RoleplaySession } from '@english-coach/contracts';
import { Button } from '@english-coach/ui';
import { recordTurn, finalizeRoleplay, startRoleplay } from '@/lib/api/roleplay';
import styles from './roleplay.module.css';

export const RoleplayChat: React.FC<{ scenarioId: string }> = ({ scenarioId }) => {
  const [input, setInput] = useState('');
  const [session, setSession] = useState<RoleplaySession | null>(null);
  const scrollRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    startRoleplay(scenarioId).then(setSession);
  }, [scenarioId]);

  useEffect(() => {
    if (scrollRef.current) {
      scrollRef.current.scrollTop = scrollRef.current.scrollHeight;
    }
  }, [session?.turns]);

  const turnMutation = useMutation({
    mutationFn: recordTurn,
    onSuccess: (aiTurn) => {
      if (session) {
        // The learner turn was already added in the mock logic or we add it here
        // For the mock, it adds both.
        startRoleplay(scenarioId).then(setSession); // Refresh session from mock
      }
    },
  });

  const finalizeMutation = useMutation({
    mutationFn: finalizeRoleplay,
    onSuccess: setSession,
  });

  const handleSend = () => {
    if (!input.trim() || !session) return;
    const currentInput = input;
    setInput('');
    turnMutation.mutate({ sessionId: session.id, content: currentInput });
  };

  if (!session) return <div className={styles.loading}>Entering scenario...</div>;

  if (session.status === 'completed') {
    return (
      <div className={styles.summaryCard}>
        <h2 className={styles.summaryTitle}>Session Complete</h2>
        <div className={styles.summaryContent}>{session.summary}</div>
        <Button onClick={() => window.location.reload()}>Try Another Scenario</Button>
      </div>
    );
  }

  return (
    <div className={styles.chatContainer}>
      <header className={styles.chatHeader}>
        <h3>{session.scenarioTitle}</h3>
        <Button 
          variant="ghost" 
          size="sm" 
          onClick={() => finalizeMutation.mutate(session.id)}
          isLoading={finalizeMutation.isPending}
        >
          Finish Session
        </Button>
      </header>

      <div className={styles.messageList} ref={scrollRef}>
        {session.turns.map((turn) => (
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
          onKeyPress={(e) => e.key === 'Enter' && handleSend()}
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
