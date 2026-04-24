import React from "react";
import { RoleplayChat } from "@/features/roleplay/RoleplayChat";

export default function RoleplayPage() {
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
      <RoleplayChat scenarioId="s1" />
    </main>
  );
}
