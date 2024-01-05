using SSW.Rewards.Shared.DTOs.Quizzes;

namespace SSW.Rewards.Shared.Services;

public interface IQuizAdminService
{
    Task<int> AddNewQuiz(QuizDetailsDto quizDetailsDto, CancellationToken cancellationToken);
    Task<int> UpdateQuiz(QuizDetailsDto quizDetailsDto, CancellationToken cancellationToken);


}
