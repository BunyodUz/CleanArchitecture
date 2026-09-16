using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Users.Commands.SetUserRoles;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Application.UnitTests.Users.Commands;

public class SetUserRolesCommandValidatorTests
{
    private Mock<IIdentityAdminService> _identityAdminService = null!;
    private SetUserRolesCommandValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _identityAdminService = new Mock<IIdentityAdminService>();
        _identityAdminService
            .Setup(s => s.GetRolesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new IdentityRoleDto("role-1", "Administrator")]);

        _validator = new SetUserRolesCommandValidator(_identityAdminService.Object);
    }

    [Test]
    public async Task ShouldNotHaveErrorWhenRolesAreKnown()
    {
        var result = await _validator.ValidateAsync(new SetUserRolesCommand { Id = "1", Roles = ["Administrator"] });

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldNotHaveErrorWhenRolesAreEmpty()
    {
        var result = await _validator.ValidateAsync(new SetUserRolesCommand { Id = "1", Roles = [] });

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldHaveErrorWhenARoleDoesNotExist()
    {
        var result = await _validator.ValidateAsync(new SetUserRolesCommand { Id = "1", Roles = ["NotARole"] });

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(SetUserRolesCommand.Roles) && e.ErrorCode == "Unknown");
    }
}
