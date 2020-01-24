using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NanoSurvey.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace NanoSurvey.Web
{

	/// <summary>
	/// Контроллер апи для работы с вопросами анкеты.
	/// </summary>
	[ApiController]
	[Route("api/[controller]")]
	[Produces("application/json")]
	public class QuestionController : ControllerBase
	{
		readonly SurveyContext _readonlyContext;
		readonly Func<SurveyContext> _writeContext;
		readonly Func<Survey> _defaultSurveyGetter;

		/// <param name="surveyContext">Контекст БД для получения информации по анкете.</param>
		/// <param name="resultContext">Контекст БД для работы с ответами на вопросы анкеты.</param>
		/// <param name="defaultSurveyGetter"></param>
		public QuestionController(SurveyContext surveyContext, Func<SurveyContext> resultContext, Func<Survey> defaultSurveyGetter)
		{
			_readonlyContext = surveyContext;
			_writeContext = resultContext;
			_defaultSurveyGetter = defaultSurveyGetter;
		}

		/// <summary>
		/// Получение вопроса по идентификатору. 
		/// Если идентификатор не передан, возвращается первый вопрос актуальной анкеты.
		/// Актуальная анкета выбирается случайным образом.
		/// </summary>
		/// <param name="id">Идентификатор вопроса интервью. </param>
		/// <returns>Вопрос анкеты с вариантами ответов.</returns>
		/// <response code="200">Возвращает вопрос анкеты с вариантами ответов.</response>
		/// <response code="404">Не найден вопрос анкеты с указанным идентификатором.</response>
		[HttpGet]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetQuestion(long? id = null)
		{
			var response = new Question();

			if (id != null)
			{
				// получения вопроса по идентификатору
				var result = await (
						from r in _readonlyContext.Result
						join q in _readonlyContext.SurveyQuestion on new { r.Interview.SurveyId, r.QuestionId } equals new { q.SurveyId, q.QuestionId }
						join a in _readonlyContext.SurveyQuestionAnswer on new { q.SurveyId, q.QuestionId } equals new { a.SurveyId, a.QuestionId }
						where r.Id == id.Value && !r.ResultAnswer.Any()
						select new
						{
							q.SurveyId,
							q.QuestionId,
							QuestionTitle = q.Question.Title,
							q.Count,
							q.NextQuestionId,
							AnswerId = a.Id,
							AnswerTitle = a.Answer.Title,
							a.OrderNum
						}
					)
					.ToArrayAsync();
				if (!result.Any())
					return NotFound();

				var first = result.First();
				response.Title = first.QuestionTitle;
				response.Count = first.Count;
				response.HasNext = first.NextQuestionId != null;
				response.Answers = result
					.OrderBy(r => r.OrderNum).ThenBy(r => r.AnswerTitle)
					.Select(r => new Answer { Id = r.AnswerId, Title = r.AnswerTitle }).ToArray();
			} 
			else
			{
				var survey = _defaultSurveyGetter();
				var question = survey.SurveyQuestion.FirstOrDefault(q => q.PreviousQuestionId == null);
				if (question == null)
					return NotFound();

				var answers = survey.SurveyQuestionAnswer
					.Where(a => a.QuestionId == question.QuestionId)
					.OrderBy(a => a.OrderNum).ThenBy(a => a.Answer.Title)
					.Select(a => new Answer { Id = a.Id, Title = a.Answer.Title } )
					.ToArray();
				response.Title = question.Question.Title;
				response.Count = question.Count;
				response.HasNext = question.NextQuestionId != null;
				response.Answers = answers;
			}

			return Ok(response);
		}

		/// <summary>
		/// Сохранение ответов на вопрос анкеты.
		/// POST - потому что выполняется только добавление ответов анкеты.
		/// </summary>
		/// <param name="id">Идентификатор вопроса интервью. Если не передан, используется первый вопрос актуальной анкеты.</param>
		/// <param name="answers">Выбранные ответы.</param>
		/// <returns></returns>
		/// <response code="200">Возвращает идентификатор следующего вопроса.</response>
		/// <response code="400">Не верно указаны входные параметры.</response>
		/// <response code="404">Не найден вопрос анкеты с указанным идентификатором.</response>
		[HttpPost]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> SetResult([FromBody] IList<Guid> answers, long? id = null)
		{
			// Выбираем вопрос из БД.
			long? interviewId = null;
			SurveyQuestion question;
			if (id != null)
			{
				var questionInfo = await (
						from r in _readonlyContext.Result
						join q in _readonlyContext.SurveyQuestion on new { r.Interview.SurveyId, r.QuestionId } equals new { q.SurveyId, q.QuestionId }
						// Нельзя изменять ответы на вопросы. 
						where r.Id == id.Value && !r.ResultAnswer.Any()
						select new { Question = q, InterviewId = r.InterviewId }
					).FirstOrDefaultAsync();

				interviewId = questionInfo?.InterviewId;
				question = questionInfo?.Question;
			}
			else
			{
				question = _defaultSurveyGetter()?.SurveyQuestion.FirstOrDefault(q => q.PreviousQuestionId == null);
			}
			if (question == null)
				return NotFound();

			// Валидация входных параметров.
			IList<SurveyQuestionAnswer> questionAnswers = await _readonlyContext.SurveyQuestionAnswer.Where(a => answers.Contains(a.Id)).ToArrayAsync();
			if (!answers.All(answerId => questionAnswers.Any(a => a.Id == answerId && a.QuestionId == question.QuestionId)))
				return BadRequest("Переданы некорректные идентификаторы ответов.");
			// Проверяем, что выбрано нужное количество ответов.
			if (question.Count != answers.Count)
				return BadRequest($"Необходимо указать {question.Count} ответов");

			using (var scope = new TransactionScope(
				TransactionScopeOption.Required,
				new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }, 
				TransactionScopeAsyncFlowOption.Enabled))
			{
				using (var context = _writeContext())
				{
					// Сохраняем выбранные ответы.
					Result result;
					if (id == null)
					{
						// Добавление нового интервью
						result = new Result
						{
							Interview = new Interview { SurveyId = question.SurveyId, ExternalId = Guid.NewGuid() },
							QuestionId = question.QuestionId,
							ResultAnswer = questionAnswers.Select(answer => new ResultAnswer { AnswerId = answer.AnswerId }).ToArray()
						};
						await context.Result.AddAsync(result);

						interviewId = result.InterviewId;
					}
					else
					{
						var resultAnswers = questionAnswers.Select(answer => new ResultAnswer { AnswerId = answer.AnswerId, ResultId = id.Value }).ToArray();
						await context.ResultAnswer.AddRangeAsync(resultAnswers);
					}

					Result nextResult = null;
					if (question.NextQuestionId != null)
					{
						// Добавляем следующий вопрос для интервью.
						nextResult = new Result
						{
							InterviewId = interviewId.Value,
							QuestionId = question.NextQuestionId.Value
						};
						await context.Result.AddAsync(nextResult);

					}

					await context.SaveChangesAsync();

					scope.Complete();

					return Ok(new SetResultAnswer
					{
						NextId = nextResult?.Id
					});
				}
			}
		}
	}
}
