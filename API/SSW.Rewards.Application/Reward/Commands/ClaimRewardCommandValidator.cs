using FluentValidation;

namespace SSW.Rewards.Application.Reward.Commands
{
    public class ClaimRewardCommandValidator : AbstractValidator<ClaimRewardCommand>
    {
        public ClaimRewardCommandValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(128);
        }
    }
}
