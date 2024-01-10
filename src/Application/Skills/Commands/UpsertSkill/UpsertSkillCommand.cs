namespace SSW.Rewards.Application.Skills.Commands.UpsertSkill;

public class UpsertSkillCommand : IRequest<int>
{
    public int Id { get; set; }
    public string Skill { get; set; }
}

public class UpsertSkillCommandHandler : IRequestHandler<UpsertSkillCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;

    public UpsertSkillCommandHandler(IApplicationDbContext context, IDateTime dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<int> Handle(UpsertSkillCommand request, CancellationToken cancellationToken)
    {
        var found = _context.Skills.FirstOrDefault(x => x.Id == request.Id);

        if (found == null)
        {
            found = new Skill
            {
                Name = request.Skill,
                CreatedUtc = _dateTime.Now
            };

            _context.Skills.Add(found);
        }
        else
        {
            found.Name = request.Skill;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return found.Id;
    }
}
