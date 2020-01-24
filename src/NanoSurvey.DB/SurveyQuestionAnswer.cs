using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NanoSurvey.DB
{
[Table("SurveyQuestionAnswer", Schema = "survey")]
    public partial class SurveyQuestionAnswer
    {
        public Guid Id { get; set; }
        public Guid SurveyId { get; set; }
        public Guid QuestionId { get; set; }
        public Guid AnswerId { get; set; }
        public int OrderNum { get; set; }

        [ForeignKey("AnswerId")]
        [InverseProperty("SurveyQuestionAnswer")]
        public virtual Answer Answer { get; set; }
        [ForeignKey("QuestionId")]
        [InverseProperty("SurveyQuestionAnswer")]
        public virtual Question Question { get; set; }
        [ForeignKey("SurveyId")]
        [InverseProperty("SurveyQuestionAnswer")]
        public virtual Survey Survey { get; set; }
    }
}
