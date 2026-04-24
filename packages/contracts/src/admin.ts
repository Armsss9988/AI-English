import { Phrase, RoleplayScenario } from "./curriculum";

export type ContentStatus = "draft" | "published";

export interface AdminPhrase extends Phrase {
  status: ContentStatus;
}

export interface AdminScenario extends RoleplayScenario {
  status: ContentStatus;
}

export interface UpsertPhraseRequest extends Partial<Phrase> {
  id?: string;
  status?: ContentStatus;
}

export interface UpsertScenarioRequest extends Partial<RoleplayScenario> {
  id?: string;
  status?: ContentStatus;
}
