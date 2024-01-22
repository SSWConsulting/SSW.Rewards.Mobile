﻿using SSW.Rewards.Application.Common.Exceptions;

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
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;
    private readonly IQuizGPTService _quizGptService;

    public Handler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUserService userService,
        IQuizGPTService quizGptService
        )
    {
        _context            = context;
        _currentUserService = currentUserService;
        _userService        = userService;
        _quizGptService     = quizGptService;
    }

    public async Task<Unit> Handle(SubmitAnswerCommand request, CancellationToken cancellationToken)
    {
        // make sure there isn't already an answer to this question (for this submission)
        await CheckForDuplicate(request, cancellationToken);
        
        // get the question text
        string questionText = await GetQuestionText(request.QuestionId);
        
        // run the answer through GPT
        QuizGPTRequestDto payload = new QuizGPTRequestDto
        {
            QuestionText    = questionText,
            AnswerText      = request.AnswerText
        };

        QuizGPTResponseDto result = await _quizGptService.ValidateAnswer(payload, cancellationToken);
        
        // write the answer to the database
        SubmittedQuizAnswer answer = new SubmittedQuizAnswer
        {
            QuizQuestionId  = request.QuestionId,
            AnswerText      = request.AnswerText,
            Correct         = result.Correct,
            GPTConfidence   = result.Confidence,
            GPTExplanation  = result.Explanation
        };
        await _context.SubmittedAnswers.AddAsync(answer, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
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

    private async Task CheckForDuplicate(SubmitAnswerCommand request, CancellationToken cancellationToken)
    {
        bool alreadyAnswered = await _context.SubmittedAnswers
                                            .Where(a =>
                                                    a.SubmissionId == request.SubmissionId
                                                &&  a.QuizQuestionId == request.QuestionId)
                                            .AnyAsync(cancellationToken);
        if (alreadyAnswered)
            throw new ArgumentException($"Question with id {request.QuestionId} has already been submitted for submission with id {request.SubmissionId}");
    }
}