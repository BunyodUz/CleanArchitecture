using Ardalis.GuardClauses;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Users.Queries.GetUser;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Application.UnitTests.Users.Queries;

public class GetUserQueryHandlerTests
{
    private Mock<IIdentityAdminService> _identityAdminService = null!;
    private GetUserQueryHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _identityAdminService = new Mock<IIdentityAdminService>();
        _handler = new GetUserQueryHandler(_identityAdminService.Object);
    }

    [Test]
    public async Task ShouldReturnUserWhenFound()
    {
        var user = new IdentityUserDto { Id = "1", Username = "administrator" };

        _identityAdminService
            .Setup(s => s.GetUserAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.Handle(new GetUserQuery("1"), CancellationToken.None);

        result.ShouldBe(user);
    }

    [Test]
    public async Task ShouldThrowNotFoundWhenUserDoesNotExist()
    {
        _identityAdminService
            .Setup(s => s.GetUserAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityUserDto?)null);

        await Should.ThrowAsync<NotFoundException>(() => _handler.Handle(new GetUserQuery("missing"), CancellationToken.None));
    }
}
