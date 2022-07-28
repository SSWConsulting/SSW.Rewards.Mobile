using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SSW.Rewards.Application.Common.Exceptions;
using SSW.Rewards.Application.Common.Interfaces;

namespace SSW.Rewards.Application.Skills.Commands.DeleteSkill;
public class DeleteSkill : IRequest<Unit>
{
    public int Id { get; set; }

    public DeleteSkill(int id)
    {
        Id = id;
    }
}

public class DeleteSkillHandler : IRequestHandler<DeleteSkill, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteSkillHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteSkill request, CancellationToken cancellationToken)
    {
        if (request?.Id == null)
        {
            throw new Exception("Bad Request");
        }

        var found = _context.Skills.FirstOrDefault(x => x.Id == request.Id);

        if (found == null)
        {
            throw new NotFoundException("Bad Request");
        }

        _context.Skills.Remove(found);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}