using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Users.Common.Interfaces;
using SSW.Rewards.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Quizzes.Commands.SubmitUserQuiz
{
    public class SubmitUserQuizCommand : IRequest<QuizResultDto>
    {
        public int QuizId { get; set; }
        public List<QuizAnswer> Answers { get; set; }

        public sealed class Handler : IRequestHandler<SubmitUserQuizCommand, QuizResultDto>
        {
            private readonly IMapper _mapper;
            private readonly ISSWRewardsDbContext _context;
            private readonly ICurrentUserService _currentUserService;
            private readonly IUserService _userService;

            public Handler(
                IMapper mapper,
                ISSWRewardsDbContext context,
                ICurrentUserService currentUserService,
                IUserService userService)
            {
                _mapper             = mapper;
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
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(cancellationToken);

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
                    var userId = await _userService.GetUserId(_currentUserService.GetUserEmail());
                    AddCompletedQuiz(dbQuiz.Id, userId);
                    AddQuizAchievement(dbQuiz.AchievementId, userId);
                    await _context.SaveChangesAsync(cancellationToken);

                    retVal.Passed = true;
                    retVal.Points = dbQuiz.Achievement.Value;
                }
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

            private void AddCompletedQuiz(int quizId, int userId)
            {
                CompletedQuiz c = new CompletedQuiz
                {
                    QuizId = quizId,
                    UserId = userId
                };
                _context.CompletedQuizzes.Add(c);
            }
        }


    }
    public class SubmitUserQuizCommandValidator : AbstractValidator<SubmitUserQuizCommand>
    {
        private readonly ISSWRewardsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserService _userService;

        public SubmitUserQuizCommandValidator(
            ISSWRewardsDbContext context,
            ICurrentUserService currentUserService,
            IUserService userService)
        {
            this._context = context;
            this._currentUserService = currentUserService;
            this._userService = userService;
            RuleFor(x => x.QuizId)
                .MustAsync(CanSubmit);
        }

        public async Task<bool> CanSubmit(int quizId, CancellationToken token)
        {
            var userId = await this._userService.GetUserId(this._currentUserService.GetUserEmail());
            return !this._context.CompletedQuizzes.Any(x => x.QuizId == quizId && x.UserId == userId);
        }
    }
    public class QuizAnswer
    {
        public int QuestionId { get; set; }
        public int SelectedAnswerId { get; set; }
    }

}
