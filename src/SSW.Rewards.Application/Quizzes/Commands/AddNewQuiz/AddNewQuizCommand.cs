//using SSW.Rewards.Application.Quizzes.Common;

//namespace SSW.Rewards.Application.Quizzes.Commands.AddNewQuiz;
//public class AddNewQuizCommand : IRequest<int>
//{
//    public QuizDetailsDto Quiz { get; set; }

//    public int Points { get; set; }
//}

//public class AddNewQuizCommandHandler : IRequestHandler<AddNewQuizCommand, int>
//{
//    private readonly IApplicationDbContext _context;
//    private readonly ICurrentUserService _currentUserService;
//    private readonly IUserService _userService;

//    public AddNewQuizCommandHandler(
//        IApplicationDbContext context,
//        ICurrentUserService currentUserService,
//        IUserService userService)
//    {
//        _context = context;
//        _currentUserService = currentUserService;
//        _userService = userService;
//    }

//    public async Task<int> Handle(AddNewQuizCommand request, CancellationToken cancellationToken)
//    {
//        var quiz = new Quiz
//        {
//            Title       = request.Quiz.Title,
//            Description = request.Quiz.Description,
//        };

//        foreach (var question in request.Quiz.Questions)
//        {
//            var dbQuestion = await _context.QuizQuestions.FirstOrDefaultAsync(q => q.Id == question.QuestionId, cancellationToken);

//            if (dbQuestion is null)
//            {
//                dbQuestion = new QuizQuestion
//                {
//                    Text = question.Text
//                };

//                foreach (var answer in question.Answers)
//                {

//                }
//            }
//        }
//    }
//}
