using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Users.Commands.CreateUser;
using CleanArchitecture.Domain.Constants;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Application.UnitTests.Users.Commands;

public class CreateUserCommandHandlerTests
{
    private Mock<IIdentityAdminService> _identityAdminService = null!;
    private Mock<IUser> _user = null!;
    private Mock<IAuditLog> _auditLog = null!;
    private CreateUserCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _identityAdminService = new Mock<IIdentityAdminService>();
        _identityAdminService
            .Setup(s => s.CreateUserAsync(
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("new-id");

        _user = new Mock<IUser>();
        _auditLog = new Mock<IAuditLog>();
        _handler = new CreateUserCommandHandler(_identityAdminService.Object, _user.Object, _auditLog.Object);
    }

    [Test]
    public async Task ShouldCreateUserWithoutRolesGivenOnlyUsersWrite()
    {
        _user.Setup(u => u.Permissions).Returns([Permissions.Users.Write]);

        var id = await _handler.Handle(new CreateUserCommand { Username = "new" }, CancellationToken.None);

        id.ShouldBe("new-id");
    }

    [Test]
    public async Task ShouldRejectAssigningRolesWithoutRolesWrite()
    {
        _user.Setup(u => u.Permissions).Returns([Permissions.Users.Write]);

        await Should.ThrowAsync<ForbiddenAccessException>(() =>
            _handler.Handle(new CreateUserCommand { Username = "new", Roles = ["Administrator"] }, CancellationToken.None));

        _auditLog.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ShouldAssignRolesGivenRolesWrite()
    {
        _user.Setup(u => u.Permissions).Returns([Permissions.Users.Write, Permissions.Roles.Write]);

        var id = await _handler.Handle(new CreateUserCommand { Username = "new", Roles = ["Member"] }, CancellationToken.None);

        id.ShouldBe("new-id");
        _auditLog.Verify(a => a.RecordAsync(
            AuditActions.UserCreated, AuditTargets.User, "new-id", "new", "roles: +Member", It.IsAny<CancellationToken>()));
    }
}
