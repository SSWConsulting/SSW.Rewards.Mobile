namespace SSW.Rewards.Application.Skills.Commands.UpsertSkill;

public class UpsertSkillCommand : IRequest<Unit>
{
    public int Id { get; set; }
    public string Skill { get; set; }
}

public class UpsertSkillCommandHandler : IRequestHandler<UpsertSkillCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;

    public UpsertSkillCommandHandler(IApplicationDbContext context, IDateTime dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<Unit> Handle(UpsertSkillCommand request, CancellationToken cancellationToken)
    {
        var found = _context.Skills.FirstOrDefault(x => x.Id == request.Id);

        if (found == null)
        {
            _context.Skills.Add(new Skill
            {
                Name = request.Skill,
                CreatedUtc = _dateTime.Now
            });
        }
        else
        {
            found.Name = request.Skill;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
