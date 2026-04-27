"use client";

import React, { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import type {
  AdminPhrase,
  AdminScenario,
  CreatePhraseRequest,
  CreateScenarioRequest,
  CommunicationFunction,
  ContentLevel,
} from "@english-coach/contracts";
import {
  getAdminPhrases,
  getAdminScenarios,
  createPhrase,
  updatePhrase,
  publishPhrase,
  createScenario,
  publishScenario,
} from "@/lib/api/admin-content";
import styles from "./admin.module.css";

type Tab = "phrases" | "scenarios";

const FUNCTIONS: CommunicationFunction[] = [
  "Standup",
  "Issue",
  "Clarification",
  "Eta",
  "Recommendation",
  "Summary",
];
const LEVELS: ContentLevel[] = ["Survival", "Core", "ClientReady"];

/* ── Status Badge ── */
function StatusBadge({ status }: { status: string }) {
  const colors: Record<string, string> = {
    draft: "#f59e0b",
    review: "#3b82f6",
    published: "#22c55e",
    deprecated: "#ef4444",
    archived: "#6b7280",
  };
  return (
    <span
      style={{
        display: "inline-block",
        padding: "2px 10px",
        borderRadius: "100px",
        fontSize: "0.7rem",
        fontWeight: 600,
        textTransform: "uppercase",
        letterSpacing: "0.05em",
        background: `${colors[status] ?? "#6b7280"}20`,
        color: colors[status] ?? "#6b7280",
      }}
    >
      {status}
    </span>
  );
}

export const ContentManager: React.FC = () => {
  const queryClient = useQueryClient();
  const [activeTab, setActiveTab] = useState<Tab>("phrases");
  const [showPhraseForm, setShowPhraseForm] = useState(false);
  const [showScenarioForm, setShowScenarioForm] = useState(false);
  const [editingPhrase, setEditingPhrase] = useState<AdminPhrase | null>(null);

  // ── Phrase form state ──
  const [pContent, setPContent] = useState("");
  const [pMeaning, setPMeaning] = useState("");
  const [pCategory, setPCategory] = useState<CommunicationFunction>("Standup");
  const [pDifficulty, setPDifficulty] = useState<ContentLevel>("Core");
  const [pExample, setPExample] = useState("");

  // ── Scenario form state ──
  const [sTitle, setSTitle] = useState("");
  const [sGoal, setSGoal] = useState("");
  const [sContext, setSContext] = useState("");
  const [sUserRole, setSUserRole] = useState("");
  const [sPersona, setSPersona] = useState("");
  const [sMustCover, setSMustCover] = useState("");
  const [sPassCriteria, setSPassCriteria] = useState("");
  const [sDifficulty, setSDifficulty] = useState(1);

  // ── Queries ──
  const phrasesQuery = useQuery({
    queryKey: ["admin-phrases"],
    queryFn: getAdminPhrases,
  });
  const scenariosQuery = useQuery({
    queryKey: ["admin-scenarios"],
    queryFn: getAdminScenarios,
  });

  // ── Mutations ──
  const createPhraseMut = useMutation({
    mutationFn: (data: CreatePhraseRequest) => createPhrase(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin-phrases"] });
      resetPhraseForm();
    },
  });

  const updatePhraseMut = useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: string;
      data: CreatePhraseRequest;
    }) => updatePhrase(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin-phrases"] });
      resetPhraseForm();
    },
  });

  const publishPhraseMut = useMutation({
    mutationFn: (id: string) => publishPhrase(id),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: ["admin-phrases"] }),
  });

  const createScenarioMut = useMutation({
    mutationFn: (data: CreateScenarioRequest) => createScenario(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin-scenarios"] });
      resetScenarioForm();
    },
  });

  const publishScenarioMut = useMutation({
    mutationFn: (id: string) => publishScenario(id),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: ["admin-scenarios"] }),
  });

  // ── Form helpers ──
  function resetPhraseForm() {
    setShowPhraseForm(false);
    setEditingPhrase(null);
    setPContent("");
    setPMeaning("");
    setPCategory("Standup");
    setPDifficulty("Core");
    setPExample("");
  }

  function resetScenarioForm() {
    setShowScenarioForm(false);
    setSTitle("");
    setSGoal("");
    setSContext("");
    setSUserRole("");
    setSPersona("");
    setSMustCover("");
    setSPassCriteria("");
    setSDifficulty(1);
  }

  function openEditPhrase(p: AdminPhrase) {
    setPContent(p.content);
    setPMeaning(p.meaning);
    setPCategory(p.category as CommunicationFunction);
    setPDifficulty(p.difficulty as ContentLevel);
    setPExample(p.example);
    setEditingPhrase(p);
    setShowPhraseForm(true);
  }

  function handleSaveDraftPhrase() {
    if (!pContent.trim() || !pMeaning.trim() || !pExample.trim()) return;
    const data: CreatePhraseRequest = {
      content: pContent,
      meaning: pMeaning,
      category: pCategory,
      difficulty: pDifficulty,
      example: pExample,
    };
    if (editingPhrase) {
      updatePhraseMut.mutate({ id: editingPhrase.id, data });
    } else {
      createPhraseMut.mutate(data);
    }
  }

  function handleSaveDraftScenario() {
    if (!sTitle.trim() || !sGoal.trim() || !sContext.trim()) return;
    createScenarioMut.mutate({
      title: sTitle,
      goal: sGoal,
      workplaceContext: sContext,
      userRole: sUserRole,
      persona: sPersona,
      mustCoverPoints: sMustCover
        .split(",")
        .map((s) => s.trim())
        .filter(Boolean),
      passCriteria: sPassCriteria
        .split(",")
        .map((s) => s.trim())
        .filter(Boolean),
      difficulty: sDifficulty,
    });
  }

  const isPhraseValid = pContent.trim() && pMeaning.trim() && pExample.trim();
  const isScenarioValid = sTitle.trim() && sGoal.trim() && sContext.trim();

  return (
    <div className={styles.container}>
      {/* Tab bar */}
      <div className={styles.header}>
        <div className={styles.tabs}>
          <button
            className={`${styles.tab} ${activeTab === "phrases" ? styles.tabActive : ""}`}
            onClick={() => setActiveTab("phrases")}
          >
            📝 Phrases ({phrasesQuery.data?.length ?? "..."})
          </button>
          <button
            className={`${styles.tab} ${activeTab === "scenarios" ? styles.tabActive : ""}`}
            onClick={() => setActiveTab("scenarios")}
          >
            🎭 Scenarios ({scenariosQuery.data?.length ?? "..."})
          </button>
        </div>
        <button
          className={styles.addBtn}
          onClick={() =>
            activeTab === "phrases"
              ? (resetPhraseForm(), setShowPhraseForm(true))
              : (resetScenarioForm(), setShowScenarioForm(true))
          }
        >
          + Add New
        </button>
      </div>

      {/* ── Phrases List ── */}
      {activeTab === "phrases" && (
        <div className={styles.list}>
          {phrasesQuery.isLoading && (
            <p className={styles.loading}>Loading phrases...</p>
          )}
          {phrasesQuery.data?.map((p) => (
            <div key={p.id} className={styles.item}>
              <div className={styles.itemBody}>
                <div className={styles.itemTitle}>{p.content}</div>
                <div className={styles.itemMeta}>
                  {p.category} · {p.difficulty} · v{p.contentVersion}
                  <StatusBadge status={p.status} />
                </div>
                <div className={styles.itemMeaning}>{p.meaning}</div>
              </div>
              <div className={styles.itemActions}>
                <button
                  className={styles.actionBtn}
                  onClick={() => openEditPhrase(p)}
                >
                  ✏️
                </button>
                {p.status === "draft" && (
                  <button
                    className={styles.publishBtn}
                    onClick={() => publishPhraseMut.mutate(p.id)}
                    disabled={publishPhraseMut.isPending}
                  >
                    Publish
                  </button>
                )}
              </div>
            </div>
          ))}
          {phrasesQuery.data?.length === 0 && (
            <p className={styles.emptyMsg}>
              No phrases yet. Click &quot;+ Add New&quot; to create one.
            </p>
          )}
        </div>
      )}

      {/* ── Scenarios List ── */}
      {activeTab === "scenarios" && (
        <div className={styles.list}>
          {scenariosQuery.isLoading && (
            <p className={styles.loading}>Loading scenarios...</p>
          )}
          {scenariosQuery.data?.map((s) => (
            <div key={s.id} className={styles.item}>
              <div className={styles.itemBody}>
                <div className={styles.itemTitle}>{s.title}</div>
                <div className={styles.itemMeta}>
                  {s.userRole} → {s.persona} · Difficulty {s.difficulty} · v
                  {s.contentVersion}
                  <StatusBadge status={s.status} />
                </div>
                <div className={styles.itemMeaning}>{s.goal}</div>
              </div>
              <div className={styles.itemActions}>
                {s.status === "draft" && (
                  <button
                    className={styles.publishBtn}
                    onClick={() => publishScenarioMut.mutate(s.id)}
                    disabled={publishScenarioMut.isPending}
                  >
                    Publish
                  </button>
                )}
              </div>
            </div>
          ))}
          {scenariosQuery.data?.length === 0 && (
            <p className={styles.emptyMsg}>
              No scenarios yet. Click &quot;+ Add New&quot; to create one.
            </p>
          )}
        </div>
      )}

      {/* ── Phrase Form Modal ── */}
      {showPhraseForm && (
        <div className={styles.modalOverlay} onClick={() => resetPhraseForm()}>
          <div className={styles.modal} onClick={(e) => e.stopPropagation()}>
            <h3>{editingPhrase ? "Edit Phrase" : "New Phrase"}</h3>
            <form
              className={styles.form}
              onSubmit={(e) => e.preventDefault()}
            >
              <label className={styles.label}>
                English Content *
                <input
                  className={styles.input}
                  value={pContent}
                  onChange={(e) => setPContent(e.target.value)}
                  placeholder="e.g. I'd like to discuss the project timeline."
                />
              </label>
              <label className={styles.label}>
                Vietnamese Meaning *
                <input
                  className={styles.input}
                  value={pMeaning}
                  onChange={(e) => setPMeaning(e.target.value)}
                  placeholder="e.g. Tôi muốn thảo luận về tiến độ dự án."
                />
              </label>
              <label className={styles.label}>
                Example Context *
                <input
                  className={styles.input}
                  value={pExample}
                  onChange={(e) => setPExample(e.target.value)}
                  placeholder="e.g. Sprint planning meeting"
                />
              </label>
              <div className={styles.formRow}>
                <label className={styles.label}>
                  Category
                  <select
                    className={styles.input}
                    value={pCategory}
                    onChange={(e) =>
                      setPCategory(e.target.value as CommunicationFunction)
                    }
                  >
                    {FUNCTIONS.map((f) => (
                      <option key={f} value={f}>
                        {f}
                      </option>
                    ))}
                  </select>
                </label>
                <label className={styles.label}>
                  Level
                  <select
                    className={styles.input}
                    value={pDifficulty}
                    onChange={(e) =>
                      setPDifficulty(e.target.value as ContentLevel)
                    }
                  >
                    {LEVELS.map((l) => (
                      <option key={l} value={l}>
                        {l}
                      </option>
                    ))}
                  </select>
                </label>
              </div>
              <div className={styles.modalActions}>
                <button
                  className={styles.cancelBtn}
                  onClick={() => resetPhraseForm()}
                >
                  Cancel
                </button>
                <button
                  className={styles.draftBtn}
                  onClick={handleSaveDraftPhrase}
                  disabled={
                    !isPhraseValid ||
                    createPhraseMut.isPending ||
                    updatePhraseMut.isPending
                  }
                >
                  {editingPhrase ? "Save Changes" : "Save as Draft"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* ── Scenario Form Modal ── */}
      {showScenarioForm && (
        <div
          className={styles.modalOverlay}
          onClick={() => resetScenarioForm()}
        >
          <div className={styles.modal} onClick={(e) => e.stopPropagation()}>
            <h3>New Scenario</h3>
            <form
              className={styles.form}
              onSubmit={(e) => e.preventDefault()}
            >
              <label className={styles.label}>
                Title *
                <input
                  className={styles.input}
                  value={sTitle}
                  onChange={(e) => setSTitle(e.target.value)}
                  placeholder="e.g. Daily standup — blocker report"
                />
              </label>
              <label className={styles.label}>
                Communication Goal *
                <input
                  className={styles.input}
                  value={sGoal}
                  onChange={(e) => setSGoal(e.target.value)}
                  placeholder="e.g. Clearly explain the blocker and ask for help"
                />
              </label>
              <label className={styles.label}>
                Workplace Context *
                <input
                  className={styles.input}
                  value={sContext}
                  onChange={(e) => setSContext(e.target.value)}
                  placeholder="e.g. Daily scrum meeting"
                />
              </label>
              <div className={styles.formRow}>
                <label className={styles.label}>
                  Your Role
                  <input
                    className={styles.input}
                    value={sUserRole}
                    onChange={(e) => setSUserRole(e.target.value)}
                    placeholder="Developer"
                  />
                </label>
                <label className={styles.label}>
                  AI Persona
                  <input
                    className={styles.input}
                    value={sPersona}
                    onChange={(e) => setSPersona(e.target.value)}
                    placeholder="Tech Lead"
                  />
                </label>
              </div>
              <label className={styles.label}>
                Must-Cover Points (comma-separated)
                <input
                  className={styles.input}
                  value={sMustCover}
                  onChange={(e) => setSMustCover(e.target.value)}
                  placeholder="e.g. describe blocker, impact on sprint, request help"
                />
              </label>
              <label className={styles.label}>
                Pass Criteria (comma-separated)
                <input
                  className={styles.input}
                  value={sPassCriteria}
                  onChange={(e) => setSPassCriteria(e.target.value)}
                  placeholder="e.g. blocker_explained, help_requested"
                />
              </label>
              <label className={styles.label}>
                Difficulty (1-3)
                <input
                  className={styles.input}
                  type="number"
                  min={1}
                  max={3}
                  value={sDifficulty}
                  onChange={(e) => setSDifficulty(Number(e.target.value))}
                />
              </label>
              <div className={styles.modalActions}>
                <button
                  className={styles.cancelBtn}
                  onClick={() => resetScenarioForm()}
                >
                  Cancel
                </button>
                <button
                  className={styles.draftBtn}
                  onClick={handleSaveDraftScenario}
                  disabled={
                    !isScenarioValid || createScenarioMut.isPending
                  }
                >
                  Save as Draft
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
