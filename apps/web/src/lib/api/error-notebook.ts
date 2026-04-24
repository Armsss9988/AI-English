import { NotebookEntry, NotebookCategory } from '@english-coach/contracts';

const delay = (ms: number) => new Promise((res) => setTimeout(res, ms));

const MOCK_ENTRIES: NotebookEntry[] = [
  {
    id: 'e1',
    pattern: 'Missing "for" after "wait"',
    original: 'We are waiting the API team.',
    corrected: 'We are waiting for the API team.',
    explanation: 'The verb "wait" is intransitive and requires the preposition "for" when followed by an object.',
    category: 'Grammar',
    recurrenceCount: 3,
    isArchived: false,
    lastSeenAt: new Date().toISOString(),
  },
  {
    id: 'e2',
    pattern: 'Subject-Verb agreement with "Everybody"',
    original: 'Everybody are ready.',
    corrected: 'Everybody is ready.',
    explanation: '"Everybody" and "Everyone" are singular indefinite pronouns and take singular verbs.',
    category: 'Grammar',
    recurrenceCount: 2,
    isArchived: false,
    lastSeenAt: new Date().toISOString(),
  },
  {
    id: 'e3',
    pattern: 'Improper usage of "discuss about"',
    original: 'Let us discuss about the requirements.',
    corrected: 'Let us discuss the requirements.',
    explanation: '"Discuss" is a transitive verb; do not use "about" after it.',
    category: 'Vocabulary',
    recurrenceCount: 1,
    isArchived: false,
    lastSeenAt: new Date().toISOString(),
  },
];

export const getNotebookEntries = async (category?: NotebookCategory): Promise<NotebookEntry[]> => {
  await delay(800);
  if (category) {
    return MOCK_ENTRIES.filter(e => e.category === category);
  }
  return [...MOCK_ENTRIES];
};

export const archiveEntry = async (id: string): Promise<void> => {
  await delay(500);
  console.log(`Archived entry ${id}`);
};
