using Shared.DTOs.Quizzes;

namespace SSW.Rewards.Mobile.Services;

public interface IQuizService
{
    Task<IEnumerable<QuizDto>> GetQuizzes();

    Task<QuizDetailsDto> GetQuizDetails(int quizID);

    Task<QuizResultDto> SubmitQuiz(QuizSubmissionDto submission);
}
