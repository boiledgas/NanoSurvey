﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NanoSurvey.DB
{
[Table("Survey", Schema = "survey")]
    public partial class Survey
    {
        public Survey()
        {
            SurveyQuestion = new HashSet<SurveyQuestion>();
            SurveyQuestionAnswer = new HashSet<SurveyQuestionAnswer>();
        }

        public Guid Id { get; set; }
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [InverseProperty("Survey")]
        public virtual ICollection<SurveyQuestion> SurveyQuestion { get; set; }
        [InverseProperty("Survey")]
        public virtual ICollection<SurveyQuestionAnswer> SurveyQuestionAnswer { get; set; }
    }
}
