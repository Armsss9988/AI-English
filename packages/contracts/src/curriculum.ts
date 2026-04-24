export type Difficulty = "Beginner" | "Intermediate" | "Advanced";

export interface Phrase {
  id: string;
  content: string;
  meaning: string;
  category: string;
  difficulty: Difficulty;
  function: string; // e.g., "Greeting", "Technical Explanation"
}

export interface Scenario {
  id: string;
  title: string;
  goal: string;
  category: string;
  difficulty: Difficulty;
  persona: string;
}

export type RoleplayScenario = Scenario;

export interface CurriculumData {
  phrases: Phrase[];
  scenarios: Scenario[];
}
