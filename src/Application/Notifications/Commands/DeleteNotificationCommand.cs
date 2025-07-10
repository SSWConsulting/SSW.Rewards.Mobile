using SSW.Rewards.Application.Common.Exceptions;

namespace SSW.Rewards.Application.Notifications.Commands;

public record DeleteNotificationCommand(int Id) : IRequest;

public class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;

    public DeleteNotificationCommandHandler(IApplicationDbContext context, IDateTime dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = await _context.Notifications
            .TagWithContext()
            .FirstOrDefaultAsync(n => n.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Notification), request.Id);

        notification.DeletedUtc = _dateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
