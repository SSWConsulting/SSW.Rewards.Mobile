namespace Shared.DTOs.Quizzes;

public class QuizResultDto
{
    public int QuizId { get; set; }
    public bool Passed { get; set; } = false;
    public List<QuestionResultDto> Results { get; set; }
    public int Points { get; set; } = 0;

    public QuizResultDto()
    {
        this.Results = new List<QuestionResultDto>();
    }
}
