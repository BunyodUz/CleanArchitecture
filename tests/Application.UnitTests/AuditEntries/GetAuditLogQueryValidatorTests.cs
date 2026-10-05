using CleanArchitecture.Application.AuditEntries.Queries.GetAuditLog;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Application.UnitTests.AuditEntries;

public class GetAuditLogQueryValidatorTests
{
    private readonly GetAuditLogQueryValidator _validator = new();

    [Test]
    public void ShouldAcceptDefaults()
    {
        _validator.Validate(new GetAuditLogQuery()).IsValid.ShouldBeTrue();
    }

    [TestCase(0, 20)]
    [TestCase(1, 0)]
    [TestCase(1, 101)]
    public void ShouldRejectOutOfRangePaging(int pageNumber, int pageSize)
    {
        _validator.Validate(new GetAuditLogQuery { PageNumber = pageNumber, PageSize = pageSize }).IsValid.ShouldBeFalse();
    }
}
