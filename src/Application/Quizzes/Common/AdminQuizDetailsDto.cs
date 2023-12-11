using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSW.Rewards.Application.Common.Mappings;
using SSW.Rewards.Domain.Enums;

namespace SSW.Rewards.Application.Quizzes.Common;
public class AdminQuizDetailsDto : IMapFrom<Quiz>
{
    public int QuizId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Points { get; set; }
    public Icons Icon { get; set; }
    public bool IsArchived { get; set; }
    public IList<AdminQuizQuestionDto> Questions { get; set; } = new List<AdminQuizQuestionDto>();

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Quiz, AdminQuizDetailsDto>()
                .ForMember(dst => dst.QuizId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dst => dst.Points, opt => opt.MapFrom(src => src.Achievement.Value));
    }
}

public class AdminQuizQuestionDto : IMapFrom<QuizQuestion>
{
    public int QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public IList<AdminQuestionAnswerDto> Answers { get; set; } = new List<AdminQuestionAnswerDto>();

    public void Mapping(Profile profile)
    {
        profile.CreateMap<QuizQuestion, AdminQuizQuestionDto>()
                .ForMember(dst => dst.QuestionId, opt => opt.MapFrom(src => src.Id));
    }
}

public class AdminQuestionAnswerDto : IMapFrom<QuizAnswer>
{
    public int QuestionAnswerId { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<QuizAnswer, AdminQuestionAnswerDto>()
                .ForMember(dst => dst.QuestionAnswerId, opt => opt.MapFrom(src => src.Id));
    }
}
