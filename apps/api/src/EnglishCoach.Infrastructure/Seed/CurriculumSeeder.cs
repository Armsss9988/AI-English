using EnglishCoach.Domain.Curriculum;
using EnglishCoach.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishCoach.Infrastructure.Seed;

/// <summary>
/// Seeds the database with MVP curriculum content:
/// ~50 phrases across 5 categories and ~20 roleplay scenarios.
/// Idempotent: skips seeding if data already exists.
/// </summary>
public class CurriculumSeeder
{
    private readonly EnglishCoachDbContext _db;

    public CurriculumSeeder(EnglishCoachDbContext db)
    {
        _db = db;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await _db.Phrases.AnyAsync(ct))
            return; // Already seeded

        var phrases = GetSeedPhrases();
        var scenarios = GetSeedScenarios();

        _db.Phrases.AddRange(phrases);
        _db.RoleplayScenarios.AddRange(scenarios);

        await _db.SaveChangesAsync(ct);
    }

    public static IReadOnlyList<Phrase> GetSeedPhrases()
    {
        var items = new List<(string Text, string Vi, CommunicationFunction Fn, string Example)>
        {
            // Greetings / Standup (10)
            ("Good morning, thank you for joining our call.", "Chào buổi sáng, cảm ơn các bạn đã tham gia cuộc gọi.", CommunicationFunction.Standup, "Professional meeting opener"),
            ("I hope this email finds you well.", "Tôi hy vọng email này đến bạn vào lúc thuận lợi.", CommunicationFunction.Standup, "Email opener"),
            ("Please let me know if you need any clarification.", "Vui lòng cho tôi biết nếu bạn cần giải thích thêm.", CommunicationFunction.Standup, "Professional closing"),
            ("Yesterday I completed the authentication module.", "Hôm qua tôi đã hoàn thành module xác thực.", CommunicationFunction.Standup, "Completed work report"),
            ("I'm currently working on the API integration.", "Tôi đang làm việc trên tích hợp API.", CommunicationFunction.Standup, "In-progress report"),
            ("I need help with the database migration.", "Tôi cần giúp đỡ với migration database.", CommunicationFunction.Standup, "Blocker report"),
            ("My plan for today is to finish testing.", "Kế hoạch hôm nay là hoàn thành testing.", CommunicationFunction.Standup, "Plan report"),
            ("I've been blocked by the third-party API issue.", "Tôi bị chặn bởi vấn đề API bên thứ ba.", CommunicationFunction.Standup, "Blocker flag"),
            ("The feature is 80% complete, should finish today.", "Tính năng đã hoàn thành 80%, sẽ xong hôm nay.", CommunicationFunction.Standup, "Progress estimate"),
            ("Deployed v2.1 to staging environment.", "Đã deploy v2.1 lên môi trường staging.", CommunicationFunction.Standup, "Deployment status"),

            // Issue (10)
            ("We've identified a critical bug in the payment flow.", "Chúng tôi đã phát hiện lỗi nghiêm trọng trong luồng thanh toán.", CommunicationFunction.Issue, "Critical issue report"),
            ("The third-party API is returning unexpected responses.", "API bên thứ ba đang trả về kết quả không mong đợi.", CommunicationFunction.Issue, "External dependency issue"),
            ("There's a memory leak in the background worker.", "Có rò rỉ bộ nhớ trong background worker.", CommunicationFunction.Issue, "Performance issue"),
            ("Users are experiencing login failures after the update.", "Người dùng đang gặp lỗi đăng nhập sau bản cập nhật.", CommunicationFunction.Issue, "User-impacting issue"),
            ("The cache invalidation logic is not working correctly.", "Logic xoá cache không hoạt động đúng.", CommunicationFunction.Issue, "Logic bug"),
            ("Database connection pool is exhausted.", "Connection pool database đã cạn kiệt.", CommunicationFunction.Issue, "Infrastructure issue"),
            ("The batch job failed for records over 10,000.", "Batch job thất bại khi xử lý hơn 10,000 record.", CommunicationFunction.Issue, "Scale issue"),
            ("File upload fails for files larger than 5MB.", "Upload file thất bại khi file lớn hơn 5MB.", CommunicationFunction.Issue, "Size limit bug"),
            ("The notification service is sending duplicate emails.", "Dịch vụ thông báo đang gửi email trùng lặp.", CommunicationFunction.Issue, "Duplicate behavior"),
            ("SSL certificate expired on the staging server.", "Chứng chỉ SSL đã hết hạn trên server staging.", CommunicationFunction.Issue, "Infrastructure"),

            // Clarification (10)
            ("Could you clarify the acceptance criteria for this feature?", "Bạn có thể làm rõ tiêu chí nghiệm thu cho tính năng này không?", CommunicationFunction.Clarification, "Requirements"),
            ("What is the expected response time for this API?", "Thời gian phản hồi mong đợi cho API này là bao lâu?", CommunicationFunction.Clarification, "Technical specs"),
            ("Should this be a blocking or non-blocking operation?", "Đây nên là thao tác blocking hay non-blocking?", CommunicationFunction.Clarification, "Design question"),
            ("Is there a specific format for the export file?", "Có định dạng cụ thể nào cho file export không?", CommunicationFunction.Clarification, "Format question"),
            ("Do we need to support backward compatibility?", "Chúng ta có cần hỗ trợ tương thích ngược không?", CommunicationFunction.Clarification, "Compatibility"),
            ("Which user roles should have access to this feature?", "Vai trò nào nên có quyền truy cập tính năng này?", CommunicationFunction.Clarification, "Authorization"),
            ("Should we log this event for audit purposes?", "Chúng ta có nên ghi log sự kiện này cho mục đích audit không?", CommunicationFunction.Clarification, "Compliance"),
            ("What is the expected load for this endpoint?", "Tải dự kiến cho endpoint này là bao nhiêu?", CommunicationFunction.Clarification, "Capacity"),
            ("Are we targeting mobile or desktop for this release?", "Bản release này nhắm đến mobile hay desktop?", CommunicationFunction.Clarification, "Platform"),
            ("Should error messages be user-facing or technical?", "Thông báo lỗi nên hướng đến người dùng hay kỹ thuật?", CommunicationFunction.Clarification, "UX decision"),

            // ETA (10)
            ("I estimate two more days to complete this feature.", "Tôi ước tính cần thêm hai ngày để hoàn thành tính năng này.", CommunicationFunction.Eta, "Timeline estimate"),
            ("The deployment is scheduled for Friday afternoon.", "Việc deploy được lên lịch chiều thứ Sáu.", CommunicationFunction.Eta, "Release plan"),
            ("We need to push back the deadline by one sprint.", "Chúng ta cần lùi deadline thêm một sprint.", CommunicationFunction.Eta, "Delay report"),
            ("The hotfix should be ready within the hour.", "Hotfix sẽ sẵn sàng trong vòng một giờ.", CommunicationFunction.Eta, "Urgent timeline"),
            ("QA testing will take approximately three days.", "QA testing sẽ mất khoảng ba ngày.", CommunicationFunction.Eta, "Testing timeline"),
            ("We're on track to deliver by end of sprint.", "Chúng ta đang đúng tiến độ để giao cuối sprint.", CommunicationFunction.Eta, "Status update"),
            ("The migration will require a four-hour maintenance window.", "Migration sẽ cần khung thời gian bảo trì bốn giờ.", CommunicationFunction.Eta, "Maintenance plan"),
            ("I'll have the prototype ready for review by Wednesday.", "Tôi sẽ có prototype sẵn sàng để review trước thứ Tư.", CommunicationFunction.Eta, "Prototype timeline"),
            ("The security audit is expected to take two weeks.", "Security audit dự kiến mất hai tuần.", CommunicationFunction.Eta, "Audit timeline"),
            ("Performance testing results should be available tomorrow.", "Kết quả performance testing sẽ có vào ngày mai.", CommunicationFunction.Eta, "Testing status"),

            // Summary (10)
            ("To summarize, we agreed on the following action items.", "Tóm lại, chúng ta đã đồng ý các action items sau.", CommunicationFunction.Summary, "Meeting conclusion"),
            ("The key takeaway is that we need to prioritize security.", "Điểm chính là chúng ta cần ưu tiên bảo mật.", CommunicationFunction.Summary, "Priority highlight"),
            ("Next steps: I'll create the tickets and assign owners.", "Bước tiếp theo: tôi sẽ tạo tickets và phân công.", CommunicationFunction.Summary, "Action plan"),
            ("Let me recap what we discussed in today's standup.", "Để tôi tóm tắt lại những gì chúng ta đã thảo luận.", CommunicationFunction.Summary, "Recap"),
            ("In conclusion, the release is a go pending QA sign-off.", "Kết luận, bản release sẽ tiến hành sau khi QA sign-off.", CommunicationFunction.Summary, "Release decision"),
            ("I recommend we use PostgreSQL for this project.", "Tôi đề xuất sử dụng PostgreSQL cho dự án này.", CommunicationFunction.Recommendation, "Technology recommendation"),
            ("Based on our analysis, option B is the best approach.", "Dựa trên phân tích, phương án B là cách tiếp cận tốt nhất.", CommunicationFunction.Recommendation, "Technical recommendation"),
            ("I suggest we implement caching at the API gateway level.", "Tôi đề xuất triển khai caching ở tầng API gateway.", CommunicationFunction.Recommendation, "Architecture suggestion"),
            ("We should consider using feature flags for this rollout.", "Chúng ta nên cân nhắc sử dụng feature flags cho lần phát hành này.", CommunicationFunction.Recommendation, "Release strategy"),
            ("My recommendation is to break this into smaller PRs.", "Đề xuất của tôi là chia nhỏ thành các PR nhỏ hơn.", CommunicationFunction.Recommendation, "Process suggestion"),
        };

        return items.Select(p => Phrase.Create(
            Guid.NewGuid().ToString("N"),
            p.Text, p.Vi, p.Fn, ContentLevel.Core, p.Example)).ToList();
    }

    public static IReadOnlyList<RoleplayScenario> GetSeedScenarios()
    {
        var items = new List<(string Title, string Context, string UserRole, string Persona, string Goal, string Group, string[] MustCover, string[] Criteria, int Difficulty)>
        {
            // Standup (5)
            ("Daily standup — progress report", "Daily scrum meeting", "Developer", "Scrum Master", "Give a clear status update", "standup",
                new[] { "what you did", "what you'll do", "any blockers" }, new[] { "clear_update", "time_bounded" }, 1),
            ("Daily standup — blocker", "Daily scrum meeting", "Developer", "Tech Lead", "Clearly explain the blocker and ask for help", "standup",
                new[] { "describe blocker", "impact on sprint", "request help" }, new[] { "blocker_explained", "help_requested" }, 2),
            ("Daily standup — async update", "Remote async standup", "Developer", "Remote Team", "Write a concise standup message", "standup",
                new[] { "completed tasks", "planned tasks" }, new[] { "concise_update" }, 1),
            ("Daily standup — sprint goal check", "Sprint planning follow-up", "Developer", "Product Owner", "Confirm sprint goal alignment", "standup",
                new[] { "sprint goal reference", "progress towards goal" }, new[] { "goal_aligned" }, 2),
            ("Daily standup — handoff", "Team handoff meeting", "Developer", "Team Member", "Clearly explain what needs to be continued", "standup",
                new[] { "current state", "next steps", "context needed" }, new[] { "clear_handoff" }, 2),

            // Issue (5)
            ("Report critical production bug", "Production incident", "Developer", "Engineering Manager", "Describe impact and proposed fix", "issue",
                new[] { "bug description", "user impact", "proposed fix" }, new[] { "impact_communicated", "fix_proposed" }, 3),
            ("Report performance degradation", "Performance monitoring", "Developer", "SRE Engineer", "Provide metrics and root cause", "issue",
                new[] { "metrics data", "root cause", "mitigation plan" }, new[] { "data_driven", "actionable" }, 3),
            ("Report third-party outage", "External service downtime", "Developer", "DevOps Lead", "Communicate workaround and ETA", "issue",
                new[] { "service affected", "workaround", "expected resolution" }, new[] { "workaround_provided" }, 2),
            ("Report security vulnerability", "Security assessment", "Developer", "Security Lead", "Describe vulnerability and remediation", "issue",
                new[] { "vulnerability details", "severity", "remediation steps" }, new[] { "severity_assessed", "remediation_planned" }, 3),
            ("Report data inconsistency", "Data quality check", "Developer", "Database Admin", "Explain inconsistency and impact", "issue",
                new[] { "inconsistency description", "affected records", "impact" }, new[] { "scope_defined" }, 2),

            // Clarification (5)
            ("Clarify requirements with PM", "Feature planning", "Developer", "Product Manager", "Get clear acceptance criteria", "clarification",
                new[] { "specific questions", "acceptance criteria" }, new[] { "criteria_obtained" }, 2),
            ("Clarify API contract", "API design discussion", "Developer", "Backend Developer", "Agree on request/response shapes", "clarification",
                new[] { "endpoint structure", "data types", "error handling" }, new[] { "contract_agreed" }, 2),
            ("Clarify deployment process", "Release preparation", "Developer", "DevOps Engineer", "Understand the deployment pipeline", "clarification",
                new[] { "deployment steps", "rollback plan" }, new[] { "process_understood" }, 1),
            ("Clarify priority with stakeholder", "Priority alignment", "Developer", "Stakeholder", "Align on what to build next", "clarification",
                new[] { "business value", "urgency", "trade-offs" }, new[] { "priority_aligned" }, 3),
            ("Clarify testing strategy", "QA planning", "Developer", "QA Lead", "Agree on test coverage requirements", "clarification",
                new[] { "test scope", "coverage target", "automation plan" }, new[] { "strategy_agreed" }, 2),

            // ETA (3)
            ("Negotiate deadline extension", "Project timeline", "Developer", "Project Manager", "Explain why more time is needed", "eta",
                new[] { "reason for delay", "new timeline", "mitigation" }, new[] { "justified_extension" }, 3),
            ("Provide sprint estimate", "Sprint planning", "Developer", "Scrum Master", "Provide realistic estimates", "eta",
                new[] { "story points", "assumptions", "risks" }, new[] { "realistic_estimate" }, 2),
            ("Communicate delay to client", "Client meeting", "Developer", "Account Manager", "Communicate delay professionally", "eta",
                new[] { "delay reason", "revised timeline", "mitigation plan" }, new[] { "professional_communication" }, 3),

            // Summary (2)
            ("Summarize sprint retrospective", "Retro meeting", "Developer", "Agile Coach", "Capture action items and improvements", "summary",
                new[] { "what went well", "what to improve", "action items" }, new[] { "actionable_summary" }, 2),
            ("Summarize technical decision", "Architecture review", "Developer", "Tech Lead", "Explain decision and rationale", "summary",
                new[] { "decision", "alternatives considered", "rationale" }, new[] { "clear_rationale" }, 3),
        };

        return items.Select(s => RoleplayScenario.Create(
            Guid.NewGuid().ToString("N"),
            s.Title, s.Context, s.UserRole, s.Persona, s.Goal,
            s.MustCover, Array.Empty<string>(), s.Criteria, s.Difficulty)).ToList();
    }
}

/// <summary>
/// Calls CurriculumSeeder at application startup.
/// </summary>
public static class DatabaseCurriculumSeeder
{
    public static async Task SeedDatabaseAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EnglishCoachDbContext>();
        var seeder = new CurriculumSeeder(db);
        await seeder.SeedAsync();
    }
}
