# Interview Training OS Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Turn Interview Practice from a generic AI mock interview into a structured English interview training system with scoring, retry, personal memory, review, and readiness tracking.

**Architecture:** Keep business rules in `EnglishCoach.Application` use cases and `EnglishCoach.Domain` policies. AI providers return normalized coaching evidence only; code decides retry/pass, notebook promotion, review scheduling, and readiness changes. Frontend renders the training loop but does not calculate scores or progression.

**Tech Stack:** ASP.NET Core Web API, EF Core, PostgreSQL, Next.js, TypeScript contracts, existing NIM/OpenAI-compatible adapter, existing Error Notebook, Review, and Progress modules.

---

## Product Principle

The feature is valuable only if it gives the learner a repeatable training loop that a single ChatGPT prompt cannot conveniently provide:

```text
CV + JD
-> skill gap map
-> interview plan
-> timed answer
-> answer scorecard with evidence
-> retry same question until pass or max attempts
-> extract personal mistakes and useful phrases
-> add to notebook/review
-> update JD readiness map
-> show progress over sessions
```

## Non-Negotiable Behavior

- A learner answer is not automatically progress.
- An answer counts only when it passes the retry policy or reaches max retry attempts.
- Every low score must have evidence from the learner answer.
- Every correction must include a better professional English version.
- Repeated mistakes must become notebook/review material.
- Readiness is task-based, not a fake global English score.
- Provider failure must degrade to fallback coaching, not block the session with HTTP 500.

## File Map

### Backend Domain

- Modify `apps/api/src/EnglishCoach.Domain/InterviewPractice/InterviewTurn.cs`
  - Store question number, attempt number, accepted/progress status, and normalized answer evaluation JSON.
- Modify `apps/api/src/EnglishCoach.Domain/InterviewPractice/InterviewSession.cs`
  - Count accepted answers instead of raw learner turns.
  - Add methods for recording evaluated learner attempts and retry/advance decisions.
- Create `apps/api/src/EnglishCoach.Domain/InterviewPractice/InterviewAnswerScorecard.cs`
  - Strongly typed scorecard value object.
- Create `apps/api/src/EnglishCoach.Domain/InterviewPractice/InterviewRetryPolicy.cs`
  - Code-owned pass/retry thresholds.

### Backend Application

- Modify `apps/api/src/EnglishCoach.Application/Ports/IInterviewProviders.cs`
  - Add answer evaluation provider contract and normalized result records.
- Modify `apps/api/src/EnglishCoach.Application/InterviewPractice/AnswerInterviewQuestionUseCase.cs`
  - Evaluate answer, apply retry policy, persist evaluation, return scorecard/retry response.
- Create `apps/api/src/EnglishCoach.Application/InterviewPractice/GetInterviewReadinessMapQuery.cs`
  - Return task-level readiness for the current JD/profile.
- Create `apps/api/src/EnglishCoach.Application/InterviewPractice/PromoteInterviewCoachingItemsUseCase.cs`
  - Promote mistakes and phrase candidates into existing notebook/review modules.

### Backend Infrastructure

- Modify `apps/api/src/EnglishCoach.Infrastructure/AI/OpenAI/NimInterviewService.cs`
  - Implement `EvaluateAnswerAsync` with strict JSON output and parser validation.
- Modify `apps/api/src/EnglishCoach.Infrastructure/Persistence/Configurations/InterviewPracticeConfiguration.cs`
  - Persist new turn fields.
- Add EF migration under `apps/api/src/EnglishCoach.Infrastructure/Persistence/Migrations/`.

### Contracts

- Modify `apps/api/src/EnglishCoach.Contracts/InterviewPractice/InterviewContracts.cs`
  - Add scorecard, corrections, retry fields, and readiness response DTOs.
- Modify `packages/contracts/src/interview.ts`
  - Mirror backend DTOs.

### API

- Modify `apps/api/src/EnglishCoach.Api/Program.cs`
  - Register new use cases.
  - Add readiness map and coaching promotion endpoints.
  - Keep endpoints thin.

### Frontend

- Modify `apps/web/src/lib/api/interview.ts`
  - Add readiness and promotion calls.
- Modify `apps/web/src/features/interview/InterviewChat.tsx`
  - Show scorecard after each answer.
  - Show retry-required state.
  - Do not append next question when retry is required.
- Create `apps/web/src/features/interview/AnswerScorecard.tsx`
  - Render evidence, better answer, and retry drill.
- Create `apps/web/src/features/interview/InterviewReadinessMap.tsx`
  - Show JD readiness by capability.
- Modify `apps/web/src/features/interview/interview.module.css`
  - Add scorecard/readiness styles.

---

## Phase 1: Answer Scorecard and Retry Gate

### Task 1: Add Scorecard Domain Types

**Files:**
- Create: `apps/api/src/EnglishCoach.Domain/InterviewPractice/InterviewAnswerScorecard.cs`
- Create: `apps/api/src/EnglishCoach.Domain/InterviewPractice/InterviewRetryPolicy.cs`
- Test: `apps/api/tests/EnglishCoach.UnitTests/InterviewPractice/InterviewRetryPolicyTests.cs`

**Implementation Checklist:**

- [ ] Add `InterviewAnswerScorecard` with these fields:
  - `ContentFit`
  - `StarStructure`
  - `TechnicalCredibility`
  - `EnglishClarity`
  - `ProfessionalTone`
  - `FluencyDelivery`
  - `OverallScore`
  - `Evidence`
  - `Corrections`
  - `BetterAnswer`
  - `DrillPrompt`
  - `MistakeCandidates`
  - `PhraseCandidates`

- [ ] Scores use `0..100` for overall and `1..5` for dimensions.

- [ ] Add `InterviewRetryDecision` with:
  - `Accepted`
  - `RetryRequired`
  - `Reason`
  - `AttemptNumber`
  - `MaxAttempts`

- [ ] Add `InterviewRetryPolicy.Decide(scorecard, attemptNumber)` with these rules:
  - Accepted when `OverallScore >= 70`, `ContentFit >= 3`, and `EnglishClarity >= 3`.
  - Retry required when below threshold and `attemptNumber < 3`.
  - Accepted with remediation when below threshold and `attemptNumber >= 3`.

**Test Checklist:**

- [ ] Test high score is accepted.
- [ ] Test low content fit requires retry.
- [ ] Test low English clarity requires retry.
- [ ] Test third weak attempt is accepted with remediation.
- [ ] Test invalid score values throw clear argument exceptions.

**Verification Command:**

```powershell
node apps/api/scripts/run-dotnet.mjs test apps/api/tests/EnglishCoach.UnitTests/EnglishCoach.UnitTests.csproj --filter "FullyQualifiedName~InterviewRetryPolicyTests"
```

### Task 2: Extend Interview Turn Progress Semantics

**Files:**
- Modify: `apps/api/src/EnglishCoach.Domain/InterviewPractice/InterviewTurn.cs`
- Modify: `apps/api/src/EnglishCoach.Domain/InterviewPractice/InterviewSession.cs`
- Test: `apps/api/tests/EnglishCoach.UnitTests/InterviewPractice/InterviewSessionTests.cs`

**Implementation Checklist:**

- [ ] Add to `InterviewTurn`:
  - `int QuestionNumber`
  - `int AttemptNumber`
  - `bool CountsTowardProgress`
  - `string AnswerEvaluationJson`

- [ ] For interviewer turns, `QuestionNumber` is the question position shown to the learner.

- [ ] For learner turns, `QuestionNumber` references the current interviewer question.

- [ ] `LearnerAnswerCount` must count only learner turns where `CountsTowardProgress == true`.

- [ ] Add `RecordEvaluatedLearnerAnswer(message, audioUrl, questionNumber, attemptNumber, countsTowardProgress, answerEvaluationJson)`.

- [ ] Keep `AddLearnerTurn` only if existing tests still need it; internally call the new method with accepted/default values.

**Test Checklist:**

- [ ] A retry attempt does not increase `LearnerAnswerCount`.
- [ ] An accepted attempt increases `LearnerAnswerCount`.
- [ ] Question limit uses accepted answer count.
- [ ] Evaluation JSON is stored on the learner turn.
- [ ] Empty evaluation JSON is allowed only for legacy/default accepted answers.

**Verification Command:**

```powershell
node apps/api/scripts/run-dotnet.mjs test apps/api/tests/EnglishCoach.UnitTests/EnglishCoach.UnitTests.csproj --filter "FullyQualifiedName~InterviewSessionTests"
```

### Task 3: Add Scorecard Contracts

**Files:**
- Modify: `apps/api/src/EnglishCoach.Contracts/InterviewPractice/InterviewContracts.cs`
- Modify: `packages/contracts/src/interview.ts`
- Test: existing API tests compile; add API response assertions in Task 5.

**Contract Shape:**

```csharp
public record InterviewAnswerScorecardResponse(
    int OverallScore,
    int ContentFit,
    int StarStructure,
    int TechnicalCredibility,
    int EnglishClarity,
    int ProfessionalTone,
    int? FluencyDelivery,
    List<string> Evidence,
    List<InterviewAnswerCorrectionResponse> Corrections,
    string BetterAnswer,
    string DrillPrompt,
    List<InterviewMistakeCandidateResponse> MistakeCandidates,
    List<InterviewPhraseCandidateResponse> PhraseCandidates
);

public record InterviewAnswerCorrectionResponse(
    string Original,
    string Corrected,
    string ExplanationVi
);

public record InterviewMistakeCandidateResponse(
    string PatternKey,
    string Title,
    string Evidence,
    string Correction,
    string Severity
);

public record InterviewPhraseCandidateResponse(
    string Text,
    string MeaningVi,
    string Usage
);
```

**Answer Response Additions:**

- [ ] Add `InterviewAnswerScorecardResponse? Scorecard`.
- [ ] Add `bool RetryRequired`.
- [ ] Add `string? RetryReason`.
- [ ] Add `int AttemptNumber`.
- [ ] Add `int MaxAttempts`.

**Acceptance Checklist:**

- [ ] TypeScript contract exactly mirrors C# response names in camelCase.
- [ ] Existing frontend still compiles after fields are optional.
- [ ] No provider-specific fields leak into contracts.

**Verification Command:**

```powershell
pnpm --dir apps/web typecheck
```

### Task 4: Provider Contract for Answer Evaluation

**Files:**
- Modify: `apps/api/src/EnglishCoach.Application/Ports/IInterviewProviders.cs`
- Modify: `apps/api/src/EnglishCoach.Infrastructure/AI/OpenAI/NimInterviewService.cs`
- Test: `apps/api/tests/EnglishCoach.UnitTests/InterviewPractice/AnswerInterviewQuestionUseCaseTests.cs`

**Application Contract:**

- [ ] Add `EvaluateAnswerAsync(InterviewAnswerEvaluationContext context, CancellationToken ct = default)`.

- [ ] `InterviewAnswerEvaluationContext` includes:
  - `SessionId`
  - `CvAnalysis`
  - `JdAnalysis`
  - `InterviewPlan`
  - `Question`
  - `LearnerAnswer`
  - `InterviewType`
  - `QuestionNumber`
  - `AttemptNumber`

- [ ] `InterviewAnswerEvaluationResult` returns:
  - `IsSuccess`
  - `InterviewAnswerScorecard? Scorecard`
  - `ErrorMessage`
  - `Provider`

**Provider Prompt Requirements:**

- [ ] Output valid JSON only.
- [ ] Evaluate the answer, do not ask the next question.
- [ ] Correct only the 1-3 highest-impact issues.
- [ ] Prefer interview-ready English over textbook grammar.
- [ ] Include evidence from the learner answer.
- [ ] Include Vietnamese explanations for corrections.
- [ ] Include `mistakeCandidates` and `phraseCandidates`.

**Fallback Evaluation:**

- [ ] If provider fails, return a deterministic scorecard:
  - `OverallScore = 60`
  - `ContentFit = 3`
  - `EnglishClarity = 3`
  - one evidence item: `"AI evaluation was unavailable; fallback coaching was used."`
  - one drill prompt asking user to answer with role, action, and result.

**Test Checklist:**

- [ ] Fake provider success returns scorecard.
- [ ] Fake provider failure triggers fallback scorecard.
- [ ] Use case does not throw 500 when evaluation fails.

---

## Phase 2: Answer Flow Becomes a Training Loop

### Task 5: Apply Retry Policy in `AnswerInterviewQuestionUseCase`

**Files:**
- Modify: `apps/api/src/EnglishCoach.Application/InterviewPractice/AnswerInterviewQuestionUseCase.cs`
- Test: `apps/api/tests/EnglishCoach.ApiTests/Interview/InterviewEndpointsTests.cs`

**Flow Checklist:**

- [ ] Load session and verify ownership.
- [ ] Determine current question from latest interviewer turn.
- [ ] Determine attempt number for that question.
- [ ] Evaluate learner answer.
- [ ] Apply `InterviewRetryPolicy`.
- [ ] Persist learner turn with scorecard JSON and `CountsTowardProgress = decision.Accepted`.
- [ ] If retry required:
  - return `retryRequired = true`
  - return scorecard
  - return no next question
  - keep `answeredCount` unchanged
- [ ] If accepted:
  - advance to next question or complete interview
  - return scorecard and next question

**API Test Checklist:**

- [ ] Low score answer returns 200 with `retryRequired = true`.
- [ ] Low score retry does not increment `answeredCount`.
- [ ] Third low attempt returns accepted progress with remediation.
- [ ] Accepted answer returns next question.
- [ ] Provider timeout returns fallback scorecard, not HTTP 500.

**Verification Commands:**

```powershell
node apps/api/scripts/run-dotnet.mjs test apps/api/tests/EnglishCoach.ApiTests/EnglishCoach.ApiTests.csproj --filter "FullyQualifiedName~InterviewEndpointsTests"
node apps/api/scripts/run-dotnet.mjs test apps/api/EnglishCoach.sln
```

### Task 6: Scorecard UI and Retry UX

**Files:**
- Create: `apps/web/src/features/interview/AnswerScorecard.tsx`
- Modify: `apps/web/src/features/interview/InterviewChat.tsx`
- Modify: `apps/web/src/features/interview/interview.module.css`
- Modify: `apps/web/src/lib/api/interview.ts`

**UI Checklist:**

- [ ] After user submits an answer, show scorecard before next question.
- [ ] Show overall score and dimension scores.
- [ ] Show evidence snippets.
- [ ] Show corrections in this format:
  - original phrase
  - better phrase
  - Vietnamese explanation
- [ ] Show better full answer.
- [ ] If `retryRequired`, show a clear retry panel and keep the same interviewer question active.
- [ ] If accepted, show next question normally.
- [ ] Disable send button while evaluation is pending.
- [ ] Avoid showing raw provider errors to learner.

**Frontend Verification:**

```powershell
pnpm --dir apps/web typecheck
pnpm --dir apps/web lint
pnpm --dir apps/web build
```

**Manual UI Checklist:**

- [ ] Submit a weak answer and see retry panel.
- [ ] Submit improved answer and see next question.
- [ ] End interview and see final feedback.
- [ ] Refresh page does not corrupt persisted backend state.

---

## Phase 3: Memory and Review Integration

### Task 7: Promote Interview Mistakes and Phrases

**Files:**
- Create: `apps/api/src/EnglishCoach.Application/InterviewPractice/PromoteInterviewCoachingItemsUseCase.cs`
- Modify: `apps/api/src/EnglishCoach.Api/Program.cs`
- Modify: `apps/web/src/lib/api/interview.ts`
- Modify: `apps/web/src/features/interview/AnswerScorecard.tsx`
- Test: `apps/api/tests/EnglishCoach.ApiTests/Interview/InterviewEndpointsTests.cs`

**Backend Checklist:**

- [ ] Endpoint: `POST /me/interview/sessions/{sessionId}/turns/{turnId}/promote-coaching-items`.
- [ ] Load learner turn and verify it belongs to learner.
- [ ] Read scorecard `MistakeCandidates`.
- [ ] For each mistake candidate, call existing `PromoteErrorPatternUseCase`.
- [ ] For each phrase candidate, call existing `EnsureReviewItemExistsUseCase`.
- [ ] Return promoted notebook entry IDs and review item IDs.
- [ ] Operation is idempotent for repeated clicks.

**Frontend Checklist:**

- [ ] Scorecard has `Add mistakes to notebook` action.
- [ ] Scorecard has `Add useful phrases to review` action.
- [ ] Success state shows what was added.
- [ ] Duplicate promotion shows stable success, not an error toast.

**Acceptance Checklist:**

- [ ] Repeated grammar pattern merges in notebook.
- [ ] Phrase review item is not duplicated.
- [ ] Promotion is optional but strongly visible after weak answer.

### Task 8: Interview-Derived Review Queue

**Files:**
- Modify: `apps/api/src/EnglishCoach.Application/Review/EnsureReviewItemExistsUseCase.cs` only if needed for source metadata.
- Modify: `apps/api/src/EnglishCoach.Contracts/Review/EnsureReviewItemRequest.cs` only if source metadata is missing.
- Test: `apps/api/tests/EnglishCoach.UnitTests/Review/**`

**Checklist:**

- [ ] Review item source can identify `InterviewPractice`.
- [ ] Suggested phrases from interview include meaning and usage.
- [ ] Review cards show phrase, meaning, and interview context.
- [ ] Completing review uses existing review state machine.

---

## Phase 4: JD Readiness Map

### Task 9: Backend Readiness Map Query

**Files:**
- Create: `apps/api/src/EnglishCoach.Application/InterviewPractice/GetInterviewReadinessMapQuery.cs`
- Modify: `apps/api/src/EnglishCoach.Contracts/InterviewPractice/InterviewContracts.cs`
- Modify: `packages/contracts/src/interview.ts`
- Modify: `apps/api/src/EnglishCoach.Api/Program.cs`
- Test: `apps/api/tests/EnglishCoach.ApiTests/Interview/InterviewEndpointsTests.cs`

**Readiness Dimensions:**

- [ ] `SelfIntroduction`
- [ ] `ProjectDeepDive`
- [ ] `BehavioralStar`
- [ ] `TechnicalTradeoff`
- [ ] `ClientCommunication`
- [ ] `WeakSpotRetry`

**Calculation V1 Rules:**

- [ ] A dimension is `Ready` when latest accepted answer average score is `>= 75`.
- [ ] A dimension is `NeedsPractice` when average score is `60..74`.
- [ ] A dimension is `Weak` when average score is `< 60`.
- [ ] A dimension is `NotAttempted` when no accepted answer exists.
- [ ] Store formula label as `interview-readiness-v1` in response.

**Response Shape:**

- [ ] `sessionId`
- [ ] `formulaVersion`
- [ ] `overallReadiness`
- [ ] `dimensions[]`
- [ ] `topWeaknesses[]`
- [ ] `recommendedNextDrills[]`

**Acceptance Checklist:**

- [ ] Readiness is explainable with evidence.
- [ ] UI does not calculate readiness.
- [ ] No global English score is shown without dimension context.

### Task 10: Readiness Map UI

**Files:**
- Create: `apps/web/src/features/interview/InterviewReadinessMap.tsx`
- Modify: `apps/web/src/features/interview/InterviewFeedback.tsx`
- Modify: `apps/web/src/features/interview/interview.module.css`

**UI Checklist:**

- [ ] Final feedback page shows readiness by dimension.
- [ ] Each dimension shows status, score, and evidence.
- [ ] Weak dimensions link to retry drills or review items.
- [ ] Copy is direct: `Ready`, `Needs practice`, `Weak`, `Not attempted`.

---

## Phase 5: 7-Day Interview Sprint

### Task 11: Generate Sprint Plan From CV/JD

**Files:**
- Create: `apps/api/src/EnglishCoach.Application/InterviewPractice/CreateInterviewSprintPlanUseCase.cs`
- Modify: `apps/api/src/EnglishCoach.Contracts/InterviewPractice/InterviewContracts.cs`
- Modify: `packages/contracts/src/interview.ts`
- Modify: `apps/api/src/EnglishCoach.Api/Program.cs`
- Test: `apps/api/tests/EnglishCoach.ApiTests/Interview/InterviewEndpointsTests.cs`

**Sprint Plan Structure:**

- [ ] Day 1: self-introduction and CV story.
- [ ] Day 2: project deep dive.
- [ ] Day 3: technical tradeoffs.
- [ ] Day 4: behavioral STAR.
- [ ] Day 5: weak spot retry.
- [ ] Day 6: pressure mock interview.
- [ ] Day 7: final readiness check.

**Rules:**

- [ ] Plan uses latest CV analysis and JD analysis.
- [ ] Weak dimensions from readiness map influence Day 5 and Day 6.
- [ ] Each day has one target answer, one phrase set, and one success criterion.

### Task 12: Sprint Plan UI

**Files:**
- Create: `apps/web/src/features/interview/InterviewSprintPlan.tsx`
- Modify: `apps/web/src/features/interview/InterviewSetup.tsx`
- Modify: `apps/web/src/features/interview/interview.module.css`

**UI Checklist:**

- [ ] After CV/JD setup, user sees `Start mock interview` and `Create 7-day sprint`.
- [ ] Sprint days show status and next action.
- [ ] Each day links to focused interview practice.

---

## Phase 6: Provider Observability and Safety

### Task 13: Provider Audit Metadata

**Files:**
- Modify: `apps/api/src/EnglishCoach.Domain/InterviewPractice/InterviewTurn.cs`
- Modify: `apps/api/src/EnglishCoach.Infrastructure/Persistence/Configurations/InterviewPracticeConfiguration.cs`
- Modify: `apps/api/src/EnglishCoach.Application/InterviewPractice/AnswerInterviewQuestionUseCase.cs`
- Test: `apps/api/tests/EnglishCoach.UnitTests/InterviewPractice/InterviewSessionTests.cs`

**Checklist:**

- [ ] Store provider kind for evaluation.
- [ ] Store model id used for evaluation.
- [ ] Store prompt template version.
- [ ] Store fallback flag.
- [ ] Do not store raw provider payload.

### Task 14: Failure Mode QA

**Files:**
- Modify: `apps/api/tests/EnglishCoach.ApiTests/Interview/InterviewEndpointsTests.cs`
- Add or update manual QA notes in this plan after verification.

**Checklist:**

- [ ] CV analysis provider failure returns friendly 502.
- [ ] JD analysis provider failure starts fallback interview.
- [ ] First question provider failure starts fallback interview.
- [ ] Answer evaluation provider failure returns fallback scorecard.
- [ ] Next question provider failure returns fallback follow-up.
- [ ] Final feedback provider failure returns friendly degraded feedback or a recoverable retry state.

---

## Master Acceptance Checklist

### Product Value

- [ ] A user receives a scorecard after every answer.
- [ ] A weak answer triggers retry instead of silently moving on.
- [ ] User sees exact evidence from their own answer.
- [ ] User sees a better professional English answer.
- [ ] User can add mistakes to notebook.
- [ ] User can add phrases to review.
- [ ] User sees readiness by interview capability.
- [ ] User can follow a 7-day plan generated from CV/JD.

### Architecture

- [ ] Retry/pass decision lives in domain/application code, not in prompt text.
- [ ] Provider adapter only generates normalized evidence.
- [ ] Controllers remain thin.
- [ ] All mutations go through use cases.
- [ ] Contracts are explicit and mirrored in frontend types.
- [ ] Migrations exist for schema changes.

### Testing

- [ ] Domain tests cover retry policy.
- [ ] Use-case tests cover accepted answer, retry answer, provider fallback, max attempts.
- [ ] API tests cover response shapes and failure handling.
- [ ] Frontend typecheck passes.
- [ ] Frontend lint passes.
- [ ] Full backend solution test passes.

### Manual QA

- [ ] Upload or reuse CV.
- [ ] Paste JD.
- [ ] Start interview.
- [ ] Submit intentionally weak answer.
- [ ] Confirm retry panel appears.
- [ ] Submit improved answer.
- [ ] Confirm next question appears.
- [ ] Promote a mistake.
- [ ] Confirm notebook contains it.
- [ ] Promote a phrase.
- [ ] Confirm review queue contains it.
- [ ] Finish interview.
- [ ] Confirm readiness map shows weak/ready dimensions.

## Suggested Execution Order

1. Task 1
2. Task 2
3. Task 3
4. Task 4
5. Task 5
6. Task 6
7. Task 7
8. Task 8
9. Task 9
10. Task 10
11. Task 11
12. Task 12
13. Task 13
14. Task 14

Stop after Task 6 for the first usable product checkpoint. At that point the app already becomes meaningfully better than a ChatGPT prompt because it has scorecard, evidence, and retry gating.

## Verification Bundle

Run after each backend-heavy checkpoint:

```powershell
node apps/api/scripts/run-dotnet.mjs test apps/api/EnglishCoach.sln
node apps/api/scripts/run-dotnet.mjs build apps/api/EnglishCoach.sln
```

Run after each frontend-heavy checkpoint:

```powershell
pnpm --dir apps/web typecheck
pnpm --dir apps/web lint
pnpm --dir apps/web build
```

Run before claiming the whole plan is done:

```powershell
node apps/api/scripts/run-dotnet.mjs test apps/api/EnglishCoach.sln
node apps/api/scripts/run-dotnet.mjs build apps/api/EnglishCoach.sln
pnpm --dir apps/web typecheck
pnpm --dir apps/web lint
pnpm --dir apps/web build
```
