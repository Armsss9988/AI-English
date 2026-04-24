import React from 'react';
import { NotebookList } from '@/features/error-notebook/NotebookList';

export default function NotebookPage() {
  return (
    <main style={{ padding: '40px', maxWidth: '1000px', margin: '0 auto' }}>
      <header style={{ marginBottom: '48px' }}>
        <h1 style={{ fontSize: '2.5rem', fontWeight: '800', marginBottom: '12px' }}>Error Notebook</h1>
        <p style={{ color: '#94a3b8', fontSize: '1.125rem' }}>Track and review your recurring mistake patterns.</p>
      </header>
      
      <NotebookList />
    </main>
  );
}
