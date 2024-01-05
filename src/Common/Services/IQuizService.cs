using SSW.Rewards.Shared.DTOs.Quizzes;

namespace SSW.Rewards.Shared.Services;

public interface IQuizService
{
    Task<IEnumerable<QuizDto>> GetQuizzes(CancellationToken cancellationToken);

    Task<IEnumerable<QuizDetailsDto>> GetQuizDetails(CancellationToken cancellationToken);

    Task<QuizDetailsDto> GetQuizDetails(int quizID, CancellationToken cancellationToken);

    Task<QuizResultDto> SubmitQuiz(QuizSubmissionDto submission, CancellationToken cancellationToken);
}
