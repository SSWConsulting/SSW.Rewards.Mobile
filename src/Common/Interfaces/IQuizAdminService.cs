using Shared.DTOs.Quizzes;

namespace Shared.Interfaces;

public interface IQuizAdminService
{
    Task<int> AddNewQuiz(QuizDetailsDto quizDetailsDto);
    Task<int> UpdateQuiz(QuizDetailsDto quizDetailsDto);


}
