namespace SSW.Rewards.Shared.DTOs.Quizzes;

public class QuestionResultDto
{
    public int Index { get; set; }
    public int QuestionId { get; set; }
    public bool Correct { get; set; }

    // new fields for GPT quiz engine
    public string QuestionText { get; set; } = string.Empty;
    public string AnswerText { get; set; } = string.Empty;
    public string ExplanationText { get; set; } = string.Empty;
    public int Confidence { get; set; }
}
