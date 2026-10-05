using CleanArchitecture.Application.Common.Behaviours;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Roles.Queries.GetRoles;
using CleanArchitecture.Application.Users.Commands.SetUserRoles;
using CleanArchitecture.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Application.UnitTests.Common.Behaviours;

public class AuthorizationBehaviourTests
{
    private Mock<IUser> _user = null!;
    private Mock<IAuthorizationService> _authorizationService = null!;

    [SetUp]
    public void Setup()
    {
        _user = new Mock<IUser>();
        _user.Setup(u => u.Id).Returns("user-1");
        _authorizationService = new Mock<IAuthorizationService>();
    }

    private Task<Unit> Run<TRequest>(TRequest request, params string[] permissions) where TRequest : notnull
    {
        _user.Setup(u => u.Permissions).Returns([.. permissions]);
        var behaviour = new AuthorizationBehaviour<TRequest, Unit>(_user.Object, _authorizationService.Object);

        return behaviour.Handle(request, _ => Task.FromResult(Unit.Value), CancellationToken.None);
    }

    [Test]
    public async Task ShouldAllowWhenUserHoldsThePermission()
    {
        await Should.NotThrowAsync(() => Run(new GetRolesQuery(), Permissions.Roles.Read));
    }

    [Test]
    public async Task ShouldForbidWhenUserLacksThePermission()
    {
        await Should.ThrowAsync<ForbiddenAccessException>(() => Run(new GetRolesQuery(), Permissions.Users.Read));
    }

    [Test]
    public async Task ShouldForbidAssigningRolesWithUsersWriteAlone()
    {
        await Should.ThrowAsync<ForbiddenAccessException>(() => Run(new SetUserRolesCommand(), Permissions.Users.Write));
    }

    [Test]
    public async Task ShouldAllowAssigningRolesWithUsersWriteAndRolesWrite()
    {
        await Should.NotThrowAsync(() => Run(new SetUserRolesCommand(), Permissions.Users.Write, Permissions.Roles.Write));
    }

    [Test]
    public async Task ShouldRejectAnonymousCallers()
    {
        _user.Setup(u => u.Id).Returns((string?)null);

        await Should.ThrowAsync<UnauthorizedAccessException>(() => Run(new GetRolesQuery()));
    }
}
