namespace SSW.Rewards.Application.Rewards.Commands.ClaimReward;

public class ClaimRewardCommandValidator : AbstractValidator<ClaimRewardCommand>
{
    public ClaimRewardCommandValidator()
    {
        RuleFor(request => request)
            .Must(HaveCodeOrId);
    }

    private bool HaveCodeOrId(ClaimRewardCommand command)
    {
        return !string.IsNullOrWhiteSpace(command.Code) || command.Id > 0;
    }
}
