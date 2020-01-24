using Microsoft.EntityFrameworkCore;
using NanoSurvey.DB;
using NanoSurvey.Generator;
using System;

namespace NanoSurvey.Test
{
	public class TestSurveyContext : SurveyContext
	{
		public TestSurveyContext(DbContextOptions<SurveyContext> options) : base(options)
		{
		}

		public override void Dispose()
		{
		}

		public void TestDispose()
		{
			base.Dispose();
		}
	}

	public class DataBaseFixture : IDisposable
	{
		readonly TestSurveyContext _context;
		public SurveyContext Survey => _context;
		public DataBaseFixture()
		{
			_context = new TestSurveyContext(
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
		}

		public void Dispose()
		{
			_context.TestDispose();
		}
	}
}
