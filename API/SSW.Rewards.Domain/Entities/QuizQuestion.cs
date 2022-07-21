using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SSW.Rewards.Domain.Entities
{
    public class QuizQuestion : Entity
    {
        public int QuizId { get; set; }
        public virtual Quiz Quiz { get; set; }
        public string Text { get; set; }
        public virtual ICollection<QuizAnswer> Answers { get; set; }

        public QuizQuestion()
        {
            this.Answers = new HashSet<QuizAnswer>();
        }
    }
}
