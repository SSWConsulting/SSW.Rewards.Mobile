namespace SSW.Rewards.Application.Quizzes.Commands.SubmitUserQuiz;
public class QuizResultDto
{
    public int QuizId { get; set; }
    public bool Passed { get; set; }
    public List<QuestionResultDto> Results { get; set; } = new List<QuestionResultDto>();
}

public class QuestionResultDto
{
    public int QuestionId { get; set; }
    public bool Correct { get; set; }
}
