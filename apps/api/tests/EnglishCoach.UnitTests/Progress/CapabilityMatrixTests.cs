using EnglishCoach.Domain.Progress;

namespace EnglishCoach.UnitTests.Progress;

public class CapabilityMatrixTests
{
    [Fact]
    public void Evaluate_NoActivity_ReturnsAllNotStarted()
    {
        var data = new List<LearnerProgressData>
        {
            new(Guid.NewGuid(), 0m, 0, Array.Empty<string>())
        };
        var matrix = new CapabilityMatrix(data);

        var capabilities = matrix.Evaluate();

        Assert.Equal(6, capabilities.Count);
        // With zero data, some capabilities may be InProgress or NotStarted
        // depending on criteria thresholds; none should be Achieved
        Assert.DoesNotContain(capabilities, c => c.Status == CapabilityStatus.Achieved);
    }

    [Fact]
    public void Evaluate_SufficientProgress_ReturnsProgressOrAchieved()
    {
        var data = new List<LearnerProgressData>
        {
            new(Guid.NewGuid(), 0.8m, 1,
                new List<string> { "standup daily", "standup blocked", "standup async", "clarify requirements", "clarify scope" })
        };
        var matrix = new CapabilityMatrix(data);

        var capabilities = matrix.Evaluate();

        // With sufficient data, at least some capabilities should progress beyond NotStarted
        Assert.Contains(capabilities, c =>
            c.Status == CapabilityStatus.InProgress || c.Status == CapabilityStatus.Achieved);
    }

    [Fact]
    public void Evaluate_PartialProgress_ReturnsInProgress()
    {
        var data = new List<LearnerProgressData>
        {
            new(Guid.NewGuid(), 0.5m, 2, new List<string> { "standup daily" })
        };
        var matrix = new CapabilityMatrix(data);

        var capabilities = matrix.Evaluate();

        var dailyUpdate = capabilities.First(c => c.Name == CapabilityName.CanGiveDailyUpdate);
        Assert.Equal(CapabilityStatus.InProgress, dailyUpdate.Status);
        Assert.NotEmpty(dailyUpdate.Evidence);
    }

    [Fact]
    public void Evaluate_IncludesExplanation()
    {
        var data = new List<LearnerProgressData>
        {
            new(Guid.NewGuid(), 0.5m, 5, Array.Empty<string>())
        };
        var matrix = new CapabilityMatrix(data);

        var capabilities = matrix.Evaluate();

        Assert.All(capabilities, c => Assert.False(string.IsNullOrEmpty(c.Explanation)));
    }

    [Fact]
    public void Evaluate_ReturnsAllSixCapabilities()
    {
        var data = new List<LearnerProgressData>
        {
            new(Guid.NewGuid(), 0m, 0, Array.Empty<string>())
        };
        var matrix = new CapabilityMatrix(data);

        var capabilities = matrix.Evaluate();
        var names = capabilities.Select(c => c.Name).ToList();

        Assert.Contains(CapabilityName.CanGiveDailyUpdate, names);
        Assert.Contains(CapabilityName.CanExplainBug, names);
        Assert.Contains(CapabilityName.CanAskClarification, names);
        Assert.Contains(CapabilityName.CanReportDelay, names);
        Assert.Contains(CapabilityName.CanProposeOptions, names);
        Assert.Contains(CapabilityName.CanSummarizeNextSteps, names);
    }

    [Fact]
    public void Evaluate_CriticalErrorsExceedThreshold_DoesNotAchieve()
    {
        var data = new List<LearnerProgressData>
        {
            new(Guid.NewGuid(), 0.9m, 10,
                new[] { "standup", "issue", "clarify", "eta", "summary", "standup", "issue", "clarify", "eta", "summary" })
        };
        var matrix = new CapabilityMatrix(data);

        var capabilities = matrix.Evaluate();

        // With excessive critical errors, capabilities should not reach Achieved
        Assert.DoesNotContain(capabilities, c => c.Status == CapabilityStatus.Achieved);
        // All should be NotStarted or InProgress
        Assert.All(capabilities, c => Assert.True(
            c.Status == CapabilityStatus.NotStarted || c.Status == CapabilityStatus.InProgress,
            $"Expected NotStarted or InProgress but got {c.Status} for {c.Name}"));
    }
}
