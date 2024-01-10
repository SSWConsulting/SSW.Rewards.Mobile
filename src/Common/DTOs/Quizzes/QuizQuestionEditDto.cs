namespace SSW.Rewards.Shared.DTOs.Quizzes;

public class QuizQuestionEditDto
{
    public int QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public IList<QuestionAnswerEditDto> Answers { get; set; } = new List<QuestionAnswerEditDto>();
}
