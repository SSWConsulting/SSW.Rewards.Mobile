namespace SSW.Rewards.Shared.DTOs.Skills;

public class SkillEditDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImageUri { get; set; }
    public string ImageBytesInBase64 { get; set; }
    public string ImageFileName { get; set; }
}