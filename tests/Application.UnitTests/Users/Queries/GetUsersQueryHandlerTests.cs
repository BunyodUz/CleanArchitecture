using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Users.Queries.GetUsers;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Application.UnitTests.Users.Queries;

public class GetUsersQueryHandlerTests
{
    private Mock<IIdentityAdminService> _identityAdminService = null!;
    private GetUsersQueryHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _identityAdminService = new Mock<IIdentityAdminService>();
        _handler = new GetUsersQueryHandler(_identityAdminService.Object);
    }

    [Test]
    public async Task ShouldPassSearchThroughAndReturnResult()
    {
        var users = new List<IdentityUserDto>
        {
            new() { Id = "1", Username = "administrator" },
        };

        _identityAdminService
            .Setup(s => s.GetUsersAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var result = await _handler.Handle(new GetUsersQuery("admin"), CancellationToken.None);

        result.ShouldBe(users);
    }
}
