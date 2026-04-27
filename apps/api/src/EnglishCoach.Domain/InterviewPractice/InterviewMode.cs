namespace EnglishCoach.Domain.InterviewPractice;

/// <summary>
/// Interview mode determines coaching behavior during the session.
/// RealInterview: no hints during answer, interviewer behaves like a real interviewer.
/// TrainingInterview: shows hints, scorecard, retry drills, pronunciation practice.
/// </summary>
public enum InterviewMode
{
    RealInterview,
    TrainingInterview
}
