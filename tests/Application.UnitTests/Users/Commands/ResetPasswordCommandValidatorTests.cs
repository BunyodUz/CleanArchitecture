using CleanArchitecture.Application.Users.Commands.ResetPassword;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Application.UnitTests.Users.Commands;

public class ResetPasswordCommandValidatorTests
{
    private ResetPasswordCommandValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new ResetPasswordCommandValidator();
    }

    [Test]
    public async Task ShouldNotHaveErrorWhenCommandIsValid()
    {
        var result = await _validator.ValidateAsync(new ResetPasswordCommand { Id = "1", Password = "longenough" });

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldHaveErrorWhenPasswordIsTooShort()
    {
        var result = await _validator.ValidateAsync(new ResetPasswordCommand { Id = "1", Password = "short" });

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(ResetPasswordCommand.Password));
    }

    [Test]
    public async Task ShouldHaveErrorWhenIdIsEmpty()
    {
        var result = await _validator.ValidateAsync(new ResetPasswordCommand { Id = "", Password = "longenough" });

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(ResetPasswordCommand.Id));
    }
}
