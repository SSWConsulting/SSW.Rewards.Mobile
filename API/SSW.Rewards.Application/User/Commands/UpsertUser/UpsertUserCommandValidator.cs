using FluentValidation;

namespace SSW.Rewards.Application.Users.Commands.UpsertUser
{
    public class UpsertUserCommandValidator : AbstractValidator<UpsertUserCommand>
    {
        public UpsertUserCommandValidator()
        {
            RuleFor(x => x.Email).NotEmpty().MaximumLength(128);
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(128);
        }
    }
}
