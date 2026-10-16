using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Domain.Entities;
using SSW.Rewards.Infrastructure.Persistence;
using SSW.Rewards.Infrastructure.Persistence.Interceptors;

namespace SSW.Rewards.Application.UnitTests.Persistence;

[TestFixture]
public class TenantSoftDeleteTests
{
    private DbContextOptions<ApplicationDbContext> _options = null!;
    private AuditableEntitySaveChangesInterceptor _auditInterceptor = null!;

    [SetUp]
    public void Setup()
    {
        var dateTime = new Mock<IDateTime>();
        dateTime.Setup(x => x.UtcNow).Returns(new DateTime(2026, 2, 20, 12, 0, 0, DateTimeKind.Utc));

        _auditInterceptor = new AuditableEntitySaveChangesInterceptor(Mock.Of<ICurrentUserService>(), dateTime.Object);
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Test]
    public async Task RemovedTenantAndSettings_AreNotReturnedByFreshContext()
    {
        int tenantId;
        int settingsId;

        await using (var context = CreateContext())
        {
            var tenant = new Tenant { Name = "tenant", Settings = new TenantSettings { CompanyName = "company" } };
            context.Tenants.Add(tenant);
            await context.SaveChangesAsync();
            tenantId = tenant.Id;
            settingsId = tenant.Settings.Id;

            context.TenantSettings.Remove(tenant.Settings);
            context.Tenants.Remove(tenant);
            await context.SaveChangesAsync();
        }

        await using var freshContext = CreateContext();
        (await freshContext.Tenants.AnyAsync(t => t.Id == tenantId)).Should().BeFalse();
        (await freshContext.TenantSettings.AnyAsync(s => s.Id == settingsId)).Should().BeFalse();
        (await freshContext.Tenants.IgnoreQueryFilters().SingleAsync(t => t.Id == tenantId)).DeletedUtc.Should().NotBeNull();
        (await freshContext.TenantSettings.IgnoreQueryFilters().SingleAsync(s => s.Id == settingsId)).DeletedUtc.Should().NotBeNull();
    }

    private ApplicationDbContext CreateContext() =>
        new(_options, _auditInterceptor, new AchievementIntegrationIdInterceptor());
}
