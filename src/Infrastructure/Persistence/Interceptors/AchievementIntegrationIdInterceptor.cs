using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SSW.Rewards.Domain.Entities;

namespace SSW.Rewards.Infrastructure.Persistence.Interceptors;
public class AchievementIntegrationIdInterceptor : SaveChangesInterceptor
{

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;
        
        var integrationIds = dbContext.Set<Achievement>().Select(a => a.IntegrationId).ToHashSet();

        foreach (var entry in dbContext.ChangeTracker.Entries<Achievement>())
        {
            if (entry.State == EntityState.Added)
            {
                while (integrationIds.Contains(entry.Entity.IntegrationId))
                {
                    entry.Entity.GenerateIntegrationId();
                }

                // Add the new IntegrationId to the set so it will be considered for other new Achievements
                integrationIds.Add(entry.Entity.IntegrationId);
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
