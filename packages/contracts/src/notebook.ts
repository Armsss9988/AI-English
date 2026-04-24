export type NotebookCategory =
  | "Grammar"
  | "Vocabulary"
  | "Pronunciation"
  | "Business Context";

export interface NotebookEntry {
  id: string;
  pattern: string; // The recurring mistake pattern
  original: string; // An example original sentence
  corrected: string; // The corrected version
  explanation: string;
  category: NotebookCategory;
  recurrenceCount: number;
  isArchived: boolean;
  lastSeenAt: string;
}

export interface NotebookFilter {
  category?: NotebookCategory;
  showArchived?: boolean;
}
