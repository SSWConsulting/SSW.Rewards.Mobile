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
using SSW.Rewards.Application.Quizzes.Commands.SubmitAnswerCommand;
using SSW.Rewards.Application.Quizzes.Commands.BeginQuiz;
using SSW.Rewards.Application.Quizzes.Queries.GetQuizQuestionsBySubmissionId;
using SSW.Rewards.Application.Quizzes.Queries.GetQuizResults;
using SSW.Rewards.Application.Quizzes.Queries.CheckQuizCompletion;

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

    // GPT quiz-related endpoint 1/4
    [HttpPost]
    public async Task<ActionResult<BeginQuizDto>> BeginQuiz([FromBody]int quizId)
    {
        int submissionId = await Mediator.Send(new BeginQuizCommand { QuizId = quizId });
        List<QuizQuestionDto> results = await Mediator.Send(new GetQuizQuestionsBySubmissionIdQuery { SubmissionId = submissionId });

        BeginQuizDto model = new BeginQuizDto
        {
            SubmissionId = submissionId,
            Questions = results
        };
        return Ok(model);
    }
    
    // GPT quiz-related endpoint 2/4
    [HttpPost]
    public async Task<ActionResult> SubmitAnswer(SubmitQuizAnswerDto model)
    {
        await Mediator.Send(new SubmitAnswerCommand { SubmissionId = model.SubmissionId, QuestionId = model.QuestionId, AnswerText = model.AnswerText });
        return Ok();
    }

    // GPT quiz-related endpoint 3/4
    [HttpGet]
    public async Task<ActionResult> CheckQuizCompletion(int submissionId)
    {
        bool b = await Mediator.Send(new CheckQuizCompletionQuery { SubmissionId = submissionId });
        if (b)
            return Ok();
        else
            return StatusCode(202);
    }
    
    // GPT quiz-related endpoint 4/3
    [HttpGet]
    public async Task<ActionResult<QuizResultDto>> GetQuizResults(int submissionId)
    {
        var results = await Mediator.Send(new GetQuizResultsQuery { SubmissionId = submissionId });
        return Ok(results);
    }

    // TODO: Add endpoints for Admins to be able to add/update/delete quizzes and quiz questions.
    // See: https://github.com/SSWConsulting/SSW.Rewards.API/issues/5
}
