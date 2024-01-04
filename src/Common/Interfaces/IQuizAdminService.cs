using Shared.DTOs.Quizzes;

namespace Shared.Interfaces;

public interface IQuizAdminService
{
    Task<int> AddNewQuiz(QuizDetailsDto quizDetailsDto, CancellationToken cancellationToken);
    Task<int> UpdateQuiz(QuizDetailsDto quizDetailsDto, CancellationToken cancellationToken);


}
