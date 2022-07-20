using AutoMapper;
using MediatR;
using SSW.Rewards.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Quizzes.Queries.GetQuizDetailsQuery
{
    public class GetQuizDetailsQuery : IRequest<QuizDetailsDto>
    {
        public int QuizId { get; set; }

        public GetQuizDetailsQuery(int id)
        {
            this.QuizId = id;
        }
        
        public sealed class Handler : IRequestHandler<GetQuizDetailsQuery, QuizDetailsDto>
        {
            private readonly IMapper _mapper;
            private readonly ISSWRewardsDbContext _context;
            private readonly ICurrentUserService _userService;

            public Handler(
                IMapper mapper,
                ISSWRewardsDbContext context)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<QuizDetailsDto> Handle(GetQuizDetailsQuery request, CancellationToken cancellationToken)
            {
                return new QuizDetailsDto
                {
                    QuizId = 1,
                    Title = "Angular",
                    Description = "Sick Angular quiz!",
                    Questions = new List<QuizQuestionDto>
                    {
                        new QuizQuestionDto
                        {
                            QuestionId = 1,
                            Answers = new List<QuestionAnswerDto>
                            {
                                new QuestionAnswerDto
                                {
                                    QuestionAnswerId = 1,
                                    Text = "Answer 1"
                                },
                                new QuestionAnswerDto
                                {
                                    QuestionAnswerId = 2,
                                    Text = "Answer 2 with a really long bit of text to make sure that your line breaks don't completely fuck up the UI. PLEASE GOD HELP US ALL WITH XAMARIN OMG"
                                },
                                new QuestionAnswerDto
                                {
                                    QuestionAnswerId = 3,
                                    Text = "Answer 3"
                                },
                                new QuestionAnswerDto
                                {
                                    QuestionAnswerId = 4,
                                    Text = "Answer 4"
                                }
                            }
                        },
                        new QuizQuestionDto
                        {
                            QuestionId = 2,
                            Answers = new List<QuestionAnswerDto>
                            {
                                new QuestionAnswerDto
                                {
                                    QuestionAnswerId = 5,
                                    Text = "Answer 1"
                                },
                                new QuestionAnswerDto
                                {
                                    QuestionAnswerId = 6,
                                    Text = "Answer 2 with a really long bit of text to make sure that your line breaks don't completely fuck up the UI. PLEASE GOD HELP US ALL WITH XAMARIN OMG"
                                },
                                new QuestionAnswerDto
                                {
                                    QuestionAnswerId = 7,
                                    Text = "Answer 3"
                                },
                                new QuestionAnswerDto
                                {
                                    QuestionAnswerId = 8,
                                    Text = "Answer 4"
                                }
                            }
                        }
                    }
                };
            }
        }

    }
}
