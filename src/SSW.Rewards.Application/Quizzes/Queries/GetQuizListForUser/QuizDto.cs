using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSW.Rewards.Domain.Enums;

namespace SSW.Rewards.Application.Quizzes.Queries.GetQuizListForUser;
public class QuizDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Passed { get; set; } = false;
    public Icons Icon { get; set; }
}
