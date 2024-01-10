using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Shared.DTOs.Quizzes;
using SSW.Rewards.Application.Quizzes.Commands.AddNewQuiz;
using SSW.Rewards.Application.Quizzes.Commands.SubmitUserQuiz;
using SSW.Rewards.Application.Quizzes.Queries.GetQuizListForUser;
using SSW.Rewards.WebAPI.Authorisation;
using SSW.Rewards.Application.Quizzes.Queries.GetAdminQuizList;
using SSW.Rewards.Application.Quizzes.Queries.GetAdminQuizDetails;
using SSW.Rewards.Application.Quizzes.Queries.GetQuizDetails;

namespace SSW.Rewards.WebAPI.Controllers;

[Authorize]
public class QuizzesController : ApiControllerBase
{

    [Authorize(Roles = AuthorizationRoles.Admin)]
    [HttpPost]
    public async Task<ActionResult<int>> AddNewQuiz(QuizEditDto quiz)
    {
        return Ok(await Mediator.Send(new AdminAddNewQuiz { NewQuiz = quiz }));
    }

    [Authorize(Roles = AuthorizationRoles.Admin)]
    [HttpPut]
    public async Task<ActionResult<int>> UpdateQuiz(QuizEditDto quiz)
    {
        return Ok(await Mediator.Send(new AdminUpdateQuiz { Quiz = quiz }));
    }

    [Authorize(Roles = AuthorizationRoles.Admin)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<QuizDetailsDto>>> GetAdminQuizList()
    {
        return Ok(await Mediator.Send(new GetAdminQuizListQuery()));
    }

    [Authorize(Roles = AuthorizationRoles.Admin)]
#pragma warning disable ASP0023 // Route conflict detected between controller actions - base controller uses the action as the route
    [HttpGet("{id}")]
#pragma warning restore ASP0023 // Route conflict detected between controller actions
    public async Task<ActionResult<QuizEditDto>> GetAdminQuizEdit(int id)
    {
        return Ok(await Mediator.Send(new GetAdminQuizDetailsQuery(id)));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<QuizDto>>> GetQuizListForUser()
    {
        return Ok(await Mediator.Send(new GetQuizListForUser()));
    }

#pragma warning disable ASP0023 // Route conflict detected between controller actions - base controller uses the action as the route
    [HttpGet("{id}")]
#pragma warning restore ASP0023 // Route conflict detected between controller actions
    public async Task<ActionResult<QuizDetailsDto>> GetQuizDetails(int id)
    {
        return Ok(await Mediator.Send(new GetQuizDetails(id)));
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