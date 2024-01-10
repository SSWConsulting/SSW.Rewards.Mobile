namespace SSW.Rewards.Application.Quizzes.Queries.GetAdminQuizDetails;

public class GetAdminQuizDetailsQueryValidator : AbstractValidator<GetAdminQuizDetailsQuery>
{
    private readonly IApplicationDbContext _context;

    public GetAdminQuizDetailsQueryValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.QuizId)
            .NotEmpty().WithMessage("QuizId is required.");

        RuleFor(v => v.QuizId)
            .MustAsync(ExistAsync)
            .WithErrorCode("404")
            .WithMessage("The specified quiz does not exist.");
    }

    private async Task<bool> ExistAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Quizzes.AnyAsync(l => l.Id == id, cancellationToken);
    }
}
