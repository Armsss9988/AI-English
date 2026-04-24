export interface CapabilityScore {
  area: string; // e.g. "Meeting Participation", "Technical Explanation"
  score: number; // 0-100
  explanation: string;
}

export interface ReadinessSnapshot {
  overallScore: number;
  date: string;
  version: string;
  capabilities: CapabilityScore[];
  trend: "improving" | "stable" | "declining";
}
