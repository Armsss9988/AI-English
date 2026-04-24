namespace EnglishCoach.Domain.Progress;

public enum CapabilityStatus
{
    Achieved,
    InProgress,
    NotStarted
}

public record CapabilityAssessment(
    CapabilityName Name,
    CapabilityStatus Status,
    string Explanation,
    IReadOnlyList<string> Evidence
);

public record LearnerProgressData(
    Guid LearnerId,
    decimal AveragePhraseMastery,
    int CriticalErrorCount,
    IReadOnlyList<string> RoleplayScenariosCompleted
);

public class CapabilityMatrix
{
    private readonly IReadOnlyList<LearnerProgressData> _learnerData;

    public CapabilityMatrix(IReadOnlyList<LearnerProgressData> learnerData)
    {
        _learnerData = learnerData;
    }

    public IReadOnlyList<CapabilityAssessment> Evaluate()
    {
        var assessments = new List<CapabilityAssessment>();

        foreach (var criteria in CanDoCriteria.All)
        {
            var assessment = EvaluateCapability(criteria);
            assessments.Add(assessment);
        }

        return assessments;
    }

    private CapabilityAssessment EvaluateCapability(CapabilityCriteria criteria)
    {
        var evidence = new List<string>();
        var requirementsMet = 0;

        // Check roleplay scenarios completed in this category
        var roleplayCount = _learnerData.Sum(d =>
            d.RoleplayScenariosCompleted.Count(r => MatchesCapability(r, criteria.Name)));
        if (roleplayCount >= criteria.RequiredRoleplayScenarios)
        {
            requirementsMet++;
            evidence.Add($"Completed {roleplayCount} relevant roleplay scenarios");
        }
        else
        {
            evidence.Add($"Need {criteria.RequiredRoleplayScenarios - roleplayCount} more relevant roleplay scenarios (have {roleplayCount})");
        }

        // Check phrase mastery
        if (_learnerData.Any(d => d.AveragePhraseMastery >= criteria.RequiredPhraseMastery))
        {
            requirementsMet++;
            var avg = _learnerData.Average(d => d.AveragePhraseMastery);
            evidence.Add($"Average phrase mastery: {avg:P0}");
        }
        else
        {
            var avg = _learnerData.Average(d => d.AveragePhraseMastery);
            evidence.Add($"Phrase mastery {avg:P0} below threshold {criteria.RequiredPhraseMastery}");
        }

        // Check critical errors
        if (_learnerData.Sum(d => d.CriticalErrorCount) <= criteria.MaxCriticalErrors)
        {
            requirementsMet++;
            evidence.Add("Critical errors within acceptable range");
        }
        else
        {
            var count = _learnerData.Sum(d => d.CriticalErrorCount);
            evidence.Add($"Critical errors ({count}) exceed threshold ({criteria.MaxCriticalErrors})");
        }

        var status = requirementsMet switch
        {
            3 => CapabilityStatus.Achieved,
            > 0 => CapabilityStatus.InProgress,
            _ => CapabilityStatus.NotStarted
        };

        return new CapabilityAssessment(
            criteria.Name,
            status,
            criteria.Description,
            evidence
        );
    }

    private static bool MatchesCapability(string scenarioName, CapabilityName capability)
    {
        return capability switch
        {
            CapabilityName.CanGiveDailyUpdate =>
                scenarioName.Contains("standup", StringComparison.OrdinalIgnoreCase),
            CapabilityName.CanExplainBug =>
                scenarioName.Contains("issue", StringComparison.OrdinalIgnoreCase) ||
                scenarioName.Contains("bug", StringComparison.OrdinalIgnoreCase),
            CapabilityName.CanAskClarification =>
                scenarioName.Contains("clarif", StringComparison.OrdinalIgnoreCase),
            CapabilityName.CanReportDelay =>
                scenarioName.Contains("eta", StringComparison.OrdinalIgnoreCase) ||
                scenarioName.Contains("delay", StringComparison.OrdinalIgnoreCase),
            CapabilityName.CanProposeOptions =>
                scenarioName.Contains("option", StringComparison.OrdinalIgnoreCase) ||
                scenarioName.Contains("propos", StringComparison.OrdinalIgnoreCase),
            CapabilityName.CanSummarizeNextSteps =>
                scenarioName.Contains("summary", StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }
}
