using Ardalis.GuardClauses;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Roles.Commands.DeleteRole;
using CleanArchitecture.Application.Roles.Commands.UpdateRole;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Application.UnitTests.Roles.Commands;

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
    }

    [TestCase("Administrator")]
    [TestCase("administrator")]
    public async Task UpdateShouldRejectTheAdministratorRole(string name)
    {
        var validator = new UpdateRoleCommandValidator(_identityAdminService.Object);

        var result = await validator.ValidateAsync(new UpdateRoleCommand { Name = name });

        result.Errors.ShouldContain(e => e.PropertyName == nameof(UpdateRoleCommand.Name) && e.ErrorCode == "Protected");
    }

    [TestCase("Administrator")]
    [TestCase("ADMINISTRATOR")]
    public async Task DeleteShouldRejectTheAdministratorRole(string name)
    {
        var validator = new DeleteRoleCommandValidator();

        var result = await validator.ValidateAsync(new DeleteRoleCommand(name));

        result.Errors.ShouldContain(e => e.PropertyName == nameof(DeleteRoleCommand.Name) && e.ErrorCode == "Protected");
    }

    [Test]
    public async Task UpdateShouldAllowOtherRoles()
    {
        var validator = new UpdateRoleCommandValidator(_identityAdminService.Object);

        var result = await validator.ValidateAsync(new UpdateRoleCommand { Name = "Member", Permissions = ["todolists.read"] });

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task UpdateShouldThrowNotFoundForAnUnknownRole()
    {
        _identityAdminService
            .Setup(s => s.GetRoleAsync("Ghost", It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityRoleDto?)null);

        var handler = new UpdateRoleCommandHandler(_identityAdminService.Object);

        await Should.ThrowAsync<NotFoundException>(() => handler.Handle(new UpdateRoleCommand { Name = "Ghost" }, CancellationToken.None));
        _identityAdminService.Verify(
            s => s.UpdateRoleAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
