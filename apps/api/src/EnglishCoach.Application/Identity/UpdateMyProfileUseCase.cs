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
        var englishLevel = Enum.Parse<EnglishLevel>(request.CurrentLevel, ignoreCase: true);
        var role = Enum.Parse<LearnerRole>(request.Role, ignoreCase: true);

        var displayName = string.IsNullOrWhiteSpace(request.DisplayName) ? "Learner" : request.DisplayName;
        var nativeLanguage = string.IsNullOrWhiteSpace(request.NativeLanguage) ? "Vietnamese" : request.NativeLanguage;
        var timelineWeeks = request.TargetTimelineWeeks ?? 12;

        if (currentProfile is null)
        {
            currentProfile = LearnerProfile.Create(
                userId,
                displayName,
                nativeLanguage,
                request.Timezone,
                englishLevel,
                request.TargetUseCase,
                timelineWeeks,
                role);

            await _repository.CreateAsync(currentProfile, cancellationToken);

            return LearnerProfileContractMapper.ToResponse(currentProfile);
        }

        currentProfile.Update(
            displayName,
            nativeLanguage,
            request.Timezone,
            englishLevel,
            request.TargetUseCase,
            timelineWeeks,
            role);

        await _repository.UpdateAsync(currentProfile, cancellationToken);

        return LearnerProfileContractMapper.ToResponse(currentProfile);
    }
}
