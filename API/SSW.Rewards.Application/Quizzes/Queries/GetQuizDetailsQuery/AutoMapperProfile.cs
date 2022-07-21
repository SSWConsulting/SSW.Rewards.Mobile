using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace SSW.Rewards.Application.Quizzes.Queries.GetQuizDetailsQuery
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Domain.Entities.Quiz, QuizDetailsDto>()
                .ForMember(dst => dst.QuizId, opt => opt.MapFrom(src => src.Id));
            //.ForMember(dst => dst.Questions, opt => opt.MapFrom(src => src.Questions));

            CreateMap<Domain.Entities.QuizQuestion, QuizQuestionDto>()
                .ForMember(dst => dst.QuestionId, opt => opt.MapFrom(src => src.Id));
                //.ForMember(dst => dst.Answers, opt => opt.MapFrom(src => src.Answers));

            CreateMap<Domain.Entities.QuizAnswer, QuestionAnswerDto>()
                .ForMember(dst => dst.QuestionAnswerId, opt => opt.MapFrom(src => src.Id));
        }
    }
}
