"use client";

import React, { useEffect, useRef, useState } from "react";
import { useMutation } from "@tanstack/react-query";
import { getLatestCvProfile, uploadCv, uploadCvFile } from "@/lib/api/interview";
import styles from "./interview.module.css";

interface InterviewSetupProps {
  onReady: (profileId: string, jdText: string, interviewType: string) => void;
}

const INTERVIEW_TYPES = [
  {
    key: "Mixed",
    title: "Mixed",
    desc: "Behavioral + Technical questions",
  },
  {
    key: "Situational",
    title: "Situational",
    desc: "\"What would you do if...\" scenarios",
  },
  {
    key: "Behavioral",
    title: "Behavioral",
    desc: "\"Tell me about a time when...\"",
  },
  {
    key: "Technical",
    title: "Technical",
    desc: "System design & coding questions",
  },
];

export const InterviewSetup: React.FC<InterviewSetupProps> = ({ onReady }) => {
  const [step, setStep] = useState(1);
  const [cvText, setCvText] = useState("");
  const [jdText, setJdText] = useState("");
  const [interviewType, setInterviewType] = useState("Mixed");
  const [profileId, setProfileId] = useState<string | null>(null);
  const [cvAnalysis, setCvAnalysis] = useState<string | null>(null);
  const [cvFile, setCvFile] = useState<File | null>(null);
  const fileInputRef = useRef<HTMLInputElement | null>(null);

  useEffect(() => {
    let isMounted = true;

    getLatestCvProfile()
      .then((profile) => {
        if (!isMounted || !profile) return;

        setProfileId(profile.profileId);
        setCvAnalysis(profile.cvAnalysis);
        setStep(2);
      })
      .catch(() => {
        // Latest CV is an optional convenience; manual upload/paste still works.
      });

    return () => {
      isMounted = false;
    };
  }, []);

  const cvMutation = useMutation({
    mutationFn: () => (cvFile ? uploadCvFile(cvFile) : uploadCv(cvText)),
    onSuccess: (res) => {
      setProfileId(res.profileId);
      setCvAnalysis(res.cvAnalysis);
      setStep(2);
    },
  });

  const cvErrorMessage =
    cvMutation.error instanceof Error
      ? cvMutation.error.message
      : "CV analysis failed. Check the API and AI provider configuration.";

  const canAnalyzeCv = Boolean(cvFile) || Boolean(cvText.trim());

  const handleCvFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0] ?? null;
    setCvFile(file);
    if (cvMutation.isError) cvMutation.reset();
  };

  const handleRemoveCvFile = () => {
    setCvFile(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
    if (cvMutation.isError) cvMutation.reset();
  };

  const handleCvSubmit = () => {
    if (!canAnalyzeCv) return;
    cvMutation.mutate();
  };

  const handleStartInterview = () => {
    if (!profileId || !jdText.trim()) return;
    onReady(profileId, jdText, interviewType);
  };

  return (
    <div className={styles.setupContainer}>
      <h1 className={styles.setupTitle}>🎤 Mock Interview Practice</h1>
      <p className={styles.setupSubtitle}>
        Upload your CV and JD to start a personalized interview simulation
      </p>

      <div className={styles.stepIndicator}>
        <div
          className={`${styles.step} ${step >= 1 ? styles.active : ""} ${step > 1 ? styles.completed : ""}`}
        />
        <div
          className={`${styles.step} ${step >= 2 ? styles.active : ""} ${step > 2 ? styles.completed : ""}`}
        />
        <div className={`${styles.step} ${step >= 3 ? styles.active : ""}`} />
      </div>

      {step === 1 && (
        <div className={styles.formSection}>
          <div className={styles.sectionLabel}>📄 Step 1: Your CV / Resume</div>
          <div className={styles.uploadBox}>
            <input
              ref={fileInputRef}
              className={styles.fileInput}
              type="file"
              accept=".pdf,application/pdf"
              onChange={handleCvFileChange}
              disabled={cvMutation.isPending}
            />
            {cvFile ? (
              <div className={styles.selectedFile}>
                <span>{cvFile.name}</span>
                <button
                  type="button"
                  className={styles.removeFileBtn}
                  onClick={handleRemoveCvFile}
                  disabled={cvMutation.isPending}
                >
                  Remove
                </button>
              </div>
            ) : (
              <div className={styles.uploadHint}>
                Upload a text-based PDF CV, or paste your CV below.
              </div>
            )}
          </div>
          <textarea
            className={styles.textarea}
            placeholder="Paste your CV content here...&#10;&#10;Example:&#10;Nguyen Van A — Full-stack Developer&#10;3 years experience with C#, ASP.NET Core, React, PostgreSQL..."
            value={cvText}
            onChange={(e) => {
              setCvText(e.target.value);
              if (cvMutation.isError) cvMutation.reset();
            }}
            disabled={cvMutation.isPending || Boolean(cvFile)}
          />
          {cvMutation.isError && (
            <div
              className={styles.errorAlert}
              data-testid="cv-analysis-error"
              role="alert"
            >
              <div className={styles.errorTitle}>CV analysis failed</div>
              <div>{cvErrorMessage}</div>
            </div>
          )}
          <div className={styles.buttonRow}>
            <button
              className={styles.primaryBtn}
              onClick={handleCvSubmit}
              disabled={!canAnalyzeCv || cvMutation.isPending}
            >
              {cvMutation.isPending ? (
                <>
                  <span className={styles.spinner} /> Analyzing CV...
                </>
              ) : (
                "Analyze CV →"
              )}
            </button>
          </div>
        </div>
      )}

      {step === 2 && (
        <>
          {cvAnalysis && (
            <div className={styles.formSection}>
              <div className={styles.sectionLabel}>✅ CV Analysis Complete</div>
              <div className={styles.analysisResult}>
                <pre style={{ whiteSpace: "pre-wrap", margin: 0, fontFamily: "inherit" }}>
                  {cvAnalysis}
                </pre>
              </div>
            </div>
          )}

          <div className={styles.formSection}>
            <div className={styles.sectionLabel}>📋 Step 2: Job Description</div>
            <textarea
              className={styles.textarea}
              placeholder="Paste the Job Description here...&#10;&#10;Example:&#10;We're looking for a Mid-Senior Full-stack Developer to join our international team..."
              value={jdText}
              onChange={(e) => setJdText(e.target.value)}
            />
          </div>

          <div className={styles.formSection}>
            <div className={styles.sectionLabel}>🎯 Step 3: Interview Type</div>
            <div className={styles.typeSelector}>
              {INTERVIEW_TYPES.map((type) => (
                <div
                  key={type.key}
                  className={`${styles.typeCard} ${interviewType === type.key ? styles.selected : ""}`}
                  onClick={() => setInterviewType(type.key)}
                >
                  <div className={styles.typeCardTitle}>{type.title}</div>
                  <div className={styles.typeCardDesc}>{type.desc}</div>
                </div>
              ))}
            </div>
          </div>

          <div className={styles.buttonRow}>
            <button
              className={styles.secondaryBtn}
              onClick={() => setStep(1)}
            >
              ← Back
            </button>
            <button
              className={styles.primaryBtn}
              onClick={handleStartInterview}
              disabled={!jdText.trim()}
            >
              🚀 Start Mock Interview
            </button>
          </div>
        </>
      )}
    </div>
  );
};
