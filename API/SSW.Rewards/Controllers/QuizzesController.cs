using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Application.Quizzes.Commands.SubmitUserQuiz;
using SSW.Rewards.Application.Quizzes.Queries.GetQuizDetailsQuery;
using SSW.Rewards.Application.Quizzes.Queries.GetQuizListForUserQuery;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SSW.Rewards.WebAPI.Controllers
{
    [Authorize]
    public class QuizzesController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuizDto>>> GetQuizListForUser()
        {
            return Ok(await Mediator.Send(new GetQuizListForUserQuery()));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<QuizDetailsDto>> GetQuizDetails(int id)
        {
            return Ok(await Mediator.Send(new GetQuizDetailsQuery(id)));
        }

        [HttpPost]
        public async Task<ActionResult<QuizResultDto>> SubmitCompletedQuiz(SubmitUserQuizCommand quiz)
        {
            return Ok(await Mediator.Send(quiz));
        }

        // TODO: Add endpoints for Admins to be able to add/update/delete quizzes and quiz questions.
    }
}