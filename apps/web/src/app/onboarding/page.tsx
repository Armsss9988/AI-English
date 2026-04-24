import React from 'react';
import { OnboardingForm } from '@/features/onboarding/OnboardingForm';

export default function OnboardingPage() {
  return (
    <main style={{ 
      display: 'flex', 
      alignItems: 'center', 
      justifyContent: 'center', 
      minHeight: '100vh',
      padding: '20px'
    }}>
      <OnboardingForm />
    </main>
  );
}
