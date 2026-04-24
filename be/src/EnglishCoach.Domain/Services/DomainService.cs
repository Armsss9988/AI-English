namespace EnglishCoach.Domain.Services;

public interface IDomainService
{
    bool IsValidStateTransition(object entity, string fromState, string toState);
}