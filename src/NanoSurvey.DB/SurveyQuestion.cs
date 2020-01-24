using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NanoSurvey.DB
{
[Table("SurveyQuestion", Schema = "survey")]
    public partial class SurveyQuestion
    {
        public Guid Id { get; set; }
        public Guid SurveyId { get; set; }
        public Guid QuestionId { get; set; }
        public Guid? NextQuestionId { get; set; }
        public Guid? PreviousQuestionId { get; set; }
        public int Count { get; set; }

        [ForeignKey("NextQuestionId")]
        [InverseProperty("SurveyQuestionNextQuestion")]
        public virtual Question NextQuestion { get; set; }
        [ForeignKey("PreviousQuestionId")]
        [InverseProperty("SurveyQuestionPreviousQuestion")]
        public virtual Question PreviousQuestion { get; set; }
        [ForeignKey("QuestionId")]
        [InverseProperty("SurveyQuestionQuestion")]
        public virtual Question Question { get; set; }
        [ForeignKey("SurveyId")]
        [InverseProperty("SurveyQuestion")]
        public virtual Survey Survey { get; set; }
    }
}
