using FluentValidation;

namespace SSW.Rewards.Application.Rewards.Commands
{
    public class ClaimRewardCommandValidator : AbstractValidator<ClaimRewardCommand>
    {
        public ClaimRewardCommandValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(128);
        }
    }
}
