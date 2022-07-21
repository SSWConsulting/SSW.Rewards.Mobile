using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SSW.Rewards.Domain.Entities
{
    public class CompletedQuiz : Entity
    {
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public int QuizId { get; set; }
        public virtual Quiz Quiz { get; set; }
    }
}
