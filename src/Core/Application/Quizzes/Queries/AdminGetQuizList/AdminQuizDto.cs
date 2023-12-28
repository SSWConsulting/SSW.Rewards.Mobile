namespace SSW.Rewards.Application.Quizzes.Queries.AdminGetQuizList;
public class AdminQuizDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Points { get; set; }
    public DateTime DateCreated { get; set; }
}
