"use client";

import React from "react";
import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import { getDailyMission } from "@/lib/api/daily-mission";
import styles from "./mission.module.css";

const TYPE_TO_ROUTE: Record<string, string> = {
  review: "/review",
  speaking: "/speaking",
  roleplay: "/roleplay",
  retry: "/notebook",
};

const TYPE_EMOJI: Record<string, string> = {
  review: "📝",
  speaking: "🎙️",
  roleplay: "🎭",
  retry: "🔄",
};

const getMissionHref = (type: string, id: string) => {
  const route = TYPE_TO_ROUTE[type] || "/";

  if (type === "speaking") {
    return `${route}?drillId=${encodeURIComponent(id)}`;
  }

  if (type === "roleplay") {
    return `${route}?scenarioId=${encodeURIComponent(id)}`;
  }

  return route;
};

export const MissionList: React.FC = () => {
  const { data: mission, isLoading, isError } = useQuery({
    queryKey: ["daily-mission"],
    queryFn: () => getDailyMission(),
  });

  if (isLoading) {
    return (
      <div className={styles.grid}>
        {[1, 2, 3].map((i) => (
          <div key={i} className={styles.skeleton}>
            <div className={styles.skeletonIcon} />
            <div className={styles.skeletonTitle} />
            <div className={styles.skeletonDesc} />
          </div>
        ))}
      </div>
    );
  }

  if (isError) {
    return (
      <div className={styles.emptyState}>
        <div className={styles.emptyEmoji}>⚠️</div>
        <p className={styles.emptyTitle}>Could not load missions</p>
        <p className={styles.emptyDesc}>
          Make sure the backend API is running, then refresh.
        </p>
      </div>
    );
  }

  if (!mission?.missions?.length) {
    return (
      <div className={styles.emptyState}>
        <div className={styles.emptyEmoji}>🌟</div>
        <p className={styles.emptyTitle}>No missions yet</p>
        <p className={styles.emptyDesc}>
          Browse content to get started with your first practice session.
        </p>
        <div className={styles.emptyLinks}>
          <Link href="/curriculum" className={styles.emptyLink}>
            📖 Browse Curriculum
          </Link>
          <Link href="/admin" className={styles.emptyLink}>
            ⚙️ Admin: Add Content
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className={styles.grid}>
      {mission.missions.map((item) => (
        <Link
          key={item.id}
          href={getMissionHref(item.type, item.id)}
          className={`${styles.card} ${item.isCompleted ? styles.completed : ""}`}
        >
          <div className={styles.cardHeader}>
            <span className={styles.icon}>
              {TYPE_EMOJI[item.type] || "📌"}
            </span>
            {item.isCompleted && <span className={styles.checkBadge}>✓</span>}
          </div>
          <h3 className={styles.cardTitle}>{item.title}</h3>
          <p className={styles.cardDesc}>{item.description}</p>
          {item.count !== undefined && (
            <div className={styles.countBadge}>{item.count} items</div>
          )}
        </Link>
      ))}
    </div>
  );
};
