using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Quizzes.Queries.GetQuizDetails;
public class QuizDetailsDto
{
    public int QuizId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IList<QuizQuestionDto> Questions { get; set; } = new List<QuizQuestionDto>();
}

public class QuizQuestionDto
{
    public int QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public IList<QuestionAnswerDto> Answers { get; set; } = new List<QuestionAnswerDto>();
}

public class QuestionAnswerDto
{
    public int QuestionAnswerId { get; set; }
    public string Text { get; set; } = string.Empty;
}
