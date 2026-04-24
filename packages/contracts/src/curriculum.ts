export type Difficulty = 'Beginner' | 'Intermediate' | 'Advanced';

export interface Phrase {
  id: string;
  content: string;
  meaning: string;
  function: string; // e.g., "Greeting", "Technical Explanation"
}

export interface Scenario {
  id: string;
  title: string;
  goal: string;
  difficulty: Difficulty;
  persona: string;
}

export interface CurriculumData {
  phrases: Phrase[];
  scenarios: Scenario[];
}
