using SSW.Rewards.Application.Quizzes.Commands.SubmitAnswerCommand;
using SSW.Rewards.Shared.DTOs.Quizzes;

namespace SSW.Rewards.Application.Common.Interfaces;

public interface IQuizGPTService
{
    void ProcessAnswer(QuizGPTRequestDto payload, SubmitAnswerCommand request);
}
