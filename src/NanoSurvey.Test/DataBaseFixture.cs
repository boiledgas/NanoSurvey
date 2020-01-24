using Microsoft.EntityFrameworkCore;
using NanoSurvey.DB;
using NanoSurvey.Generator;
using System;

namespace NanoSurvey.Test
{
	public class DataBaseFixture : IDisposable
	{
		public SurveyContext Survey { get; }
		public ResultContext Results { get; }

		public DataBaseFixture()
		{
			Survey = new SurveyContext(
				new DbContextOptionsBuilder<SurveyContext>()
					.UseInMemoryDatabase("NanoSurvey")
					.Options);
			var data = SurveyGenerator.Generate();

			Survey.AddRange(data.Surveys);
			Survey.AddRange(data.Questions);
			Survey.AddRange(data.Answers);
			Survey.AddRange(data.SurveyQuestions);
			Survey.AddRange(data.SurveyQuestionAnswers);

			Survey.SaveChanges();

			Results = new ResultContext(
				new DbContextOptionsBuilder<ResultContext>()
					.UseInMemoryDatabase("NanoSurvey")
					.Options);
		}

		public void Dispose()
		{
			Survey.Dispose();
			Results.Dispose();
		}
	}
}
