using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NanoSurvey.DB
{
[Table("Answer", Schema = "survey")]
    public partial class Answer
    {
        public Answer()
        {
            SurveyQuestionAnswer = new HashSet<SurveyQuestionAnswer>();
        }

        public Guid Id { get; set; }
        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [InverseProperty("Answer")]
        public virtual ICollection<SurveyQuestionAnswer> SurveyQuestionAnswer { get; set; }
    }
}
