using FluentValidation;

namespace SSW.Consulting.Application.User.Commands.UpsertUser
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
