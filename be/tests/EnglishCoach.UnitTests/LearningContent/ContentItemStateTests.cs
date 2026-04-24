using EnglishCoach.Domain.LearningContent;
using FluentAssertions;
using Xunit;

namespace EnglishCoach.UnitTests.LearningContent;

public class ContentItemStateTests
{
    [Fact]
    public void Create_NewPhrase_ShouldHaveDraftState()
    {
        // Arrange & Act
        var phrase = ContentItem.CreatePhrase("Hello", "greeting", "Used when meeting someone");

        // Assert
        phrase.State.Should().Be(ContentState.Draft);
        phrase.IsEditable.Should().BeTrue();
        phrase.IsPublished.Should().BeFalse();
    }

    [Theory]
    [InlineData(ContentState.Draft, ContentState.Review, true)]
    [InlineData(ContentState.Review, ContentState.Draft, true)]
    [InlineData(ContentState.Review, ContentState.Published, true)]
    [InlineData(ContentState.Published, ContentState.Deprecated, true)]
    [InlineData(ContentState.Deprecated, ContentState.Archived, true)]
    [InlineData(ContentState.Draft, ContentState.Published, false)]
    [InlineData(ContentState.Draft, ContentState.Archived, false)]
    [InlineData(ContentState.Published, ContentState.Draft, false)]
    [InlineData(ContentState.Published, ContentState.Archived, false)]
    [InlineData(ContentState.Archived, ContentState.Draft, false)]
    [InlineData(ContentState.Archived, ContentState.Review, false)]
    public void CanTransition_ShouldReturnExpectedResult(ContentState from, ContentState to, bool expected)
    {
        // Act
        var result = ContentStateTransitions.CanTransition(from, to);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void SubmitForReview_FromDraft_ShouldTransitionToReview()
    {
        // Arrange
        var phrase = ContentItem.CreatePhrase("Hello", "greeting", "Used when meeting");

        // Act
        phrase.SubmitForReview();

        // Assert
        phrase.State.Should().Be(ContentState.Review);
    }

    [Fact]
    public void Publish_FromReview_ShouldTransitionToPublished()
    {
        // Arrange
        var phrase = ContentItem.CreatePhrase("Hello", "greeting", "Used when meeting");
        phrase.SubmitForReview();

        // Act
        phrase.Publish();

        // Assert
        phrase.State.Should().Be(ContentState.Published);
        phrase.IsPublished.Should().BeTrue();
    }

    [Fact]
    public void ReturnToDraft_FromReview_ShouldTransitionToDraft()
    {
        // Arrange
        var phrase = ContentItem.CreatePhrase("Hello", "greeting", "Used when meeting");
        phrase.SubmitForReview();

        // Act
        phrase.ReturnToDraft();

        // Assert
        phrase.State.Should().Be(ContentState.Draft);
    }

    [Fact]
    public void Deprecate_FromPublished_ShouldTransitionToDeprecated()
    {
        // Arrange
        var phrase = ContentItem.CreatePhrase("Hello", "greeting", "Used when meeting");
        phrase.SubmitForReview();
        phrase.Publish();

        // Act
        phrase.Deprecate();

        // Assert
        phrase.State.Should().Be(ContentState.Deprecated);
    }

    [Fact]
    public void Archive_FromDeprecated_ShouldTransitionToArchived()
    {
        // Arrange
        var phrase = ContentItem.CreatePhrase("Hello", "greeting", "Used when meeting");
        phrase.SubmitForReview();
        phrase.Publish();
        phrase.Deprecate();

        // Act
        phrase.Archive();

        // Assert
        phrase.State.Should().Be(ContentState.Archived);
    }

    [Fact]
    public void SubmitForReview_FromPublished_ShouldThrow()
    {
        // Arrange
        var phrase = ContentItem.CreatePhrase("Hello", "greeting", "Used when meeting");
        phrase.SubmitForReview();
        phrase.Publish();

        // Act & Assert
        var act = () => phrase.SubmitForReview();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Invalid state transition*");
    }

    [Fact]
    public void Publish_FromDraft_ShouldThrow()
    {
        // Arrange
        var phrase = ContentItem.CreatePhrase("Hello", "greeting", "Used when meeting");

        // Act & Assert
        var act = () => phrase.Publish();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Can only publish from Review state*");
    }

    [Fact]
    public void CreateNewVersion_WhenPublished_ShouldThrow()
    {
        // Arrange
        var phrase = ContentItem.CreatePhrase("Hello", "greeting", "Used when meeting");
        phrase.SubmitForReview();
        phrase.Publish();

        // Act & Assert
        var act = () => phrase.CreateNewVersion("Updated content");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot edit in place*");
    }

    [Fact]
    public void CreateNewPublishedVersion_WhenPublished_ShouldSucceed()
    {
        // Arrange
        var phrase = ContentItem.CreatePhrase("Hello", "greeting", "Used when meeting");
        phrase.SubmitForReview();
        phrase.Publish();

        // Act
        var newVersion = phrase.CreateNewPublishedVersion("Hello v2", "Updated greeting phrase");

        // Assert
        newVersion.Should().NotBeNull();
        newVersion.VersionNumber.Should().Be(2);
        phrase.CurrentVersion.Should().Be(2);
    }

    [Fact]
    public void CreateNewVersion_WhenDraft_ShouldSucceed()
    {
        // Arrange
        var phrase = ContentItem.CreatePhrase("Hello", "greeting", "Used when meeting");

        // Act
        var newVersion = phrase.CreateNewVersion("Updated greeting", "greeting", "Updated example");

        // Assert
        newVersion.Should().NotBeNull();
        newVersion.VersionNumber.Should().Be(2);
        phrase.CurrentVersion.Should().Be(2);
    }

    [Fact]
    public void CreateNewVersion_WhenArchived_ShouldThrow()
    {
        // Arrange
        var phrase = ContentItem.CreatePhrase("Hello", "greeting", "Used when meeting");
        phrase.SubmitForReview();
        phrase.Publish();
        phrase.Deprecate();
        phrase.Archive();

        // Act & Assert
        var act = () => phrase.CreateNewVersion("Updated content");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Archived content cannot be modified*");
    }

    [Fact]
    public void IsEditable_WhenDraftOrReview_ShouldReturnTrue()
    {
        // Arrange
        var phrase = ContentItem.CreatePhrase("Hello", "greeting", "Used");

        // Assert
        phrase.IsEditable.Should().BeTrue();

        // Act
        phrase.SubmitForReview();

        // Assert
        phrase.IsEditable.Should().BeTrue();
    }

    [Fact]
    public void IsEditable_WhenPublished_ShouldReturnFalse()
    {
        // Arrange
        var phrase = ContentItem.CreatePhrase("Hello", "greeting", "Used");
        phrase.SubmitForReview();
        phrase.Publish();

        // Assert
        phrase.IsEditable.Should().BeFalse();
    }

    [Fact]
    public void GetAllowedTransitions_FromDraft_ShouldReturnReviewOnly()
    {
        // Act
        var allowed = ContentStateTransitions.GetAllowedTransitions(ContentState.Draft);

        // Assert
        allowed.Should().ContainSingle().Which.Should().Be(ContentState.Review);
    }

    [Fact]
    public void GetAllowedTransitions_FromArchived_ShouldReturnEmpty()
    {
        // Act
        var allowed = ContentStateTransitions.GetAllowedTransitions(ContentState.Archived);

        // Assert
        allowed.Should().BeEmpty();
    }
}