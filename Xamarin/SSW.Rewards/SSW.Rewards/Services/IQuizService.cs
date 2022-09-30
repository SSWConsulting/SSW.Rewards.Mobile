﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace SSW.Rewards.Services
{
    public interface IQuizService
    {
        Task<IEnumerable<QuizDto>> GetQuizzes();

        Task<QuizDetailsDto> GetQuizDetails(int quizID);

        Task<QuizResultDto> SubmitQuiz(SubmitUserQuizCommand command);
    }
}