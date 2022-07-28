using System.Collections.Generic;

namespace SSW.Rewards.Application.Quizzes.Queries.GetQuizDetailsQuery
{
    public class QuizDetailsDto
    {
        public int QuizId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public List <QuizQuestionDto> Questions { get; set; }

        public QuizDetailsDto()
        {
            this.Questions = new List<QuizQuestionDto>();
        }

    }

    public class QuizQuestionDto
    {
        public int QuestionId { get; set; }
        public string Text { get; set; }
        public List<QuestionAnswerDto> Answers { get; set; }

        public QuizQuestionDto()
        {
            this.Answers = new List<QuestionAnswerDto>();
        }
    }

    public class QuestionAnswerDto
    {
        public int QuestionAnswerId { get; set; }
        public string Text { get; set; }
    }
}
