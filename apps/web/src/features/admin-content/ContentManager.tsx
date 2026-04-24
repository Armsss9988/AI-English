'use client';

import React, { useState } from 'react';
import { useQuery, useMutation } from '@tanstack/react-query';
import { Button, Input, Select } from '@english-coach/ui';
import { getAdminPhrases, getAdminScenarios, upsertPhrase, upsertScenario } from '@/lib/api/admin-content';
import styles from './admin.module.css';

export const ContentManager: React.FC = () => {
  const [activeTab, setActiveTab] = useState<'phrases' | 'scenarios'>('phrases');
  const [showForm, setShowForm] = useState(false);

  const phrasesQuery = useQuery({ queryKey: ['admin-phrases'], queryFn: getAdminPhrases });
  const scenariosQuery = useQuery({ queryKey: ['admin-scenarios'], queryFn: getAdminScenarios });

  const phraseMutation = useMutation({ mutationFn: upsertPhrase, onSuccess: () => setShowForm(false) });
  const scenarioMutation = useMutation({ mutationFn: upsertScenario, onSuccess: () => setShowForm(false) });

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <div className={styles.tabs}>
          <button 
            className={`${styles.tab} ${activeTab === 'phrases' ? styles.tabActive : ''}`}
            onClick={() => setActiveTab('phrases')}
          >
            Phrases
          </button>
          <button 
            className={`${styles.tab} ${activeTab === 'scenarios' ? styles.tabActive : ''}`}
            onClick={() => setActiveTab('scenarios')}
          >
            Scenarios
          </button>
        </div>
        <Button onClick={() => setShowForm(true)}>Add New</Button>
      </div>

      <div className={styles.content}>
        {activeTab === 'phrases' ? (
          <div className={styles.list}>
            {phrasesQuery.data?.map(p => (
              <div key={p.id} className={styles.item}>
                <div>
                  <div className={styles.itemTitle}>{p.content}</div>
                  <div className={styles.itemMeta}>{p.category} • {p.difficulty} • {p.status}</div>
                </div>
                <Button variant="ghost" size="sm">Edit</Button>
              </div>
            ))}
          </div>
        ) : (
          <div className={styles.list}>
            {scenariosQuery.data?.map(s => (
              <div key={s.id} className={styles.item}>
                <div>
                  <div className={styles.itemTitle}>{s.title}</div>
                  <div className={styles.itemMeta}>{s.category} • {s.difficulty} • {s.status}</div>
                </div>
                <Button variant="ghost" size="sm">Edit</Button>
              </div>
            ))}
          </div>
        )}
      </div>

      {showForm && (
        <div className={styles.modalOverlay}>
          <div className={styles.modal}>
            <h3>Add New {activeTab === 'phrases' ? 'Phrase' : 'Scenario'}</h3>
            <form className={styles.form} onSubmit={(e) => e.preventDefault()}>
              <Input label="Content / Title" placeholder="Enter content..." />
              <Input label="Meaning / Description" placeholder="Enter meaning..." />
              <div className={styles.formRow}>
                <Select label="Category" options={[{label: 'Meetings', value: 'Meetings'}, {label: 'General', value: 'General'}]} />
                <Select label="Difficulty" options={[{label: 'Beginner', value: 'Beginner'}, {label: 'Advanced', value: 'Advanced'}]} />
              </div>
              <div className={styles.modalActions}>
                <Button variant="ghost" onClick={() => setShowForm(false)}>Cancel</Button>
                <Button>Save as Draft</Button>
                <Button variant="primary">Publish</Button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
