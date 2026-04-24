import React from "react";
import { MissionList } from "@/features/daily-mission/MissionList";

export default function TodayPage() {
  return (
    <div style={{ padding: "40px", maxWidth: "1000px", margin: "0 auto" }}>
      <header style={{ marginBottom: "48px" }}>
        <h1
          style={{
            fontSize: "2.5rem",
            fontWeight: "800",
            marginBottom: "12px",
          }}
        >
          Good morning!
        </h1>
        <p style={{ color: "#94a3b8", fontSize: "1.125rem" }}>
          Here are your learning goals for today.
        </p>
      </header>

      <section>
        <MissionList />
      </section>
    </div>
  );
}
