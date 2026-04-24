"use client";

import React from "react";
import { useForm } from "react-hook-form";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { UpdateProfileRequest, EnglishLevel } from "@english-coach/contracts";
import { Button, Input, Select } from "@english-coach/ui";
import { updateProfile } from "@/lib/api/identity";
import styles from "./onboarding.module.css";

const ROLES: { value: UpdateProfileRequest["role"]; label: string }[] = [
  { value: "dev", label: "Developer (Frontend/Backend/Fullstack)" },
  { value: "qa", label: "QA / Tester / Automation" },
  { value: "ba", label: "Business Analyst" },
  { value: "pm", label: "Project Manager / Product Owner" },
  { value: "support", label: "Technical Support / IT" },
  { value: "other", label: "Other Role" },
];

const ENGLISH_LEVELS: { value: EnglishLevel; label: string }[] = [
  { value: "A1", label: "A1 - Beginner" },
  { value: "A2", label: "A2 - Elementary" },
  { value: "B1", label: "B1 - Intermediate" },
  { value: "B2", label: "B2 - Upper Intermediate" },
  { value: "C1", label: "C1 - Advanced" },
  { value: "C2", label: "C2 - Proficiency" },
];

export const OnboardingForm: React.FC = () => {
  const router = useRouter();
  const queryClient = useQueryClient();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<UpdateProfileRequest>({
    defaultValues: {
      role: "dev",
      timezone: "UTC+7",
      currentLevel: "B1",
      targetUseCase: "",
    },
  });

  const mutation = useMutation({
    mutationFn: updateProfile,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["profile"] });
      router.push("/today");
    },
  });

  const onSubmit = (data: UpdateProfileRequest) => {
    mutation.mutate(data);
  };

  return (
    <div className={styles.formCard}>
      <div className={styles.header}>
        <h1 className={styles.title}>Welcome to English Coach</h1>
        <p className={styles.subtitle}>
          Let&apos;s personalize your learning experience
        </p>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className={styles.form}>
        <Select
          label="Professional Role"
          options={ROLES}
          error={errors.role?.message}
          {...register("role", { required: "Role is required" })}
        />

        <div className={styles.row}>
          <Select
            label="Current English Level"
            options={ENGLISH_LEVELS}
            error={errors.currentLevel?.message}
            {...register("currentLevel", { required: "Level is required" })}
          />

          <Input
            label="Timezone"
            placeholder="e.g. UTC+7"
            error={errors.timezone?.message}
            {...register("timezone", { required: "Timezone is required" })}
          />
        </div>

        <Input
          label="Primary Learning Goal"
          placeholder="e.g. Pitching to US clients, Technical reviews..."
          error={errors.targetUseCase?.message}
          {...register("targetUseCase", { required: "Goal is required" })}
        />

        {mutation.isError && (
          <div className={styles.errorBanner}>
            {(mutation.error as Error).message}
          </div>
        )}

        {mutation.isSuccess && (
          <div className={styles.successBanner}>
            Profile saved successfully!
          </div>
        )}

        <Button
          type="submit"
          size="lg"
          isLoading={mutation.isPending}
          className={styles.submitBtn}
        >
          Start Learning
        </Button>
      </form>
    </div>
  );
};
