using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NanoSurvey.DB;
using NanoSurvey.Web;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NanoSurvey.Test
{
	public class QuestionControllerTest : IClassFixture<DataBaseFixture>
	{
		readonly DataBaseFixture _database;
		readonly QuestionController _controller;

		public QuestionControllerTest(DataBaseFixture database)
		{
			_database = database;

			var defaultSurvey = database.Survey.Survey
					.Include(s => s.SurveyQuestion)
						.ThenInclude(s => s.Question)
					.Include(s => s.SurveyQuestionAnswer)
						.ThenInclude(s => s.Answer)
					.OrderBy(s => s.Id)
					.FirstOrDefault();

			_controller = new QuestionController(database.Survey, () => database.Survey, () => defaultSurvey);
		}

		[Fact]
		public async Task SetResult_WrongAnswers()
		{
			var survey = _database.GetDefaultSurvey();

			SurveyQuestion questionDb = survey.SurveyQuestion.First(q => q.PreviousQuestionId == null);
			SurveyQuestionAnswer[] answers = survey.SurveyQuestionAnswer.Where(a => a.QuestionId == questionDb.QuestionId).ToArray();
			Guid[] selected = answers.Take(questionDb.Count).Select(a => Guid.NewGuid()).ToArray();

			var actionResult = await _controller.SetResult(selected, null);
			var notFound = Assert.IsType<BadRequestObjectResult>(actionResult);
		}

		[Fact]
		public async Task Survey_Complete()
		{
			long? nextId = null;
			do
			{
				nextId = await CorrectAnswerQuestionAndGetNext(nextId);
			} while (nextId != null);
		}

		public async Task<long?> CorrectAnswerQuestionAndGetNext(long? nextId)
		{
			IActionResult actionResult = await _controller.GetQuestion(nextId);
			var okResult = Assert.IsType<OkObjectResult>(actionResult);
			var model = Assert.IsType<Web.Question>(okResult.Value);

			var surveyQuestion = _database.GetQuestionByNextId(nextId);
			var answers = _database.GetAnswersByNextId(nextId);

			Assert.NotNull(model);
			Assert.NotEmpty(model.Answers);
			Assert.False(string.IsNullOrEmpty(model.Title));
			Assert.Equal(surveyQuestion.Question.Title, model.Title);
			Assert.True(model.Answers.All(a => answers.Any(db => db.Id == a.Id && db.Answer.Title == a.Title)));

			Guid[] selected = answers.Take(surveyQuestion.Count).Select(a => a.Id).ToArray();

			actionResult = await _controller.SetResult(selected, nextId);
			okResult = Assert.IsType<OkObjectResult>(actionResult);
			var resultAnswer = Assert.IsType<SetResultAnswer>(okResult.Value);

			var previousResult = _database.GetPreviousesultByNextId(nextId);
			if (previousResult == null)
				return resultAnswer.NextId;

			Assert.NotNull(previousResult);
			var interview = previousResult.Interview;

			var question = _database.GetQuestionByNextId(nextId);
			var anwers = _database.GetAnswersByNextId(nextId);

			var result = previousResult.Interview.Result.SingleOrDefault(r => r.QuestionId == question.QuestionId);
			Assert.NotNull(result);
			var selectedAnswersIds = anwers.Where(surveyAnswer => selected.Contains(surveyAnswer.Id)).Select(a => a.AnswerId).ToArray();
			Assert.True(result.ResultAnswer.All(a => selectedAnswersIds.Contains(a.AnswerId)));

			if (resultAnswer.NextId != null)
			{
				var nextQuestion = _database.GetQuestionByNextId(resultAnswer.NextId);
				var nextResult = interview.Result.SingleOrDefault(r => r.QuestionId == nextQuestion.QuestionId);
				Assert.NotNull(nextResult);
				Assert.Empty(nextResult.ResultAnswer);
			}

			return resultAnswer.NextId;
		}
	}
}
