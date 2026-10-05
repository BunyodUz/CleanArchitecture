using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Roles.Commands.CreateRole;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Application.UnitTests.Roles.Commands;

public class CreateRoleCommandValidatorTests
{
    private Mock<IIdentityAdminService> _identityAdminService = null!;
    private CreateRoleCommandValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _identityAdminService = new Mock<IIdentityAdminService>();
        _identityAdminService
            .Setup(s => s.GetRolesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new IdentityRoleDto { Id = "1", Name = "Administrator" }]);
        _identityAdminService
            .Setup(s => s.GetPermissionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new IdentityPermissionDto("todolists.read", null)]);

        _validator = new CreateRoleCommandValidator(_identityAdminService.Object);
    }

    [Test]
    public async Task ShouldNotHaveErrorWhenCommandIsValid()
    {
        var result = await _validator.ValidateAsync(new CreateRoleCommand { Name = "Auditor", Permissions = ["todolists.read"] });

        result.IsValid.ShouldBeTrue();
    }

    [TestCase("")]
    [TestCase("1st-role")]
    [TestCase("bad/name")]
    public async Task ShouldHaveErrorWhenNameIsInvalid(string name)
    {
        var result = await _validator.ValidateAsync(new CreateRoleCommand { Name = name });

        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateRoleCommand.Name));
    }

    [Test]
    public async Task ShouldReportNullNameAsAValidationErrorRatherThanThrowing()
    {
        var result = await _validator.ValidateAsync(new CreateRoleCommand { Name = null! });

        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateRoleCommand.Name));
    }

    [Test]
    public async Task ShouldHaveErrorWhenNameIsReserved()
    {
        var result = await _validator.ValidateAsync(new CreateRoleCommand { Name = "default-roles-anything" });

        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateRoleCommand.Name));
    }

    [Test]
    public async Task ShouldHaveErrorWhenNameIsTakenIgnoringCase()
    {
        var result = await _validator.ValidateAsync(new CreateRoleCommand { Name = "administrator" });

        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateRoleCommand.Name) && e.ErrorCode == "Unique");
    }

    [Test]
    public async Task ShouldHaveErrorWhenAPermissionDoesNotExist()
    {
        var result = await _validator.ValidateAsync(new CreateRoleCommand { Name = "Auditor", Permissions = ["nope.read"] });

        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateRoleCommand.Permissions) && e.ErrorCode == "Unknown");
    }
}
