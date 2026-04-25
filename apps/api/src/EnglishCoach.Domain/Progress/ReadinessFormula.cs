namespace EnglishCoach.Domain.Progress;


public record ReadinessComponents(
    decimal ReviewCompletionRate,
    decimal PhraseMasteryAverage,
    decimal SpeakingTaskCompletionRate,
    decimal RoleplaySuccessRate,
    decimal CriticalErrorCount,
    decimal RetrySuccessRate
);

public record ReadinessScore(
    Guid LearnerId,
    decimal Score,
    int FormulaVersion,
    IReadOnlyList<ReadinessComponent> Components,
    DateTimeOffset CalculatedAt
);

public record ReadinessComponent(
    string Name,
    decimal RawValue,
    decimal Weight,
    decimal WeightedValue,
    string Explanation
);

public static class ReadinessFormula
{
    public const int ReadinessFormulaVersion = 1;

    // Weights must sum to 1.0
    private const decimal ReviewCompletionWeight = 0.25m;
    private const decimal PhraseMasteryWeight = 0.20m;
    private const decimal SpeakingCompletionWeight = 0.20m;
    private const decimal RoleplaySuccessWeight = 0.20m;
    private const decimal CriticalErrorWeight = 0.10m;
    private const decimal RetrySuccessWeight = 0.05m;

    public static ReadinessScore Calculate(Guid learnerId, ReadinessComponents components)
    {
        var readinessComponents = new List<ReadinessComponent>
        {
            CalculateReviewCompletion(components.ReviewCompletionRate),
            CalculatePhraseMastery(components.PhraseMasteryAverage),
            CalculateSpeakingCompletion(components.SpeakingTaskCompletionRate),
            CalculateRoleplaySuccess(components.RoleplaySuccessRate),
            CalculateCriticalErrors(components.CriticalErrorCount),
            CalculateRetrySuccess(components.RetrySuccessRate)
        };

        var totalScore = readinessComponents.Sum(c => c.WeightedValue);

        // Clamp score between 0 and 100
        totalScore = Math.Clamp(totalScore, 0m, 100m);

        return new ReadinessScore(
            learnerId,
            Math.Round(totalScore, 1),
            ReadinessFormulaVersion,
            readinessComponents,
            DateTimeOffset.UtcNow
        );
    }

    private static ReadinessComponent CalculateReviewCompletion(decimal rate)
    {
        var weighted = rate * ReviewCompletionWeight * 100;
        return new ReadinessComponent(
            "due_review_completion_rate",
            rate,
            ReviewCompletionWeight,
            weighted,
            $"Completed {rate:P0} of due reviews (weight: {ReviewCompletionWeight:P0})"
        );
    }

    private static ReadinessComponent CalculatePhraseMastery(decimal average)
    {
        var weighted = average * PhraseMasteryWeight * 100;
        return new ReadinessComponent(
            "phrase_mastery_average",
            average,
            PhraseMasteryWeight,
            weighted,
            $"Average phrase mastery: {average:P0} (weight: {PhraseMasteryWeight:P0})"
        );
    }

    private static ReadinessComponent CalculateSpeakingCompletion(decimal rate)
    {
        var weighted = rate * SpeakingCompletionWeight * 100;
        return new ReadinessComponent(
            "speaking_task_completion_rate",
            rate,
            SpeakingCompletionWeight,
            weighted,
            $"Completed {rate:P0} of speaking tasks (weight: {SpeakingCompletionWeight:P0})"
        );
    }

    private static ReadinessComponent CalculateRoleplaySuccess(decimal rate)
    {
        var weighted = rate * RoleplaySuccessWeight * 100;
        return new ReadinessComponent(
            "roleplay_success_rate",
            rate,
            RoleplaySuccessWeight,
            weighted,
            $"Passed {rate:P0} of roleplay scenarios (weight: {RoleplaySuccessWeight:P0})"
        );
    }

    private static ReadinessComponent CalculateCriticalErrors(decimal count)
    {
        // Fewer errors = higher score. Max errors considered is 10.
        var normalized = Math.Max(0, 1 - (count / 10m));
        var weighted = normalized * CriticalErrorWeight * 100;
        return new ReadinessComponent(
            "critical_error_count",
            count,
            CriticalErrorWeight,
            weighted,
            $"{count} critical errors in recent period (weight: {CriticalErrorWeight:P0})"
        );
    }

    private static ReadinessComponent CalculateRetrySuccess(decimal rate)
    {
        var weighted = rate * RetrySuccessWeight * 100;
        return new ReadinessComponent(
            "retry_success_rate",
            rate,
            RetrySuccessWeight,
            weighted,
            $"Succeeded in {rate:P0} of retry tasks (weight: {RetrySuccessWeight:P0})"
        );
    }
}
