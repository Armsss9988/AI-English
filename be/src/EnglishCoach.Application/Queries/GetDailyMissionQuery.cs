using EnglishCoach.Application.Dto;

namespace EnglishCoach.Application.Queries;

public record GetDailyMissionQuery(Guid LearnerId);

public interface IGetDailyMissionHandler
{
    Task<DailyMissionDto> HandleAsync(GetDailyMissionQuery query, CancellationToken ct = default);
}