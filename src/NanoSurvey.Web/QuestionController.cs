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
		readonly SurveyContext _surveyContext;
		readonly ResultContext _resultContext;
		readonly Func<Survey> _defaultSurveyGetter;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="surveyContext">Контекст БД для получения информации по анкете.</param>
		/// <param name="resultContext">Контекст БД для работы с ответами на вопросы анкеты.</param>
		/// <param name="defaultSurveyGetter"></param>
		public QuestionController(SurveyContext surveyContext, ResultContext resultContext, Func<Survey> defaultSurveyGetter)
		{
			_surveyContext = surveyContext;
			_resultContext = resultContext;
			_defaultSurveyGetter = defaultSurveyGetter;
		}

		/// <summary>
		/// Получение вопроса по идентификатору. 
		/// Если идентификатор не передан, возвращается первый вопрос актуальной анкеты.
		/// Актуальная анкета меняется каждые 2 минуты.
		/// </summary>
		/// <param name="id">Идентификатор вопроса интервью. </param>
		/// <param name="surveyId">Идентификато опроса.</param>
		/// <returns>Вопрос анкеты с вариантами ответов.</returns>
		/// <response code="200">Возвращает вопрос анкеты с вариантами ответов.</response>
		/// <response code="404">Не найден вопрос анкеты с указанным идентификатором.</response>
		[HttpGet]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetQuestion(long? id = null, Guid? surveyId = null)
		{
			var result = id != null 
				? await _resultContext.Result
					.Include(r => r.ResultAnswer)
					.Include(r => r.Interview)
					.FirstOrDefaultAsync(q => q.Id == id)
				: null;
			if (result == null && id.HasValue)
				return NotFound();

			// Нельзя просматривать чужие ответы. 
			if (result != null && result.ResultAnswer.Any())
				return NotFound();

			SurveyQuestion question = await GetSurveyQuestion(surveyId, result);
			if (question == null)
				return NotFound();

			IList<SurveyQuestionAnswer> answers = question.Survey.SurveyQuestionAnswer.Where(a => a.QuestionId == question.QuestionId).ToArray();

			return Ok(new Question
			{
				Title = question.Question.Title,
				Count = question.Count,
				HasNext = question.NextQuestionId != null,
				Answers = answers
					.OrderBy(a => a.OrderNum).ThenBy(a => a.Answer.Title)
					.Select(a => new Answer
					{
						Id = a.Id,
						Title = a.Answer.Title,
					}).ToArray()
			});
		}

		/// <summary>
		/// Сохранение ответов на вопрос анкеты.
		/// POST - потому что выполняется только добавление ответов анкеты.
		/// </summary>
		/// <param name="id">Идентификатор вопроса интервью. Если не передан, используется первый вопрос актуальной анкеты.</param>
		/// <param name="surveyId">Идентификато опроса.</param>
		/// <param name="answers">Выбранные ответы.</param>
		/// <returns></returns>
		/// <response code="200">Возвращает идентификатор следующего вопроса.</response>
		/// <response code="400">Не верно указаны входные параметры.</response>
		/// <response code="404">Не найден вопрос анкеты с указанным идентификатором.</response>
		[HttpPost]
		public async Task<IActionResult> SetResult([FromBody] IList<Guid> answers, long? id = null, Guid? surveyId = null)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			IList<SurveyQuestionAnswer> questionAnswers = await _surveyContext.SurveyQuestionAnswer.Where(a => answers.Contains(a.Id)).ToArrayAsync();
			if (!answers.All(answerId => questionAnswers.Any(a => a.Id == answerId)))
				throw new ArgumentException("Переданы некорректные идентификаторы ответов.");

			var result = await _resultContext.Result
				.Include(r => r.Interview)
				.Include(r => r.ResultAnswer)
				.FirstOrDefaultAsync(q => q.Id == id);
			if (result == null && id.HasValue)
				NotFound();

			// Нельзя изменять ответы на вопросы. 
			bool hasAnswer = result != null && result.ResultAnswer.Any();
			if (hasAnswer)
				return NotFound();

			SurveyQuestion question = await GetSurveyQuestion(surveyId, result);
			if (question == null)
				return NotFound();
			// Проверяем, что выбрано нужное количество ответов.
			if (question.Count != answers.Count)
				return BadRequest($"Необходимо указать {question.Count} ответов");

			using (var scope = new TransactionScope(
				TransactionScopeOption.Required, 
				new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }, 
				TransactionScopeAsyncFlowOption.Enabled))
			{
				// Сохраняем выбранные ответы.
				var resultAnswers = questionAnswers.Select(answer => new ResultAnswer { AnswerId = answer.AnswerId, Result = result }).ToArray();
				if (result == null)
				{
					// Добавление нового интервью
					result = new Result
					{
						Interview = new Interview { SurveyId = question.SurveyId, ExternalId = Guid.NewGuid() },
						QuestionId = question.QuestionId,
						ResultAnswer = resultAnswers
					};
					await _resultContext.Result.AddAsync(result);
				}
				else
				{
					_resultContext.ResultAnswer.RemoveRange(result.ResultAnswer);
					await _resultContext.ResultAnswer.AddRangeAsync(resultAnswers);
				}

				Result nextResult = null;
				if (question.NextQuestionId != null)
				{
					if (nextResult == null)
					{
						// Добавляем следующий вопрос для интервью.
						nextResult = new Result
						{
							Interview = result.Interview,
							QuestionId = question.NextQuestionId.Value
						};
						await _resultContext.Result.AddAsync(nextResult);
					}
				}

				await _resultContext.SaveChangesAsync();

				scope.Complete();

				return Ok(new SetResultAnswer
				{
					NextId = nextResult?.Id
				});
			}
		}

		/// <summary>
		/// Получение вопроса анкеты.
		/// </summary>
		/// <param name="surveyId">Идентификатор анкеты.</param>
		/// <param name="result">Незаполненный результат.</param>
		/// <returns></returns>
		private async Task<SurveyQuestion> GetSurveyQuestion(Guid? surveyId, Result result)
		{
			surveyId = result?.Interview.SurveyId ?? surveyId;

			Survey survey = surveyId != null 
				? await _surveyContext.Survey
					.Include(s => s.SurveyQuestion)
						.ThenInclude(s => s.Question)
					.Include(s => s.SurveyQuestionAnswer)
						.ThenInclude(s => s.Answer)
					.SingleOrDefaultAsync(s => s.Id == surveyId)
				: _defaultSurveyGetter();
			if (survey == null)
				return null;

			SurveyQuestion question = result != null
				? survey.SurveyQuestion.Single(q => q.QuestionId == result.QuestionId)
				: survey.SurveyQuestion.Single(q => q.PreviousQuestionId == null);

			return question;
		}
	}
}
