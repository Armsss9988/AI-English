'use client';

import React, { useState } from 'react';
import { PhraseBrowser } from '@/features/curriculum/PhraseBrowser';
import { ScenarioBrowser } from '@/features/curriculum/ScenarioBrowser';
import styles from '@/features/curriculum/curriculum.module.css';

export default function CurriculumPage() {
  const [activeTab, setActiveTab] = useState<'phrases' | 'scenarios'>('phrases');

  return (
    <div className={styles.container}>
      <header className={styles.header}>
        <h1 className={styles.title}>Curriculum Browser</h1>
        <p className={styles.subtitle}>Explore language functions and practice scenarios</p>
      </header>

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

      {activeTab === 'phrases' ? <PhraseBrowser /> : <ScenarioBrowser />}
    </div>
  );
}
