using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Quizzes;
using SSW.Rewards.Application.Quizzes.Commands.AddNewQuiz;
using SSW.Rewards.Application.Quizzes.Commands.SubmitUserQuiz;
using SSW.Rewards.Application.Quizzes.Queries.GetQuizDetailsQuery;
using SSW.Rewards.Application.Quizzes.Queries.GetQuizListForUser;
using SSW.Rewards.WebAPI.Authorisation;

namespace SSW.Rewards.WebAPI.Controllers;

[Authorize]
public class QuizzesController : ApiControllerBase
{

    [Authorize(Roles = AuthorizationRoles.Admin)]
    [HttpPost]
    public async Task<ActionResult<int>> AddNewQuiz(QuizDetailsDto quiz)
    {
        return Ok(await Mediator.Send(new AdminAddNewQuiz { NewQuiz = quiz }));
    }

    [Authorize(Roles = AuthorizationRoles.Admin)]
    [HttpPut]
    public async Task<ActionResult<int>> UpdateQuiz(QuizDetailsDto quiz)
    {
        return Ok(await Mediator.Send(new AdminUpdateQuiz { Quiz = quiz }));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<QuizDto>>> GetQuizListForUser()
    {
        return Ok(await Mediator.Send(new GetQuizListForUser()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<QuizDetailsDto>> GetQuizDetails(int id)
    {
        return Ok(await Mediator.Send(new GetQuizDetailsQuery(id)));
    }

    [HttpPost]
    public async Task<ActionResult<QuizResultDto>> SubmitCompletedQuiz(QuizSubmissionDto dto)
    {
        return Ok(await Mediator.Send(new SubmitUserQuizCommand
        {
            QuizId = dto.QuizId,
            Answers = dto.Answers.ToList()
        }));
    }

    // TODO: Add endpoints for Admins to be able to add/update/delete quizzes and quiz questions.
    // See: https://github.com/SSWConsulting/SSW.Rewards.API/issues/5
}