namespace SSW.Rewards.Shared.DTOs.Quizzes;

public class QuizQuestionDto
{
    public int QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    
    // TODO [tech debt]: Remove when GPT quiz engine is working
    public IList<QuestionAnswerDto> Answers { get; set; } = new List<QuestionAnswerDto>();
}
