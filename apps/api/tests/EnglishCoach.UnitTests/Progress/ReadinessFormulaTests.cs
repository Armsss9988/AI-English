using EnglishCoach.Domain.Progress;

namespace EnglishCoach.UnitTests.Progress;

public class ReadinessFormulaTests
{
    private readonly Guid _learnerId = Guid.NewGuid();

    [Fact]
    public void Calculate_PerfectScores_Returns100()
    {
        var components = new ReadinessComponents(1.0m, 1.0m, 1.0m, 1.0m, 0m, 1.0m);
        var score = ReadinessFormula.Calculate(_learnerId, components);

        Assert.Equal(100m, score.Score);
        Assert.Equal(ReadinessFormula.ReadinessFormulaVersion, score.FormulaVersion);
        Assert.Equal(6, score.Components.Count);
    }

    [Fact]
    public void Calculate_ZeroScores_Returns0()
    {
        var components = new ReadinessComponents(0m, 0m, 0m, 0m, 10m, 0m);
        var score = ReadinessFormula.Calculate(_learnerId, components);

        Assert.Equal(0m, score.Score);
    }

    [Fact]
    public void Calculate_PartialScores_ReturnsBetween0And100()
    {
        var components = new ReadinessComponents(0.8m, 0.7m, 0.9m, 0.6m, 2m, 0.5m);
        var score = ReadinessFormula.Calculate(_learnerId, components);

        Assert.True(score.Score > 0);
        Assert.True(score.Score < 100);
        Assert.Equal(6, score.Components.Count);
    }

    [Fact]
    public void Calculate_ClampsScoreBetween0And100()
    {
        var components = new ReadinessComponents(1.5m, 1.5m, 1.5m, 1.5m, -5m, 1.5m);
        var score = ReadinessFormula.Calculate(_learnerId, components);

        Assert.True(score.Score >= 0m);
        Assert.True(score.Score <= 100m);
    }

    [Fact]
    public void Calculate_IsDeterministic()
    {
        var components = new ReadinessComponents(0.75m, 0.80m, 0.65m, 0.70m, 3m, 0.60m);

        var score1 = ReadinessFormula.Calculate(_learnerId, components);
        var score2 = ReadinessFormula.Calculate(_learnerId, components);

        Assert.Equal(score1.Score, score2.Score);
        Assert.Equal(score1.FormulaVersion, score2.FormulaVersion);
    }

    [Fact]
    public void Calculate_RecordsAllComponents()
    {
        var components = new ReadinessComponents(0.5m, 0.5m, 0.5m, 0.5m, 5m, 0.5m);
        var score = ReadinessFormula.Calculate(_learnerId, components);

        var names = score.Components.Select(c => c.Name).ToList();
        Assert.Contains("due_review_completion_rate", names);
        Assert.Contains("phrase_mastery_average", names);
        Assert.Contains("speaking_task_completion_rate", names);
        Assert.Contains("roleplay_success_rate", names);
        Assert.Contains("critical_error_count", names);
        Assert.Contains("retry_success_rate", names);
    }

    [Fact]
    public void Calculate_EachComponentHasWeightAndExplanation()
    {
        var components = new ReadinessComponents(0.5m, 0.5m, 0.5m, 0.5m, 5m, 0.5m);
        var score = ReadinessFormula.Calculate(_learnerId, components);

        foreach (var c in score.Components)
        {
            Assert.True(c.Weight > 0);
            Assert.True(c.WeightedValue >= 0);
            Assert.False(string.IsNullOrEmpty(c.Explanation));
        }
    }

    [Fact]
    public void Calculate_WeightsSumTo100()
    {
        var components = new ReadinessComponents(1m, 1m, 1m, 1m, 0m, 1m);
        var score = ReadinessFormula.Calculate(_learnerId, components);

        var totalWeighted = score.Components.Sum(c => c.WeightedValue);
        Assert.Equal(100m, totalWeighted);
    }
}
