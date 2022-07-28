using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Quizzes.Commands.SubmitUserQuiz;

namespace SSW.Rewards.Application.Quizzes.Queries.ValidateQuiz;
public class ValidateQuiz : IRequest<QuizResultDto>
{
    public int QuizId { get; set; }
    public List<QuizAnswerDto> Answers { get; set; }

    public ValidateQuiz(int quizId, List<QuizAnswerDto> answers)
    {
        QuizId  = quizId;
        Answers = answers;
    }
}

public class ValidateQuizHandler : IRequestHandler<ValidateQuiz, QuizResultDto>
{
    private readonly IApplicationDbContext _context;

    public ValidateQuizHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<QuizResultDto> Handle(ValidateQuiz request, CancellationToken cancellationToken)
    {
        // get quiz from db
        var dbQuiz = await _context.Quizzes
                                .Include(x => x.Achievement)
                                .Include(x => x.Questions)
                                    .ThenInclude(x => x.Answers)
                                .Where(x => x.Id == request.QuizId)
                                .AsNoTracking()
                                .FirstAsync(cancellationToken);

        // build return object
        QuizResultDto retVal = new QuizResultDto
        {
            QuizId = dbQuiz.Id,
            Passed = false, // set it to false here because we conditionally set it to true further down.
            Results = request.Answers.Select(userAnswer => new QuestionResultDto
            {
                QuestionId = userAnswer.QuestionId,
                Correct = userAnswer.SelectedAnswerId == dbQuiz.Questions
                                    .First(q => q.Id == userAnswer.QuestionId).Answers
                                        .First(dbAnswer => dbAnswer.IsCorrect).Id
            }).ToList()
        };

        // passed?
        retVal.Passed = !retVal.Results.Any(x => !x.Correct);
        
        return retVal;
    }
}