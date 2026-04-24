import React from "react";
import { SpeakingDrill } from "@/features/speaking/SpeakingDrill";

export default function SpeakingPage() {
  return (
    <main
      style={{
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        justifyContent: "center",
        minHeight: "100vh",
        padding: "40px",
      }}
    >
      <header style={{ marginBottom: "48px", textAlign: "center" }}>
        <h1
          style={{
            fontSize: "2.5rem",
            fontWeight: "800",
            marginBottom: "12px",
          }}
        >
          Speaking Drill
        </h1>
        <p style={{ color: "#64748b" }}>
          Practice your communication skills with real-time feedback
        </p>
      </header>

      <SpeakingDrill drillId="11111111-1111-1111-1111-111111111111" />
    </main>
  );
}
