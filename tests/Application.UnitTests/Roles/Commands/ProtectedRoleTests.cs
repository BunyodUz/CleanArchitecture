using Ardalis.GuardClauses;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Roles.Commands.DeleteRole;
using CleanArchitecture.Application.Roles.Commands.UpdateRole;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Application.UnitTests.Roles.Commands;

/// <summary>Whether a role is protected comes from Keycloak (the role's attribute), never from its name.</summary>
public class ProtectedRoleTests
{
    private Mock<IIdentityAdminService> _identityAdminService = null!;

    [SetUp]
    public void Setup()
    {
        _identityAdminService = new Mock<IIdentityAdminService>();
        _identityAdminService
            .Setup(s => s.GetPermissionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new IdentityPermissionDto("todolists.read", null)]);
        _identityAdminService
            .Setup(s => s.GetRoleAsync("Owner", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IdentityRoleDto { Id = "1", Name = "Owner", IsProtected = true });
        _identityAdminService
            .Setup(s => s.GetRoleAsync("Administrator", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IdentityRoleDto { Id = "2", Name = "Administrator" });
    }

    [Test]
    public async Task UpdateShouldRejectAProtectedRole()
    {
        var validator = new UpdateRoleCommandValidator(_identityAdminService.Object);

        var result = await validator.ValidateAsync(new UpdateRoleCommand { Name = "Owner" });

        result.Errors.ShouldContain(e => e.PropertyName == nameof(UpdateRoleCommand.Name) && e.ErrorCode == "Protected");
    }

    [Test]
    public async Task DeleteShouldRejectAProtectedRole()
    {
        var validator = new DeleteRoleCommandValidator(_identityAdminService.Object);

        var result = await validator.ValidateAsync(new DeleteRoleCommand("Owner"));

        result.Errors.ShouldContain(e => e.PropertyName == nameof(DeleteRoleCommand.Name) && e.ErrorCode == "Protected");
    }

    [Test]
    public async Task ARoleNamedAdministratorIsNotSpecial()
    {
        var update = await new UpdateRoleCommandValidator(_identityAdminService.Object)
            .ValidateAsync(new UpdateRoleCommand { Name = "Administrator", Permissions = ["todolists.read"] });
        var delete = await new DeleteRoleCommandValidator(_identityAdminService.Object)
            .ValidateAsync(new DeleteRoleCommand("Administrator"));

        update.IsValid.ShouldBeTrue();
        delete.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task UnknownRolesPassValidationAndAreLeftToTheHandler()
    {
        var result = await new DeleteRoleCommandValidator(_identityAdminService.Object).ValidateAsync(new DeleteRoleCommand("Ghost"));

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task UpdateShouldThrowNotFoundForAnUnknownRole()
    {
        _identityAdminService
            .Setup(s => s.GetRoleAsync("Ghost", It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityRoleDto?)null);

        var handler = new UpdateRoleCommandHandler(_identityAdminService.Object, Mock.Of<IAuditLog>());

        await Should.ThrowAsync<NotFoundException>(() => handler.Handle(new UpdateRoleCommand { Name = "Ghost" }, CancellationToken.None));
        _identityAdminService.Verify(
            s => s.UpdateRoleAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
