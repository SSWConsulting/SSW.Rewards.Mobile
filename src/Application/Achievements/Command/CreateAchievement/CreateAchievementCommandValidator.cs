namespace SSW.Rewards.Application.Achievements.Command.PostAchievement;

public class CreateAchievementCommandValidator : AbstractValidator<CreateAchievementCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateAchievementCommandValidator(IApplicationDbContext dbContext)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(128);
        
        RuleFor(x => x.Name)
            .MustAsync(BeUniqueName)
            .WithMessage("Achievement name already in use");


        _dbContext = dbContext;
    }

    private async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
    {
        return !await _dbContext.Achievements
            .AnyAsync(a => a.Name.ToLower() == name.ToLower(), cancellationToken);
    }
}
