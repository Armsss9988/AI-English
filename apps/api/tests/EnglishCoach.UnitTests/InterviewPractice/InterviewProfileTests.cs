using EnglishCoach.Domain.InterviewPractice;
using Xunit;

namespace EnglishCoach.UnitTests.InterviewPractice;

public sealed class InterviewProfileTests
{
    [Fact]
    public void Create_Removes_Null_Characters_From_Cv_Text()
    {
        var profile = InterviewProfile.Create(
            "profile-1",
            "learner-1",
            "Backend\0 developer with ASP.NET Core");

        Assert.Equal("Backend developer with ASP.NET Core", profile.CvText);
    }

    [Fact]
    public void UpdateCv_Removes_Null_Characters_From_Cv_Text()
    {
        var profile = InterviewProfile.Create("profile-1", "learner-1", "Initial CV");

        profile.UpdateCv("React\0 and PostgreSQL engineer");

        Assert.Equal("React and PostgreSQL engineer", profile.CvText);
    }

    [Fact]
    public void SetCvAnalysis_Removes_Null_Characters_From_Analysis()
    {
        var profile = InterviewProfile.Create("profile-1", "learner-1", "Initial CV");

        profile.SetCvAnalysis("{\"name\":\"Nguyen\0 Van A\"}");

        Assert.Equal("""{"name":"Nguyen Van A"}""", profile.CvAnalysis);
    }
}
