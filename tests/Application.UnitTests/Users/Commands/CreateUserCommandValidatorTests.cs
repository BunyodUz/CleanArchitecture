using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Users.Commands.CreateUser;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Application.UnitTests.Users.Commands;

public class CreateUserCommandValidatorTests
{
    private Mock<IIdentityAdminService> _identityAdminService = null!;
    private CreateUserCommandValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _identityAdminService = new Mock<IIdentityAdminService>();
        _identityAdminService
            .Setup(s => s.GetUsersAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _validator = new CreateUserCommandValidator(_identityAdminService.Object);
    }

    [Test]
    public async Task ShouldNotHaveErrorWhenCommandIsValid()
    {
        var result = await _validator.ValidateAsync(new CreateUserCommand { Username = "newuser" });

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldHaveErrorWhenUsernameIsEmpty()
    {
        var result = await _validator.ValidateAsync(new CreateUserCommand { Username = "" });

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateUserCommand.Username));
    }

    [Test]
    public async Task ShouldHaveErrorWhenUsernameIsTaken()
    {
        _identityAdminService
            .Setup(s => s.GetUsersAsync("existing", It.IsAny<CancellationToken>()))
            .ReturnsAsync([new IdentityUserDto { Id = "1", Username = "existing" }]);

        var result = await _validator.ValidateAsync(new CreateUserCommand { Username = "existing" });

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateUserCommand.Username) && e.ErrorCode == "Unique");
    }

    [Test]
    public async Task ShouldHaveErrorWhenEmailIsInvalid()
    {
        var result = await _validator.ValidateAsync(new CreateUserCommand { Username = "newuser", Email = "not-an-email" });

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateUserCommand.Email));
    }

    [Test]
    public async Task ShouldHaveErrorWhenPasswordIsTooShort()
    {
        var result = await _validator.ValidateAsync(new CreateUserCommand { Username = "newuser", Password = "short" });

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateUserCommand.Password));
    }
}
