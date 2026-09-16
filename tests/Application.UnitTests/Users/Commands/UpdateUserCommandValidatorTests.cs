using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Users.Commands.UpdateUser;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Application.UnitTests.Users.Commands;

public class UpdateUserCommandValidatorTests
{
    private Mock<IIdentityAdminService> _identityAdminService = null!;
    private UpdateUserCommandValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _identityAdminService = new Mock<IIdentityAdminService>();
        _identityAdminService
            .Setup(s => s.GetUsersAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _validator = new UpdateUserCommandValidator(_identityAdminService.Object);
    }

    [Test]
    public async Task ShouldNotHaveErrorWhenCommandIsValid()
    {
        var result = await _validator.ValidateAsync(new UpdateUserCommand { Id = "1", Username = "someuser" });

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldNotHaveErrorWhenUsernameBelongsToTheSameUser()
    {
        _identityAdminService
            .Setup(s => s.GetUsersAsync("someuser", It.IsAny<CancellationToken>()))
            .ReturnsAsync([new IdentityUserDto { Id = "1", Username = "someuser" }]);

        var result = await _validator.ValidateAsync(new UpdateUserCommand { Id = "1", Username = "someuser" });

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldHaveErrorWhenUsernameBelongsToAnotherUser()
    {
        _identityAdminService
            .Setup(s => s.GetUsersAsync("taken", It.IsAny<CancellationToken>()))
            .ReturnsAsync([new IdentityUserDto { Id = "2", Username = "taken" }]);

        var result = await _validator.ValidateAsync(new UpdateUserCommand { Id = "1", Username = "taken" });

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(UpdateUserCommand.Username) && e.ErrorCode == "Unique");
    }
}
