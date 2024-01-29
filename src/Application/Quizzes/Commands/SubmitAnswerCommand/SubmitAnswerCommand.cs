using Microsoft.Extensions.Logging;
using SSW.Rewards.Application.Common.Exceptions;

namespace SSW.Rewards.Application.Quizzes.Commands.SubmitAnswerCommand;

public class SubmitAnswerCommand : IRequest<Unit>
{
    public int SubmissionId { get; set; }
    public int QuestionId { get; set; }
    public string AnswerText { get; set; } = "";
}

public sealed class Handler : IRequestHandler<SubmitAnswerCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IQuizGPTService _quizGptService;
    private readonly ILogger<SubmitAnswerCommand> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;

    public Handler(
        IApplicationDbContext context,
        IQuizGPTService quizGptService,
        ILogger<SubmitAnswerCommand> logger,
        ICurrentUserService currentUserService,
        IUserService userService
        )
    {
        _context            = context;
        _quizGptService     = quizGptService;
        _logger             = logger;
        _currentUserService = currentUserService;
        _userService        = userService;
    }

    public async Task<Unit> Handle(SubmitAnswerCommand request, CancellationToken cancellationToken)
    {
        // get the question text
        string questionText = await GetQuestionText(request.QuestionId);

        // Build the object to send to GPT
        QuizGPTRequestDto payload = new QuizGPTRequestDto
        {
            QuestionText    = questionText,
            AnswerText      = request.AnswerText
        };

        int userId = await _userService.GetUserId(_currentUserService.GetUserEmail(), cancellationToken);

        _quizGptService.ProcessAnswer(userId, payload, request);

        return Unit.Value;
    }
    
    private async Task<string> GetQuestionText(int questionId)
    {
        QuizQuestion? dbQuestion = await _context.QuizQuestions
                                            .Where(q => q.Id == questionId)
                                            .FirstOrDefaultAsync();
        if (dbQuestion == null)
            throw new NotFoundException(nameof(QuizQuestion), questionId);
        string questionText = dbQuestion.Text ?? throw new ArgumentNullException(nameof(dbQuestion.Text));
        return questionText;
    }
}