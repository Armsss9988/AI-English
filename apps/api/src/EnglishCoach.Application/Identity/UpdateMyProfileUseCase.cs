using EnglishCoach.Contracts.Identity;
using EnglishCoach.Domain.Identity;

namespace EnglishCoach.Application.Identity;

public sealed class UpdateMyProfileUseCase
{
    private readonly ILearnerProfileRepository _repository;

    public UpdateMyProfileUseCase(ILearnerProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<MyProfileResponse> ExecuteAsync(
        string userId,
        UpdateMyProfileRequest request,
        CancellationToken cancellationToken)
    {
        var currentProfile = await _repository.GetByUserIdAsync(userId, cancellationToken);
        var englishLevel = Enum.Parse<EnglishLevel>(request.CurrentEnglishLevel, ignoreCase: true);
        var role = Enum.Parse<LearnerRole>(request.Role, ignoreCase: true);

        if (currentProfile is null)
        {
            currentProfile = LearnerProfile.Create(
                userId,
                request.DisplayName,
                request.NativeLanguage,
                request.Timezone,
                englishLevel,
                request.TargetUseCase,
                request.TargetTimelineWeeks,
                role);

            await _repository.CreateAsync(currentProfile, cancellationToken);

            return LearnerProfileContractMapper.ToResponse(currentProfile);
        }

        currentProfile.Update(
            request.DisplayName,
            request.NativeLanguage,
            request.Timezone,
            englishLevel,
            request.TargetUseCase,
            request.TargetTimelineWeeks,
            role);

        await _repository.UpdateAsync(currentProfile, cancellationToken);

        return LearnerProfileContractMapper.ToResponse(currentProfile);
    }
}
