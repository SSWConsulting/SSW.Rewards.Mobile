using SSW.Rewards.Shared.DTOs.Quizzes;

namespace SSW.Rewards.Mobile.Services;

public interface IQuizService
{
    Task<IEnumerable<QuizDto>> GetQuizzes();

    Task<QuizDetailsDto> GetQuizDetails(int quizID);

    Task<QuizResultDto> SubmitQuiz(QuizSubmissionDto dto);

    Task<BeginQuizDto> BeginQuiz(int quizId);

    Task SubmitAnswer(SubmitQuizAnswerDto dto);

    Task<bool?> CheckQuizCompletion(int submissionId);

    Task<QuizResultDto> GetQuizResults(int submissionId);
}
