import { DailyMission } from "@english-coach/contracts";
import { apiClient } from "../apiClient";

type ApiMissionTask = {
  type: string;
  itemId: string;
  title: string;
  category: string;
  description?: string | null;
};

type ApiDailyMission = {
  missionDate: string;
  missions: ApiMissionTask[];
};

export const getDailyMission = async (): Promise<DailyMission> => {
  const response = await apiClient.get<ApiDailyMission>(
    "/progress/daily-mission"
  );

  return {
    date: response.missionDate,
    missions: response.missions.map((item) => {
      const type =
        item.type.toLowerCase() as DailyMission["missions"][number]["type"];

      return {
        id: item.itemId,
        type,
        title: item.title,
        description: item.description || item.category,
        isCompleted: false,
        count: type === "review" ? 1 : undefined,
      };
    }),
  };
};
