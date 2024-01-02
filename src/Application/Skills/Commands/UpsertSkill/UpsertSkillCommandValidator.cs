namespace SSW.Rewards.Application.Skills.Commands.UpsertSkill;
public class UpsertSkillCommandValidator : AbstractValidator<UpsertSkillCommand>
{
	public UpsertSkillCommandValidator()
	{
		RuleFor(c => c.Skill).NotEmpty();
	}
}
