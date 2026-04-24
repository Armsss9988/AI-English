using EnglishCoach.Domain.LearningContent;
using EnglishCoach.Infrastructure.Seed;
using FluentAssertions;
using Xunit;

namespace EnglishCoach.UnitTests.LearningContent;

public class CurriculumSeederTests
{
    [Fact]
    public void Seed_ShouldCreateContentItems()
    {
        // Act
        var seeder = CurriculumSeeder.Seed();

        // Assert
        seeder.TotalPhrases.Should().BeGreaterThan(0);
        seeder.TotalScenarios.Should().BeGreaterThan(0);
        seeder.TotalContent.Should().BeGreaterThan(50);
    }

    [Fact]
    public void Seed_ShouldCreatePublishedContent()
    {
        // Act
        var seeder = CurriculumSeeder.Seed();

        // Assert - all content should be published
        seeder.Phrases.Should().OnlyContain(p => p.State == ContentState.Published);
        seeder.Scenarios.Should().OnlyContain(s => s.State == ContentState.Published);
    }

    [Fact]
    public void Seed_ShouldCreatePhrasesInAllCategories()
    {
        // Act
        var seeder = CurriculumSeeder.Seed();

        // Assert
        seeder.PhrasesByCategory.Should().ContainKey("greeting");
        seeder.PhrasesByCategory.Should().ContainKey("standup");
        seeder.PhrasesByCategory.Should().ContainKey("bug");
        seeder.PhrasesByCategory.Should().ContainKey("clarification");
        seeder.PhrasesByCategory.Should().ContainKey("eta");
        seeder.PhrasesByCategory.Should().ContainKey("technical");
    }

    [Fact]
    public void Seed_ShouldCreateScenariosInAllGroups()
    {
        // Act
        var seeder = CurriculumSeeder.Seed();

        // Assert - 5 groups: standup, issue, clarification, eta, summary
        seeder.ScenariosByGroup.Should().ContainKey("standup");
        seeder.ScenariosByGroup.Should().ContainKey("issue");
        seeder.ScenariosByGroup.Should().ContainKey("clarification");
        seeder.ScenariosByGroup.Should().ContainKey("eta");
        seeder.ScenariosByGroup.Should().ContainKey("summary");
    }

    [Fact]
    public void Seed_ShouldHaveAtLeast5ScenariosPerGroup()
    {
        // Act
        var seeder = CurriculumSeeder.Seed();

        // Assert - requirement: at least 5 scenarios per group
        foreach (var group in seeder.ScenariosByGroup)
        {
            group.Value.Should().BeGreaterOrEqualTo(5, $"Group '{group.Key}' should have at least 5 scenarios");
        }
    }

    [Fact]
    public void Seed_ShouldHave100OrMorePhrases()
    {
        // Act
        var seeder = CurriculumSeeder.Seed();

        // Assert - requirement: 100-150 phrase items
        seeder.TotalPhrases.Should().BeGreaterOrEqualTo(100);
        seeder.TotalPhrases.Should().BeLessOrEqualTo(150);
    }

    [Fact]
    public void Seed_ShouldHave20OrMoreScenarios()
    {
        // Act
        var seeder = CurriculumSeeder.Seed();

        // Assert - requirement: 20 roleplay scenarios
        seeder.TotalScenarios.Should().BeGreaterOrEqualTo(20);
    }

    [Fact]
    public void Seed_ShouldHaveIdempotentStructure()
    {
        // Arrange
        var seeder1 = CurriculumSeeder.Seed();
        var seeder2 = CurriculumSeeder.Seed();

        // Assert - both should produce same structure
        seeder1.TotalPhrases.Should().Be(seeder2.TotalPhrases);
        seeder1.TotalScenarios.Should().Be(seeder2.TotalScenarios);
        seeder1.PhrasesByCategory.Keys.Should().BeEquivalentTo(seeder2.PhrasesByCategory.Keys);
    }

    [Fact]
    public void SeededContent_ShouldHaveVersions()
    {
        // Act
        var seeder = CurriculumSeeder.Seed();

        // Assert
        foreach (var phrase in seeder.Phrases)
        {
            phrase.Versions.Should().NotBeEmpty();
            phrase.CurrentVersion.Should().BeGreaterOrEqualTo(1);
        }
    }
}