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
  retry: "/review/retry",
};

export const MissionList: React.FC = () => {
  const { data: mission, isLoading } = useQuery({
    queryKey: ["daily-mission"],
    queryFn: () => getDailyMission(),
  });

  if (isLoading)
    return <div className={styles.loading}>Loading your missions...</div>;

  return (
    <div className={styles.grid}>
      {mission?.missions.map((item) => (
        <Link
          key={item.id}
          href={TYPE_TO_ROUTE[item.type] || "#"}
          className={`${styles.card} ${item.isCompleted ? styles.completed : ""}`}
        >
          <div className={styles.cardHeader}>
            <span className={`${styles.icon} ${styles[item.type]}`}>
              {item.type[0].toUpperCase()}
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
