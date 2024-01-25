namespace SSW.Rewards.Domain.Entities;
public class SubmittedQuizAnswer : BaseEntity
{
    public int SubmissionId { get; set; }
    public CompletedQuiz Submission { get; set; }
    
    public int? QuizQuestionId { get; set; }
    public QuizQuestion? QuizQuestion { get; set; }
    public string AnswerText { get; set; } = string.Empty;
    public bool Correct { get; set; }
    public string GPTExplanation { get; set; } = string.Empty;
    public int GPTConfidence { get; set; }

    // TODO [tech debt]: Remove these fields once we are happy with the GPT iteration
    public int? AnswerId { get; set; }
    public QuizAnswer? Answer { get; set; }

}
