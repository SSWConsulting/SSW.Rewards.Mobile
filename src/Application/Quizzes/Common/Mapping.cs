using SSW.Rewards.Shared.DTOs.Quizzes;

namespace SSW.Rewards.Application.Quizzes.Common;
public class Mapping : Profile
{
        public Mapping()
        {
                CreateMap<Quiz, QuizDetailsDto>()
                        .ForMember(dst => dst.QuizId, opt => opt.MapFrom(src => src.Id))
                        .ForMember(dst => dst.DateCreated, opt => opt.MapFrom(src => src.CreatedUtc))
                        .ForMember(dst => dst.Points, opt => opt.MapFrom(src => src.Achievement.Value));

                CreateMap<QuizQuestion, QuizQuestionDto>()
                        .ForMember(dst => dst.QuestionId, opt => opt.MapFrom(src => src.Id));

                CreateMap<QuizAnswer, QuestionAnswerDto>()
                        .ForMember(dst => dst.QuestionAnswerId, opt => opt.MapFrom(src => src.Id))
                        .ForMember(dst => dst.IsCorrect, opt => opt.Ignore());
        }
}
