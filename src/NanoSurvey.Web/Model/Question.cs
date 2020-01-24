using System.Collections.Generic;

namespace NanoSurvey.Web
{
	/// <summary>
	/// Вопрос анкеты.
	/// </summary>
	public class Question
	{
		/// <summary>
		/// Заголовок вопроса.
		/// </summary>
		public string Title { get; set; }
		/// <summary>
		/// Количество ответов, которое нужно указать.
		/// </summary>
		public int Count { get; set; }
		/// <summary>
		/// Имеется следующий вопрос или нет.
		/// </summary>
		public bool HasNext { get; set; }
		/// <summary>
		/// Доступные ответы.
		/// </summary>
		public IList<Answer> Answers { get; set; } = new List<Answer>();
	}
}
