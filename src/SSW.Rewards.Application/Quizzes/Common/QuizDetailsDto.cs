using SSW.Rewards.Application.Common.Mappings;
using SSW.Rewards.Domain.Enums;

namespace SSW.Rewards.Application.Quizzes.Common;

public class QuizDetailsDto : IMapFrom<Quiz>
{
    public int QuizId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Points { get; set; }
    public Icons Icon { get; set; }
    public IList<QuizQuestionDto> Questions { get; set; } = new List<QuizQuestionDto>();

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Quiz, QuizDetailsDto>()
                .ForMember(dst => dst.QuizId, opt => opt.MapFrom(src => src.Id));
    }
}

public class QuizQuestionDto : IMapFrom<QuizQuestion>
{
    public int QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public IList<QuestionAnswerDto> Answers { get; set; } = new List<QuestionAnswerDto>();

    public void Mapping(Profile profile)
    {
        profile.CreateMap<QuizQuestion, QuizQuestionDto>()
                .ForMember(dst => dst.QuestionId, opt => opt.MapFrom(src => src.Id));
    }
}

public class QuestionAnswerDto : IMapFrom<QuizAnswer>
{
    public int QuestionAnswerId { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<QuizAnswer, QuestionAnswerDto>()
                .ForMember(dst => dst.QuestionAnswerId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dst => dst.IsCorrect, opt => opt.Ignore());
    }
}
