using EnglishCoach.Application.Dto;
using EnglishCoach.Application.Queries;
using EnglishCoach.Contracts.DailyMission;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCoach.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class DailyMissionController : ControllerBase
{
    private readonly IGetDailyMissionHandler _handler;

    public DailyMissionController(IGetDailyMissionHandler handler)
    {
        _handler = handler;
    }

    [HttpGet("{learnerId:guid}")]
    public async Task<ActionResult<GetDailyMissionResponse>> Get(
        Guid learnerId,
        CancellationToken ct = default)
    {
        var query = new GetDailyMissionQuery(learnerId);
        var dto = await _handler.HandleAsync(query, ct);

        var response = new GetDailyMissionResponse(
            dto.LearnerId,
            dto.MissionDate.ToString("yyyy-MM-dd"),
            MapSection(dto.Reviews),
            MapSection(dto.Speaking),
            MapSection(dto.Roleplay),
            MapSection(dto.Retry),
            dto.TotalItems,
            dto.IsComplete
        );

        return Ok(response);
    }

    private static DailyMissionSectionContract MapSection(DailyMissionSectionDto section) =>
        new(
            section.SectionName,
            section.Required,
            section.Provided,
            section.Items.Select(i => new DailyMissionItemContract(
                i.Id,
                i.Title,
                i.Subtitle,
                i.Category,
                i.ItemType
            )).ToList(),
            section.IsDegraded
        );
}