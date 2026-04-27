import React from "react";
import { RoleplayChat } from "@/features/roleplay/RoleplayChat";

type RoleplayPageProps = {
  searchParams?: Promise<{ scenarioId?: string }>;
};

export default async function RoleplayPage({
  searchParams,
}: RoleplayPageProps) {
  const params = await searchParams;
  const scenarioId = params?.scenarioId ?? "";

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
      <RoleplayChat scenarioId={scenarioId} />
    </main>
  );
}
