using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NanoSurvey.DB
{
[Table("Question", Schema = "survey")]
    public partial class Question
    {
        public Question()
        {
            SurveyQuestionAnswer = new HashSet<SurveyQuestionAnswer>();
            SurveyQuestionNextQuestion = new HashSet<SurveyQuestion>();
            SurveyQuestionPreviousQuestion = new HashSet<SurveyQuestion>();
            SurveyQuestionQuestion = new HashSet<SurveyQuestion>();
        }

        public Guid Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Help { get; set; }

        [InverseProperty("Question")]
        public virtual ICollection<SurveyQuestionAnswer> SurveyQuestionAnswer { get; set; }
        [InverseProperty("NextQuestion")]
        public virtual ICollection<SurveyQuestion> SurveyQuestionNextQuestion { get; set; }
        [InverseProperty("PreviousQuestion")]
        public virtual ICollection<SurveyQuestion> SurveyQuestionPreviousQuestion { get; set; }
        [InverseProperty("Question")]
        public virtual ICollection<SurveyQuestion> SurveyQuestionQuestion { get; set; }
    }
}
