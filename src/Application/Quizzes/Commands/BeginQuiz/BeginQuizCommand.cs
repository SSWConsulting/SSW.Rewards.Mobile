using SSW.Rewards.Application.Common.Exceptions;

namespace SSW.Rewards.Application.Quizzes.Commands.BeginQuiz;

public class BeginQuizCommand : IRequest<int>
{
    public int QuizId { get; set; }
}

public sealed class Handler : IRequestHandler<BeginQuizCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;

    public Handler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUserService userService
        )
    {
        _context            = context;
        _currentUserService = currentUserService;
        _userService        = userService;
    }

    public async Task<int> Handle(BeginQuizCommand request, CancellationToken cancellationToken)
    {
        // make sure the quiz exists
        var quiz = await _context.Quizzes.FindAsync(request.QuizId);
        if (quiz == null)
            throw new NotFoundException(nameof(Quiz), request.QuizId);

        // create a new submission
        int userId = await _userService.GetUserId(_currentUserService.GetUserEmail(), cancellationToken);
        var submission = new CompletedQuiz
        {
            QuizId = request.QuizId,
            UserId = userId
        };
        await _context.CompletedQuizzes.AddAsync(submission, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return submission.Id;
    }
}