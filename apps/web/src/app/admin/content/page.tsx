import React from "react";
import { ContentManager } from "@/features/admin-content/ContentManager";

export default function AdminContentPage() {
  return (
    <main style={{ padding: "40px", maxWidth: "1000px", margin: "0 auto" }}>
      <header style={{ marginBottom: "48px" }}>
        <h1
          style={{
            fontSize: "2.5rem",
            fontWeight: "800",
            marginBottom: "12px",
          }}
        >
          Content Management
        </h1>
        <p style={{ color: "#94a3b8", fontSize: "1.125rem" }}>
          Draft and publish phrases and scenarios for the curriculum.
        </p>
      </header>

      <ContentManager />
    </main>
  );
}
