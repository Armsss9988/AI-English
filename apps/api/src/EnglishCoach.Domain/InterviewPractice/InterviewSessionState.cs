namespace EnglishCoach.Domain.InterviewPractice;

public enum InterviewSessionState
{
    Created,
    Analyzing,
    Ready,
    Active,
    AwaitingFeedback,
    Finalized,
    Archived
}

public enum InterviewType
{
    Behavioral,
    Technical,
    Mixed,
    Situational
}

public enum InterviewQuestionCategory
{
    Behavioral,
    Technical,
    Situational,
    FollowUp,
    Opening,
    Closing
}
