using CleanArchitecture.Application.AuditEntries;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Application.UnitTests.AuditEntries;

public class AuditDetailsTests
{
    [Test]
    public void ChangesShouldListOnlyFieldsThatChanged()
    {
        AuditDetails.Changes(("email", "a@x", "b@x"), ("first name", "Ann", "Ann"), ("status", "enabled", "disabled"))
            .ShouldBe("email: a@x → b@x; status: enabled → disabled");
    }

    [Test]
    public void ChangesShouldTreatNullAndBlankAsTheSame()
    {
        AuditDetails.Changes(("email", null, ""), ("last name", "  ", null)).ShouldBeNull();
    }

    [Test]
    public void ChangesShouldShowMissingValuesAsNone()
    {
        AuditDetails.Changes(("email", null, "b@x")).ShouldBe("email: (none) → b@x");
    }

    [Test]
    public void SetChangesShouldListAddedThenRemovedInOrder()
    {
        AuditDetails.SetChanges("roles", ["Member", "Viewer"], ["Member", "Auditor", "Admin"])
            .ShouldBe("roles: +Admin, +Auditor, −Viewer");
    }

    [Test]
    public void SetChangesShouldReturnNullForEqualSets()
    {
        AuditDetails.SetChanges("roles", ["A", "B"], ["B", "A"]).ShouldBeNull();
    }

    [Test]
    public void JoinShouldSkipEmptyParts()
    {
        AuditDetails.Join(null, "a", "", "b").ShouldBe("a; b");
        AuditDetails.Join(null, null).ShouldBeNull();
    }
}
