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
      <RoleplayChat scenarioId="22222222-2222-2222-2222-222222222222" />
    </main>
  );
}
