using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Users.Commands.DeleteUser;
using CleanArchitecture.Application.Users.Commands.SetUserRoles;
using CleanArchitecture.Application.Users.Commands.UpdateUser;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Application.UnitTests.Users.Commands;

/// <summary>No change to a user may leave the realm without an enabled user holding a protected role.</summary>
public class LastAdministratorGuardTests
{
    private static readonly IdentityUserDto Admin = new() { Id = "admin", Username = "admin", Enabled = true, Roles = ["Owner", "Member"] };

    private Mock<IIdentityAdminService> _identityAdminService = null!;
    private List<IdentityRoleMemberDto> _ownerMembers = null!;

    [SetUp]
    public void Setup()
    {
        _ownerMembers = [new IdentityRoleMemberDto("admin", "admin", true)];

        _identityAdminService = new Mock<IIdentityAdminService>();
        _identityAdminService.Setup(s => s.GetUserAsync("admin", It.IsAny<CancellationToken>())).ReturnsAsync(Admin);
        _identityAdminService
            .Setup(s => s.GetRolesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new IdentityRoleDto { Id = "1", Name = "Owner", IsProtected = true },
                new IdentityRoleDto { Id = "2", Name = "Member" },
            ]);
        _identityAdminService
            .Setup(s => s.GetRoleMembersAsync("Owner", It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => _ownerMembers);
    }

    private SetUserRolesCommandHandler SetRolesHandler() => new(_identityAdminService.Object, Mock.Of<IAuditLog>());

    [Test]
    public async Task ShouldRefuseToRemoveTheLastAdministratorsProtectedRole()
    {
        var ex = await Should.ThrowAsync<ValidationException>(() =>
            SetRolesHandler().Handle(new SetUserRolesCommand { Id = "admin", Roles = ["Member"] }, CancellationToken.None));

        ex.Errors.ShouldContainKey(nameof(SetUserRolesCommand.Roles));
        _identityAdminService.Verify(
            s => s.SetUserRolesAsync(It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ShouldAllowRemovingItWhenAnotherEnabledUserHoldsIt()
    {
        _ownerMembers.Add(new IdentityRoleMemberDto("other", "other", true));

        await SetRolesHandler().Handle(new SetUserRolesCommand { Id = "admin", Roles = ["Member"] }, CancellationToken.None);

        _identityAdminService.Verify(s => s.SetUserRolesAsync("admin", It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task ADisabledHolderDoesNotCount()
    {
        _ownerMembers.Add(new IdentityRoleMemberDto("other", "other", false));

        await Should.ThrowAsync<ValidationException>(() =>
            SetRolesHandler().Handle(new SetUserRolesCommand { Id = "admin", Roles = [] }, CancellationToken.None));
    }

    [Test]
    public async Task ShouldAllowRemovingUnprotectedRoles()
    {
        await SetRolesHandler().Handle(new SetUserRolesCommand { Id = "admin", Roles = ["Owner"] }, CancellationToken.None);

        _identityAdminService.Verify(s => s.SetUserRolesAsync("admin", It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()));
        _identityAdminService.Verify(s => s.GetRoleMembersAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ShouldRefuseToDisableTheLastAdministrator()
    {
        var handler = new UpdateUserCommandHandler(_identityAdminService.Object, Mock.Of<IAuditLog>());

        var ex = await Should.ThrowAsync<ValidationException>(() =>
            handler.Handle(new UpdateUserCommand { Id = "admin", Username = "admin", Enabled = false }, CancellationToken.None));

        ex.Errors.ShouldContainKey(nameof(UpdateUserCommand.Enabled));
    }

    [Test]
    public async Task ShouldRefuseToDeleteTheLastAdministrator()
    {
        var handler = new DeleteUserCommandHandler(_identityAdminService.Object, Mock.Of<IAuditLog>());

        await Should.ThrowAsync<ValidationException>(() => handler.Handle(new DeleteUserCommand("admin"), CancellationToken.None));

        _identityAdminService.Verify(s => s.DeleteUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ShouldAllowDeletingADisabledAdministrator()
    {
        _identityAdminService
            .Setup(s => s.GetUserAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Admin with { Enabled = false });
        var handler = new DeleteUserCommandHandler(_identityAdminService.Object, Mock.Of<IAuditLog>());

        await handler.Handle(new DeleteUserCommand("admin"), CancellationToken.None);

        _identityAdminService.Verify(s => s.DeleteUserAsync("admin", It.IsAny<CancellationToken>()));
    }
}
