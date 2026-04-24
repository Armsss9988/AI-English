import React from 'react';
import { ProgressDashboard } from '@/features/progress/ProgressDashboard';

export default function ProgressPage() {
  return (
    <main style={{ padding: '40px', maxWidth: '1000px', margin: '0 auto' }}>
      <header style={{ marginBottom: '48px', textAlign: 'center' }}>
        <h1 style={{ fontSize: '2.5rem', fontWeight: '800', marginBottom: '12px' }}>Your Progress</h1>
        <p style={{ color: '#94a3b8', fontSize: '1.125rem' }}>Visualizing your English communication readiness for IT professionals.</p>
      </header>
      
      <ProgressDashboard />
    </main>
  );
}
