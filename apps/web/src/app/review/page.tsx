import React from "react";
import { ReviewSession } from "@/features/review/ReviewSession";

export default function ReviewPage() {
  return (
    <main
      style={{
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        justifyContent: "center",
        minHeight: "100vh",
        padding: "20px",
      }}
    >
      <header style={{ marginBottom: "40px", textAlign: "center" }}>
        <h1
          style={{ fontSize: "2rem", fontWeight: "800", marginBottom: "8px" }}
        >
          Daily Review
        </h1>
        <p style={{ color: "#64748b" }}>
          Strengthen your memory with spaced repetition
        </p>
      </header>
      <ReviewSession />
    </main>
  );
}
