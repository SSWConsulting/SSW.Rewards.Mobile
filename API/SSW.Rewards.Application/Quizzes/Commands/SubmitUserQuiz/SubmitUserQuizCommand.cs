using AutoMapper;
using FluentValidation;
using MediatR;
using SSW.Rewards.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
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
            private readonly ICurrentUserService _userService;

            public Handler(
                IMapper mapper,
                ISSWRewardsDbContext context,
                ICurrentUserService userServ)
            {
                _mapper = mapper;
                _context = context;
                _userService = userServ;
            }

            public async Task<QuizResultDto> Handle(SubmitUserQuizCommand request, CancellationToken cancellationToken)
            {
                // check results

                // success? Add achievement and get points!

                // oh no! they've done it already!

                return new QuizResultDto
                {
                    QuizId = 1,
                    Passed = true,
                    Results = new List<QuestionResultDto>
                    {
                        new QuestionResultDto
                        {
                            QuestionId = 1,
                            Correct = true
                        }
                    }
                };
            }
        
        
        }

        public class SubmitUserQuizCommandValidator : AbstractValidator<SubmitUserQuizCommand>
        {
            private readonly ISSWRewardsDbContext _context;
            private readonly ICurrentUserService _currentUserService;

            public SubmitUserQuizCommandValidator(ISSWRewardsDbContext context, ICurrentUserService currentUserService)
            {
                this._context = context;
                this._currentUserService = currentUserService;
                RuleFor(x => x.QuizId)
                    .MustAsync(CanSubmit);
            }

            public async Task<bool> CanSubmit(int quizId, CancellationToken token)
            {
                // get the quiz from the UserQuizzes table by QuizId and if it exists return false (to fail)

                return true;
            }
        }

    }
    public class QuizAnswer
    {
        public int QuestionId { get; set; }
        public int SelectedAnswerId { get; set; }
    }

}
