using FluentValidation;

namespace SSW.Consulting.Application.Reward.Commands
{
    public class ClaimRewardCommandValidator : AbstractValidator<ClaimRewardCommand>
    {
        public ClaimRewardCommandValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(128);
        }
    }
}
