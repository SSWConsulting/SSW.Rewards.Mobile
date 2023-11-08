namespace SSW.Rewards.Application.Quizzes.Commands.SubmitUserQuiz;

public class SubmitUserQuizCommand : IRequest<QuizResultDto>
{
    public int QuizId { get; set; }
    public List<QuizAnswerDto> Answers { get; set; }

    public sealed class Handler : IRequestHandler<SubmitUserQuizCommand, QuizResultDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserService _userService;

        public Handler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            IUserService userService)
        {
            _context            = context;
            _currentUserService = currentUserService;
            _userService        = userService;
        }

        public async Task<QuizResultDto> Handle(SubmitUserQuizCommand request, CancellationToken cancellationToken)
        {
            // get quiz from db
            var dbQuiz = await _context.Quizzes
                                    .Include(x => x.Achievement)
                                    .Include(x => x.Questions)
                                        .ThenInclude(x => x.Answers)
                                    .Where(x => x.Id == request.QuizId)
                                    //.AsNoTracking()
                                    .FirstOrDefaultAsync(cancellationToken);

            var userId = await _userService.GetUserId(_currentUserService.GetUserEmail());

            // build return object
            QuizResultDto retVal = new QuizResultDto
            {
                QuizId  = dbQuiz.Id,
                Passed  = false, // set it to false here because we conditionally set it to true further down.
                Results = request.Answers.Select(userAnswer => new QuestionResultDto
                {
                    QuestionId  = userAnswer.QuestionId,
                    Correct     = userAnswer.SelectedAnswerId == dbQuiz.Questions
                                        .First(q => q.Id == userAnswer.QuestionId).Answers
                                            .First(dbAnswer => dbAnswer.IsCorrect).Id
                }).ToList()
            };

            // success? Add the quiz to the user's completed list and give them the achievement!
            if (!retVal.Results.Any(x => !x.Correct))
            {                
                AddQuizAchievement(dbQuiz.AchievementId, userId);
                retVal.Passed = true;
                retVal.Points = dbQuiz.Achievement.Value;
            }

            var dbAnswers = dbQuiz.Questions.SelectMany(q => q.Answers);

            AddCompletedQuiz(dbQuiz.Id, userId, request.Answers, dbAnswers, retVal.Passed);
            
            await _context.SaveChangesAsync(cancellationToken);

            return retVal;
        }

        private void AddQuizAchievement(int achievementId, int userId)
        {
            UserAchievement quizCompletedAchievement = new UserAchievement
            {
                UserId        = userId,
                AwardedAt     = DateTime.UtcNow,
                AchievementId = achievementId
            };
            _context.UserAchievements.Add(quizCompletedAchievement);
        }

        private void AddCompletedQuiz(int quizId, int userId, List<QuizAnswerDto> submittedAnswers, IEnumerable<QuizAnswer> dbAnswers, bool passed)
        {
            CompletedQuiz c = new CompletedQuiz
            {
                QuizId = quizId,
                UserId = userId,
                Passed = passed
            };

            foreach (var answer in submittedAnswers)
            {
                var dbAnswer = dbAnswers.First(q => q.Id == answer.SelectedAnswerId);

                c.Answers.Add(new SubmittedQuizAnswer { Answer = dbAnswer });
            }

            _context.CompletedQuizzes.Add(c);
        }
    }

    public class SubmitUserQuizCommandValidator : AbstractValidator<SubmitUserQuizCommand>
    {
        private readonly IApplicationDbContext _context;

        public SubmitUserQuizCommandValidator(IApplicationDbContext context)
        {
            this._context = context;
            RuleFor(x => x.QuizId)
                .MustAsync(CanSubmit);
        }

        public async Task<bool> CanSubmit(int quizId, CancellationToken token)
        {
            if (await _context.CompletedQuizzes.AnyAsync(c => c.QuizId == quizId && c.Passed))
                return false;

            return true;
        }
    }

}
public class QuizAnswerDto
{
    public int QuestionId { get; set; }
    public int SelectedAnswerId { get; set; }
}

