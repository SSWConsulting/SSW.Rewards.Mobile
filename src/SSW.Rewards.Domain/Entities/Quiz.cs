using System;
using System.Collections.Generic;

namespace SSW.Rewards.Domain.Entities
{
    public class Quiz : Entity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Icons Icon { get; set; }
        public DateTime? LastUpdatedUtc { get; set; }
        public bool IsArchived { get; set; }
        public int AchievementId { get; set; }
        public virtual Achievement Achievement { get; set; }
        public virtual ICollection<QuizQuestion> Questions { get; set; }
        public virtual ICollection<CompletedQuiz> CompletedQuizzes { get; set; }

        public Quiz()
        {
            this.Questions = new HashSet<QuizQuestion>();
            this.CompletedQuizzes = new HashSet<CompletedQuiz>();
        }
    }
}
