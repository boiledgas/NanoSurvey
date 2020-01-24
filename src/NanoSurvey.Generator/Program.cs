using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NanoSurvey.DB;
using System;
using System.Collections.Generic;

namespace NanoSurvey.Generator
{
	public class SurveyGenerator
	{
		public IEnumerable<Survey> Surveys { get; }
		public IEnumerable<Question> Questions { get; }
		public IEnumerable<Answer> Answers { get; }
		public IEnumerable<SurveyQuestion> SurveyQuestions { get; }
		public IEnumerable<SurveyQuestionAnswer> SurveyQuestionAnswers { get; }

		public SurveyGenerator(IEnumerable<Survey> surveys,
			IEnumerable<Question> questions,
			IEnumerable<Answer> answers,
			IEnumerable<SurveyQuestion> surveyQuestions,
			IEnumerable<SurveyQuestionAnswer> surveyQuestionAnswers)
		{
			Surveys = surveys;
			Questions = questions;
			Answers = answers;
			SurveyQuestions = surveyQuestions;
			SurveyQuestionAnswers = surveyQuestionAnswers;
		}

		public static SurveyGenerator Generate()
		{
			var surveys = new[] {
					new
					{
						Name = "Взять ипотеку",
						Questions = new[]
						{
							new {
								Question = "В каком регионе вы проживаете?",
								Count = 1,
								Answers = new[] { "Москва", "Московская область", "Санкт-Петербург", "Ленинградская область", "Рязанская область", "Владимирская область", "Тульская область", "Другой регион" },
							},
							new {
								Question = "Вы снимаете жилье?",
								Count = 1,
								Answers = new[] { "Да", "Нет" },
							},
							new
							{
								Question = "Хотели бы Вы взять ипотеку под 6% год?",
								Count = 1,
								Answers = new[] { "Да", "Нет", "Не знаю" },
							}
						}
					},
					new
					{
						Name = "Взять кредит",
						Questions = new[]
						{
							new {
								Question = "В каком регионе вы проживаете?",
								Count = 1,
								Answers = new[] { "Москва", "Санкт-Петербург", "Другой регион" },
							},
							new {
								Question = "Что вы хотите приобрести?",
								Count = 3,
								Answers = new[] { "Микроволновую печь", "Автомобиль", "Телевизор", "Холодильник", "Стиральную машину", "Ничего" },
							},
							new
							{
								Question = "Хотели бы Вы взять кредит под 10% в год?",
								Count = 1,
								Answers = new[] { "Да", "Нет", "Не знаю" },
							}
						}
					},
					new
					{
						Name = "Взять в рассрочку",
						Questions = new[]
						{
							new {
								Question = "В каком регионе вы проживаете?",
								Count = 1,
								Answers = new[] { "Ленинградская область", "Рязанская область", "Владимирская область", "Тульская область", "Волгоградская область", "Другой регион" },
							},
							new {
								Question = "Хотели бы вы приобрести микроволновку?",
								Count = 1,
								Answers = new[] { "Да", "Нет" },
							},
							new {
								Question = "Хотели бы вы приобрести автомобиль?",
								Count = 1,
								Answers = new[] { "Да", "Нет" },
							},
							new {
								Question = "Хотели бы вы приобрести телевизор?",
								Count = 1,
								Answers = new[] { "Да", "Нет" },
							},
							new {
								Question = "Хотели бы вы приобрести холодильник?",
								Count = 1,
								Answers = new[] { "Да", "Нет" },
							},
							new
							{
								Question = "Хотели бы Вы взять рассрочку на 12 месяцев?",
								Count = 1,
								Answers = new[] { "Да", "Нет", "Не знаю" },
							}
						}
					}
				};

			var surveysIndex = new List<Survey>();
			var questionsIndex = new Dictionary<string, Question>();
			var answersIndex = new Dictionary<string, Answer>();
			var surveyQuestions = new List<SurveyQuestion>();
			var surveyQuestionAnswers = new List<SurveyQuestionAnswer>();

			for(var i = 0; i < 100; i ++)
			foreach (var survey in surveys)
			{
				var dbSurvey = new Survey
				{
					Id = Guid.NewGuid(),
					Name = survey.Name,
				};
				surveysIndex.Add(dbSurvey);

				SurveyQuestion dbPreviousQuestion = null;
				foreach (var question in survey.Questions)
				{
					Question dbQuestion;
					if (!questionsIndex.TryGetValue(question.Question, out dbQuestion))
					{
						questionsIndex.Add(question.Question, dbQuestion = new Question
						{
							Id = Guid.NewGuid(),
							Help = question.Question,
							Title = question.Question
						});
					}

					var surveyQuestion = new SurveyQuestion
					{
						Id = Guid.NewGuid(),
						Survey = dbSurvey,
						Question = dbQuestion,
						Count = question.Count
					};
					if (dbPreviousQuestion != null)
					{
						dbPreviousQuestion.NextQuestion = dbQuestion;
						surveyQuestion.PreviousQuestion = dbPreviousQuestion.Question;
					}
					surveyQuestions.Add(surveyQuestion);
					dbPreviousQuestion = surveyQuestion;

					var order = 0;
					foreach (var answer in question.Answers)
					{
						Answer dbAnswer;
						if (!answersIndex.TryGetValue(answer, out dbAnswer))
						{
							answersIndex.Add(answer, dbAnswer = new Answer
							{
								Id = Guid.NewGuid(),
								Title = answer
							});
						}
						surveyQuestionAnswers.Add(new SurveyQuestionAnswer
						{
							Id = Guid.NewGuid(),
							Survey = dbSurvey,
							Question = dbQuestion,
							Answer = dbAnswer,
							OrderNum = order++
						});
					}
				}
			}

			return new SurveyGenerator(surveysIndex,
				questionsIndex.Values,
				answersIndex.Values,
				surveyQuestions,
				surveyQuestionAnswers
			);
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			var configuration = new ConfigurationBuilder()
				.AddEnvironmentVariables()
				.AddUserSecrets<Program>()
				.Build();

			var optionsBuilder = new DbContextOptionsBuilder<SurveyContext>()
				.UseSqlServer(configuration.GetConnectionString("NanoSurvey"));

			using (var context = new SurveyContext(optionsBuilder.Options))
			{
				var data = SurveyGenerator.Generate();

				context.AddRange(data.Surveys);
				context.AddRange(data.Questions);
				context.AddRange(data.Answers);
				context.AddRange(data.SurveyQuestions);
				context.AddRange(data.SurveyQuestionAnswers);

				context.SaveChanges();
			}
		}
	}
}
