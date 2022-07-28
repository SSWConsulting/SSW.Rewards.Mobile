using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Quizzes.Queries.ValidateQuiz;
public class QuizAnswerDto
{
    public int QuestionId { get; set; }
    public int SelectedAnswerId { get; set; }
}
