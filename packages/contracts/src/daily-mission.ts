export type MissionType = "review" | "speaking" | "roleplay" | "retry";

export interface MissionItem {
  id: string;
  type: MissionType;
  title: string;
  description: string;
  isCompleted: boolean;
  count?: number;
}

export interface DailyMission {
  date: string;
  missions: MissionItem[];
}
