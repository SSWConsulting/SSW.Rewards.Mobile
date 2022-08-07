using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Application.Quizzes.Commands.SubmitUserQuiz;
using SSW.Rewards.Application.Quizzes.Queries.Common;
using SSW.Rewards.Application.Quizzes.Queries.GetQuizDetailsQuery;
using SSW.Rewards.Application.Quizzes.Queries.GetQuizListForUserQuery;

namespace SSW.Rewards.WebAPI.Controllers;

[Authorize]
public class QuizzesController : ApiControllerBase
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
    // See: https://github.com/SSWConsulting/SSW.Rewards.API/issues/5
}