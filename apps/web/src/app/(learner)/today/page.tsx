"use client";

import React from "react";
import Link from "next/link";
import { MissionList } from "@/features/daily-mission/MissionList";

function getGreeting(): string {
  const hour = new Date().getHours();
  if (hour < 12) return "Good morning";
  if (hour < 17) return "Good afternoon";
  return "Good evening";
}

const practiceCards = [
  {
    emoji: "📝",
    title: "Review",
    desc: "Practice your spaced-repetition review items",
    href: "/review",
    gradient: "linear-gradient(135deg, #667eea 0%, #764ba2 100%)",
  },
  {
    emoji: "🎙️",
    title: "Speaking Drill",
    desc: "Practice pronunciation and fluency with guided speaking exercises",
    href: "/speaking",
    gradient: "linear-gradient(135deg, #f093fb 0%, #f5576c 100%)",
  },
  {
    emoji: "🎭",
    title: "Roleplay",
    desc: "Simulate real workplace conversations with AI personas",
    href: "/roleplay",
    gradient: "linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)",
  },
  {
    emoji: "🎤",
    title: "Mock Interview",
    desc: "Full-voice mock interviews based on your CV and a Job Description",
    href: "/interview",
    gradient: "linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)",
  },
  {
    emoji: "📓",
    title: "Error Notebook",
    desc: "Review and fix your recurring mistakes",
    href: "/notebook",
    gradient: "linear-gradient(135deg, #fa709a 0%, #fee140 100%)",
  },
  {
    emoji: "📊",
    title: "Progress",
    desc: "Track your readiness and capability growth",
    href: "/progress",
    gradient: "linear-gradient(135deg, #a18cd1 0%, #fbc2eb 100%)",
  },
];

export default function TodayPage() {
  return (
    <div className="today-container">
      <header className="today-header">
        <h1 className="today-greeting">{getGreeting()}! 👋</h1>
        <p className="today-subtitle">
          Here&apos;s your learning plan for today. Stay consistent!
        </p>
      </header>

      <section className="today-section">
        <h2 className="today-section-title">🎯 Today&apos;s Missions</h2>
        <MissionList />
      </section>

      <section className="today-section">
        <h2 className="today-section-title">🚀 Practice</h2>
        <div className="practice-grid">
          {practiceCards.map((card) => (
            <Link
              key={card.href}
              href={card.href}
              className="practice-card"
              style={{ "--card-gradient": card.gradient } as React.CSSProperties}
            >
              <div className="practice-card-emoji">{card.emoji}</div>
              <h3 className="practice-card-title">{card.title}</h3>
              <p className="practice-card-desc">{card.desc}</p>
              <span className="practice-card-arrow">→</span>
            </Link>
          ))}
        </div>
      </section>

      <section className="today-section">
        <h2 className="today-section-title">📚 Content</h2>
        <div className="content-links">
          <Link href="/curriculum" className="content-link">
            <span>📖</span>
            <span>Browse Curriculum</span>
          </Link>
          <Link href="/admin" className="content-link">
            <span>⚙️</span>
            <span>Admin: Manage Content</span>
          </Link>
        </div>
      </section>
    </div>
  );
}
