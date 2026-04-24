import Link from 'next/link';

export default function Home() {
  return (
    <main style={{ 
      display: 'flex', 
      flexDirection: 'column',
      alignItems: 'center', 
      justifyContent: 'center', 
      minHeight: '100vh',
      gap: '24px'
    }}>
      <h1 style={{ fontSize: '3rem', fontWeight: 'bold' }}>English Coach</h1>
      <Link href="/onboarding" style={{ 
        padding: '12px 24px', 
        background: '#4f46e5', 
        borderRadius: '12px', 
        color: 'white', 
        textDecoration: 'none',
        fontWeight: '600'
      }}>
        Get Started
      </Link>
    </main>
  );
}
