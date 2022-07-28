using AutoMapper;

namespace SSW.Rewards.Application.Quizzes.Queries.GetQuizDetailsQuery
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Domain.Entities.Quiz, QuizDetailsDto>()
                .ForMember(dst => dst.QuizId, opt => opt.MapFrom(src => src.Id));

            CreateMap<Domain.Entities.QuizQuestion, QuizQuestionDto>()
                .ForMember(dst => dst.QuestionId, opt => opt.MapFrom(src => src.Id));

            CreateMap<Domain.Entities.QuizAnswer, QuestionAnswerDto>()
                .ForMember(dst => dst.QuestionAnswerId, opt => opt.MapFrom(src => src.Id));
        }
    }
}
