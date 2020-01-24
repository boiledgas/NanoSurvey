using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NanoSurvey.DB
{
[Table("Result", Schema = "result")]
    public partial class Result
    {
        public Result()
        {
            ResultAnswer = new HashSet<ResultAnswer>();
        }

        public long Id { get; set; }
        public long InterviewId { get; set; }
        public Guid QuestionId { get; set; }

        [ForeignKey("InterviewId")]
        [InverseProperty("Result")]
        public virtual Interview Interview { get; set; }
        [ForeignKey("QuestionId")]
        [InverseProperty("Result")]
        public virtual Question Question { get; set; }
        [InverseProperty("Result")]
        public virtual ICollection<ResultAnswer> ResultAnswer { get; set; }
    }
}
