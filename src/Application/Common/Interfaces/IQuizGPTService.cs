using SSW.Rewards.Application.Quizzes.Commands.SubmitAnswerCommand;

namespace SSW.Rewards.Application.Common.Interfaces;

public interface IQuizGPTService
{
    void ProcessAnswer(int userId, QuizGPTRequestDto payload, SubmitAnswerCommand request);
}
