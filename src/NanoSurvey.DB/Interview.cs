using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NanoSurvey.DB
{
[Table("Interview", Schema = "result")]
    public partial class Interview
    {
        public Interview()
        {
            Result = new HashSet<Result>();
        }

        public long Id { get; set; }
        public Guid ExternalId { get; set; }
        public Guid SurveyId { get; set; }

        [InverseProperty("Interview")]
        public virtual ICollection<Result> Result { get; set; }
    }
}
