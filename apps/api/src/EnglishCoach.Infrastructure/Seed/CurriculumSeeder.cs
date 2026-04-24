using EnglishCoach.Domain.LearningContent;

namespace EnglishCoach.Infrastructure.Seed;

public class CurriculumSeeder
{
    private readonly List<ContentItem> _phrases = new();
    private readonly List<ContentItem> _scenarios = new();

    public IReadOnlyList<ContentItem> Phrases => _phrases.AsReadOnly();
    public IReadOnlyList<ContentItem> Scenarios => _scenarios.AsReadOnly();

    public static CurriculumSeeder Seed()
    {
        var seeder = new CurriculumSeeder();
        seeder.SeedPhrases();
        seeder.SeedScenarios();
        return seeder;
    }

    private void SeedPhrases()
    {
        var phrases = new[]
        {
            // Greetings & Closings
            ("Good morning, thank you for joining our call.", "greeting", "Professional meeting opener"),
            ("I hope this email finds you well.", "greeting", "Email opener"),
            ("Please let me know if you need any clarification.", "greeting", "Professional closing"),
            ("Looking forward to your feedback.", "greeting", "Email closing"),
            ("Best regards from the team.", "greeting", "Sign-off"),

            // Daily Standup
            ("Yesterday I completed the authentication module.", "standup", "Completed work report"),
            ("I'm currently working on the API integration.", "standup", "In-progress report"),
            ("I need help with the database migration.", "standup", "Blocker report"),
            ("My plan for today is to finish testing.", "standup", "Plan report"),
            ("I've been blocked by the third-party API issue.", "standup", "Blocker flag"),
            ("The feature is 80% complete, should finish today.", "standup", "Progress estimate"),
            ("Waiting for design review approval.", "standup", "Dependency status"),
            ("Completed code review for PR #123.", "standup", "Completed review"),
            ("Deployed v2.1 to staging environment.", "standup", "Deployment status"),

            // Bug Reports
            ("We've identified a critical bug in the payment flow.", "bug", "Critical issue"),
            ("The third-party API is returning unexpected responses.", "bug", "External dependency issue"),
            ("There's a memory leak in the background worker.", "bug", "Performance issue"),
            ("Users are experiencing login failures after the update.", "bug", "User-impacting issue"),
            ("The cache invalidation logic is not working correctly.", "bug", "Logic bug"),
            ("Database connection pool is exhausted.", "bug", "Infrastructure issue"),
            ("The batch job failed for records over 10,000.", "bug", "Scale issue"),
            ("File upload fails for files larger than 5MB.", "bug", "Size limit bug"),

            // Clarification
            ("Could you clarify the acceptance criteria for this feature?", "clarification", "Requirements"),
            ("What is the expected response time for this API?", "clarification", "Technical specs"),
            ("Should this be a blocking or non-blocking operation?", "clarification", "Design question"),
            ("Is there a specific format for the export file?", "clarification", "Format question"),
            ("Can you provide more details about the error scenario?", "clarification", "Error context"),
            ("Which priority should this task take?", "clarification", "Priority question"),
            ("Do we need to support backward compatibility?", "clarification", "Compatibility scope"),

            // ETA & Delivery
            ("We expect to complete this by end of week.", "eta", "Week estimate"),
            ("The fix should take approximately 2 hours.", "eta", "Hours estimate"),
            ("I need 3 more days to finish the feature.", "eta", "Days estimate"),
            ("The deployment will take place during off-peak hours.", "eta", "Deployment timing"),
            ("We're targeting next Tuesday for the release.", "eta", "Release date"),
            ("QA testing will require 2 days after development.", "eta", "Testing buffer"),
            ("The infrastructure setup takes about 4 hours.", "eta", "Setup estimate"),

            // Technical Discussion
            ("I'd suggest using a message queue for async processing.", "technical", "Architecture proposal"),
            ("The current architecture cannot scale beyond 1000 users.", "technical", "Scale limitation"),
            ("We should consider caching at the CDN level.", "technical", "Performance optimization"),
            ("The API design follows RESTful principles.", "technical", "Design pattern"),
            ("Event sourcing would help with audit requirements.", "technical", "Pattern recommendation"),
            ("The batch process can be parallelized for better performance.", "technical", "Optimization"),
            ("We need to add retry logic for transient failures.", "technical", "Resilience"),
        };

        foreach (var (text, category, example) in phrases)
        {
            var phrase = ContentItem.CreatePhrase(text, category, example);
            phrase.SubmitForReview();
            phrase.Publish();
            _phrases.Add(phrase);
        }
    }

    private void SeedScenarios()
    {
        var scenarios = new[]
        {
            // Standup (5)
            ("Daily Standup - Regular", "Regular daily standup meeting", "Share daily updates", "Client PM who attends daily standups"),
            ("Async Standup Update", "Chat-based standup update", "Provide written updates", "Remote client in different timezone"),
            ("Blocked Status Report", "Reporting a blocker", "Communicate obstacles clearly", "Tech lead who removes blockers"),
            ("End-of-Sprint Summary", "Sprint wrap-up meeting", "Summarize sprint accomplishments", "Scrum master"),
            ("Absence Notice", "Vacation/absence notification", "Inform team of unavailability", "Team lead"),

            // Issue (5)
            ("Critical Bug Report", "Reporting a production bug", "Communicate severity and impact", "On-call engineer"),
            ("Production Incident", "Live production issue", "Escalate and coordinate response", "SRE/DevOps"),
            ("Third-Party Failure", "External dependency outage", "Report impact and workaround", "Integration specialist"),
            ("Performance Degradation", "System slowness reported", "Diagnose and communicate timeline", "Performance engineer"),
            ("Security Vulnerability", "Security issue discovered", "Report and coordinate fix", "Security team"),

            // Clarification (5)
            ("Requirement Ambiguity", "Unclear requirements", "Get clarity on scope", "Business analyst"),
            ("Acceptance Criteria Gap", "Missing acceptance criteria", "Define done criteria", "QA lead"),
            ("Conflicting Instructions", "Received conflicting directions", "Resolve contradiction", "Project manager"),
            ("Scope Clarification", "Feature scope unclear", "Define boundaries", "Product owner"),
            ("Technical Approach Question", "Implementation approach needed", "Get technical guidance", "Solution architect"),

            // ETA (5)
            ("Initial Estimate Request", "Client asks for timeline", "Provide realistic estimate", "Project manager"),
            ("Delay Notification", "Informing about a delay", "Manage expectations", "Delivery lead"),
            ("Rush Request Response", "Responding to expedite request", "Negotiate scope or timeline", "Account manager"),
            ("Milestone Adjustment", "Requesting timeline change", "Justify and propose new date", "Program manager"),
            ("Delivery Confirmation", "Confirming delivery date", "Commit to timeline", "Technical lead"),

            // Summary (5)
            ("End-of-Day Summary", "Daily wrap-up", "Summarize day accomplishments", "Team lead"),
            ("Weekly Status Report", "Weekly progress report", "Communicate weekly progress", "Project manager"),
            ("Sprint Retrospective", "Sprint review meeting", "Present sprint results", "Scrum master"),
            ("Project Completion", "Project go-live summary", "Celebrate and document", "Delivery manager"),
            ("Handover Documentation", "Knowledge transfer", "Ensure smooth handover", "Technical writer"),
        };

        foreach (var (title, description, goal, persona) in scenarios)
        {
            var scenario = ContentItem.CreateScenario(title, description, goal, persona);
            scenario.SubmitForReview();
            scenario.Publish();
            _scenarios.Add(scenario);
        }
    }

    public int TotalPhrases => _phrases.Count;
    public int TotalScenarios => _scenarios.Count;
    public int TotalContent => _phrases.Count + _scenarios.Count;
}
