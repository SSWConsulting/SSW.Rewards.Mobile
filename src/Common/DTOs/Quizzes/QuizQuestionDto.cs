namespace SSW.Rewards.Shared.DTOs.Quizzes;

public class QuizQuestionDto
{
    public int QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public IList<QuestionAnswerDto> Answers { get; set; } = new List<QuestionAnswerDto>();
}
