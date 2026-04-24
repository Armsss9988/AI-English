using EnglishCoach.Contracts.Identity;

namespace EnglishCoach.Application.Identity;

public sealed class GetMyProfileUseCase
{
    private readonly ILearnerProfileRepository _repository;

    public GetMyProfileUseCase(ILearnerProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<MyProfileResponse?> ExecuteAsync(string userId, CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByUserIdAsync(userId, cancellationToken);

        return profile is null ? null : LearnerProfileContractMapper.ToResponse(profile);
    }
}
