using EnglishCoach.Domain.Progress;
using FluentAssertions;
using Xunit;

namespace EnglishCoach.UnitTests.Progress;

public class CapabilityMatrixTests
{
    [Fact]
    public void Evaluate_WithNoActivity_ShouldReturnAllNotStarted()
    {
        // Arrange
        var learnerData = new List<LearnerProgressData>
        {
            new(Guid.NewGuid(), 0m, 0, Array.Empty<string>())
        };
        var matrix = new CapabilityMatrix(learnerData);

        // Act
        var capabilities = matrix.Evaluate();

        // Assert
        capabilities.Should().HaveCount(6);
        capabilities.Should().OnlyContain(c => c.Status == CapabilityStatus.NotStarted);
    }

    [Fact]
    public void Evaluate_WithSufficientProgress_ShouldReturnAchieved()
    {
        // Arrange
        var learnerData = new List<LearnerProgressData>
        {
            new(
                Guid.NewGuid(),
                AveragePhraseMastery: 0.8m,
                CriticalErrorCount: 1,
                RoleplayScenariosCompleted: new List<string>
                {
                    "standup daily", "standup blocked", "standup async", // 3 standup
                    "clarify requirements", "clarify scope" // 2 clarification
                }
            )
        };
        var matrix = new CapabilityMatrix(learnerData);

        // Act
        var capabilities = matrix.Evaluate();

        // Assert
        var dailyUpdate = capabilities.First(c => c.Name == CapabilityName.CanGiveDailyUpdate);
        dailyUpdate.Status.Should().Be(CapabilityStatus.Achieved);

        var clarification = capabilities.First(c => c.Name == CapabilityName.CanAskClarification);
        clarification.Status.Should().Be(CapabilityStatus.Achieved);
    }

    [Fact]
    public void Evaluate_WithPartialProgress_ShouldReturnInProgress()
    {
        // Arrange
        var learnerData = new List<LearnerProgressData>
        {
            new(
                Guid.NewGuid(),
                AveragePhraseMastery: 0.5m,
                CriticalErrorCount: 2,
                RoleplayScenariosCompleted: new List<string>
                {
                    "standup daily" // 1 standup (needs 3)
                }
            )
        };
        var matrix = new CapabilityMatrix(learnerData);

        // Act
        var capabilities = matrix.Evaluate();

        // Assert
        var dailyUpdate = capabilities.First(c => c.Name == CapabilityName.CanGiveDailyUpdate);
        dailyUpdate.Status.Should().Be(CapabilityStatus.InProgress);
        dailyUpdate.Evidence.Should().NotBeEmpty();
    }

    [Fact]
    public void Evaluate_ShouldMapToCorrectCriteria()
    {
        // Arrange
        var learnerData = new List<LearnerProgressData>
        {
            new(Guid.NewGuid(), 0.9m, 0, new[] { "issue critical", "issue bug", "issue production" })
        };
        var matrix = new CapabilityMatrix(learnerData);

        // Act
        var capabilities = matrix.Evaluate();

        // Assert
        var explainBug = capabilities.First(c => c.Name == CapabilityName.CanExplainBug);
        explainBug.Status.Should().Be(CapabilityStatus.Achieved);
    }

    [Fact]
    public void Evaluate_ShouldIncludeExplanation()
    {
        // Arrange
        var learnerData = new List<LearnerProgressData>
        {
            new(Guid.NewGuid(), 0.5m, 5, Array.Empty<string>())
        };
        var matrix = new CapabilityMatrix(learnerData);

        // Act
        var capabilities = matrix.Evaluate();

        // Assert
        foreach (var capability in capabilities)
        {
            capability.Explanation.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void Evaluate_ShouldReturnAllSixCapabilities()
    {
        // Arrange
        var matrix = new CapabilityMatrix(new List<LearnerProgressData> { new(Guid.NewGuid(), 0m, 0, Array.Empty<string>()) });

        // Act
        var capabilities = matrix.Evaluate();

        // Assert
        capabilities.Should().Contain(c => c.Name == CapabilityName.CanGiveDailyUpdate);
        capabilities.Should().Contain(c => c.Name == CapabilityName.CanExplainBug);
        capabilities.Should().Contain(c => c.Name == CapabilityName.CanAskClarification);
        capabilities.Should().Contain(c => c.Name == CapabilityName.CanReportDelay);
        capabilities.Should().Contain(c => c.Name == CapabilityName.CanProposeOptions);
        capabilities.Should().Contain(c => c.Name == CapabilityName.CanSummarizeNextSteps);
    }

    [Fact]
    public void Evaluate_CriticalErrorsExceedThreshold_ShouldNotAchieveCapability()
    {
        // Arrange
        var learnerData = new List<LearnerProgressData>
        {
            new(
                Guid.NewGuid(),
                AveragePhraseMastery: 0.9m,
                CriticalErrorCount: 10, // Exceeds all thresholds
                RoleplayScenariosCompleted: new[] { "standup", "issue", "clarify", "eta", "summary", "standup", "issue", "clarify", "eta", "summary" }
            )
        };
        var matrix = new CapabilityMatrix(learnerData);

        // Act
        var capabilities = matrix.Evaluate();

        // Assert
        capabilities.Should().NotContain(c => c.Status == CapabilityStatus.Achieved);
    }
}