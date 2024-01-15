﻿namespace SSW.Rewards.Shared.DTOs.Quizzes;

public class QuizEditDto
{
    public int QuizId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Points { get; set; }
    public Icons Icon { get; set; }
    public bool IsArchived { get; set; }
    public DateTime DateCreated { get; set; }
    public IList<QuizQuestionEditDto> Questions { get; set; } = new List<QuizQuestionEditDto>();
}