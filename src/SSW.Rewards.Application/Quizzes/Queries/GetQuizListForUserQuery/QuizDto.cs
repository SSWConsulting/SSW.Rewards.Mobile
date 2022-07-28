using SSW.Rewards.Domain.Entities;

namespace SSW.Rewards.Application.Quizzes.Queries.GetQuizListForUserQuery
{
    public class QuizDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Passed { get; set; } = false;
        public Icons Icon { get; set; }
    }
}
