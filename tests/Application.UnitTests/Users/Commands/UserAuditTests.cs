using Ardalis.GuardClauses;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Users.Commands.DeleteUser;
using CleanArchitecture.Application.Users.Commands.ResetPassword;
using CleanArchitecture.Application.Users.Commands.SetUserRoles;
using CleanArchitecture.Application.Users.Commands.UpdateUser;
using CleanArchitecture.Domain.Constants;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Application.UnitTests.Users.Commands;

/// <summary>Every user change is recorded against the acting administrator, with a readable summary.</summary>
public class UserAuditTests
{
    private static readonly IdentityUserDto Existing = new()
    {
        Id = "u1",
        Username = "dilnoza",
        Email = "dilnoza@example.com",
        Enabled = true,
        Roles = ["Member"],
    };

    private Mock<IIdentityAdminService> _identityAdminService = null!;
    private Mock<IAuditLog> _auditLog = null!;

    [SetUp]
    public void Setup()
    {
        _identityAdminService = new Mock<IIdentityAdminService>();
        _identityAdminService.Setup(s => s.GetUserAsync("u1", It.IsAny<CancellationToken>())).ReturnsAsync(Existing);
        _auditLog = new Mock<IAuditLog>();
    }

    [Test]
    public async Task UpdateShouldRecordWhatChanged()
    {
        var handler = new UpdateUserCommandHandler(_identityAdminService.Object, _auditLog.Object);

        await handler.Handle(
            new UpdateUserCommand { Id = "u1", Username = "dilnoza", Email = "dilnoza@example.com", Enabled = false },
            CancellationToken.None);

        _auditLog.Verify(a => a.RecordAsync(
            AuditActions.UserUpdated, AuditTargets.User, "u1", "dilnoza", "status: enabled → disabled", It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task UpdateShouldThrowForUnknownUserWithoutRecording()
    {
        var handler = new UpdateUserCommandHandler(_identityAdminService.Object, _auditLog.Object);

        await Should.ThrowAsync<NotFoundException>(() =>
            handler.Handle(new UpdateUserCommand { Id = "missing", Username = "x" }, CancellationToken.None));

        _auditLog.VerifyNoOtherCalls();
    }

    [Test]
    public async Task DeleteShouldRecordTheUsername()
    {
        var handler = new DeleteUserCommandHandler(_identityAdminService.Object, _auditLog.Object);

        await handler.Handle(new DeleteUserCommand("u1"), CancellationToken.None);

        _auditLog.Verify(a => a.RecordAsync(
            AuditActions.UserDeleted, AuditTargets.User, "u1", "dilnoza", null, It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task SetRolesShouldRecordTheDifference()
    {
        var handler = new SetUserRolesCommandHandler(_identityAdminService.Object, _auditLog.Object);

        await handler.Handle(new SetUserRolesCommand { Id = "u1", Roles = ["Auditor"] }, CancellationToken.None);

        _auditLog.Verify(a => a.RecordAsync(
            AuditActions.UserRolesChanged, AuditTargets.User, "u1", "dilnoza", "roles: +Auditor, −Member", It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task SetRolesShouldNotRecordWhenNothingChanged()
    {
        var handler = new SetUserRolesCommandHandler(_identityAdminService.Object, _auditLog.Object);

        await handler.Handle(new SetUserRolesCommand { Id = "u1", Roles = ["Member"] }, CancellationToken.None);

        _identityAdminService.Verify(s => s.SetUserRolesAsync("u1", It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()));
        _auditLog.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ResetPasswordShouldNeverRecordThePassword()
    {
        var handler = new ResetPasswordCommandHandler(_identityAdminService.Object, _auditLog.Object);

        await handler.Handle(new ResetPasswordCommand { Id = "u1", Password = "S3cret!pass", Temporary = true }, CancellationToken.None);

        _auditLog.Verify(a => a.RecordAsync(
            AuditActions.UserPasswordReset,
            AuditTargets.User,
            "u1",
            "dilnoza",
            It.Is<string?>(d => d == null || !d.Contains("S3cret!pass")),
            It.IsAny<CancellationToken>()));
    }
}
