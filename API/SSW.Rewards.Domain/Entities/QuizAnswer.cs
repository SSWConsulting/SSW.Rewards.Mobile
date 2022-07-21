namespace SSW.Rewards.Domain.Entities
{
    public class QuizAnswer : Entity
    {
        public int QuestionId { get; set; }
        public virtual QuizQuestion Question { get; set; }
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
    }
}
