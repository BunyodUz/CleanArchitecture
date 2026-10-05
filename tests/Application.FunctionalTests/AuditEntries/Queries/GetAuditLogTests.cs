using CleanArchitecture.Application.AuditEntries.Queries.GetAuditLog;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Domain.Constants;
using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Application.FunctionalTests.AuditEntries.Queries;

public class GetAuditLogTests : TestBase
{
    [Test]
    public async Task ShouldReturnNewestEntriesFirstWithTotalCount()
    {
        await TestApp.RunAsAdministratorAsync();

        var start = new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero);
        for (var i = 0; i < 3; i++)
        {
            await TestApp.AddAsync(new AuditEntry
            {
                Timestamp = start.AddMinutes(i),
                ActorName = "administrator",
                Action = AuditActions.UserCreated,
                TargetType = AuditTargets.User,
                TargetName = $"user{i}",
            });
        }

        var result = await TestApp.SendAsync(new GetAuditLogQuery { PageNumber = 1, PageSize = 2 });

        result.TotalCount.ShouldBe(3);
        result.Items.Select(e => e.TargetName).ShouldBe(["user2", "user1"]);
    }

    [Test]
    public async Task ShouldRequireAuditReadPermission()
    {
        await TestApp.RunAsDefaultUserAsync();

        await Should.ThrowAsync<ForbiddenAccessException>(() => TestApp.SendAsync(new GetAuditLogQuery()));
    }
}
