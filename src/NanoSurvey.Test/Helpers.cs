using Microsoft.EntityFrameworkCore;
using NanoSurvey.DB;
using System;
using System.Linq;

namespace NanoSurvey.Test
{
	public static class Helpers
	{
		public static Survey GetDefaultSurvey(this DataBaseFixture database)
		{
			Survey survey = database.Survey.Survey
				.Include(s => s.SurveyQuestion)
					.ThenInclude(q => q.Question)
				.Include(s => s.SurveyQuestionAnswer)
					.ThenInclude(q => q.Answer)
				.OrderBy(s => s.Id)
				.First();
			return survey;
		}
		public static Survey GetSurveyByNextId(this DataBaseFixture database, long? id)
		{
			return id != null
				? database.Survey.Result.Include(r => r.Interview).Single(r => r.Id == id).Interview.Survey
				: database.GetDefaultSurvey();
		}
		public static SurveyQuestion GetQuestionByNextId(this DataBaseFixture database, long? id)
		{
			var survey = database.GetSurveyByNextId(id);

			Guid questionId = id != null
				? database.Survey.Result.Single(r => r.Id == id).QuestionId
				: survey.SurveyQuestion.Single(q => q.PreviousQuestionId == null).QuestionId;

			return survey.SurveyQuestion.Single(q => q.QuestionId == questionId);
		}
		public static SurveyQuestionAnswer[] GetAnswersByNextId(this DataBaseFixture database, long? id)
		{
			var survey = database.GetSurveyByNextId(id);
			var question = database.GetQuestionByNextId(id);

			return survey.SurveyQuestionAnswer.Where(a => a.QuestionId == question.QuestionId).ToArray();
		}
		public static Result GetResultByNextId(this DataBaseFixture database, long? id)
		{
			if (id == null)
			{
				var question = database.GetQuestionByNextId(id);
				id = database.Survey.Result.SingleOrDefault(r => r.QuestionId == question.QuestionId && r.Interview.SurveyId == question.SurveyId).Id;
			}

			var result = database.Survey.Result
				.Include(r => r.Interview)
				.Include(r => r.ResultAnswer)
				.Single(r => r.Id == id);
			return result;
		}
		public static Result GetPreviousesultByNextId(this DataBaseFixture database, long? id)
		{
			if (id == null)
				return null;

			var result = database.GetResultByNextId(id);
			var survey = database.GetSurveyByNextId(id);
			var previousId = survey.SurveyQuestion.First(q => q.NextQuestionId == result.QuestionId).QuestionId;

			var previousResult = database.Survey.Result
				.Include(r => r.Interview)
				.Include(r => r.ResultAnswer)
				.Single(r => r.InterviewId == result.InterviewId && r.QuestionId == previousId);
			return result;
		}
	}
}
