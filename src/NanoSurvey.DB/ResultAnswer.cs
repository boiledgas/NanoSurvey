using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NanoSurvey.DB
{
[Table("ResultAnswer", Schema = "result")]
    public partial class ResultAnswer
    {
        public long Id { get; set; }
        public long ResultId { get; set; }
        public Guid AnswerId { get; set; }

        [ForeignKey("AnswerId")]
        [InverseProperty("ResultAnswer")]
        public virtual Answer Answer { get; set; }
        [ForeignKey("ResultId")]
        [InverseProperty("ResultAnswer")]
        public virtual Result Result { get; set; }
    }
}
