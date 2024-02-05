using System.Text.Json.Serialization;

namespace SSW.Rewards.Application.Quizzes.Commands.SubmitAnswerCommand;
public class QuizGPTResponseDto
{
    [JsonPropertyName("question")]
    public string Question { get; set; } = string.Empty;

    [JsonPropertyName("answer")]
    public string Answer { get; set; } = string.Empty;

    [JsonPropertyName("correct")]
    public bool Correct { get; set; }

    [JsonPropertyName("explanation")]
    public string Explanation { get; set; } = string.Empty;

    [JsonPropertyName("confidence")]
    public int Confidence { get; set; }

    [JsonPropertyName("usedBenchmarkAnswer")]
    public bool UsedBenchmarkAnswer { get; set; }
}
