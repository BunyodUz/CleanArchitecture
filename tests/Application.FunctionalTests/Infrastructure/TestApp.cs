using CleanArchitecture.Domain.Constants;
using CleanArchitecture.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Application.FunctionalTests.Infrastructure;

public static class TestApp
{
    private static string? _userId;
    private static List<string>? _roles;
    private static List<string>? _permissions;

    public static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        return await mediator.Send(request);
    }

    public static async Task SendAsync(IBaseRequest request)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        await mediator.Send(request);
    }

    public static string? GetUserId() => _userId;

    public static List<string>? GetRoles() => _roles;

    public static List<string>? GetPermissions() => _permissions;

    // Any authenticated user can manage their own todos today — only admin-only actions
    // need the Administrator role, so the default test user gets the baseline permissions.
    public static Task<string> RunAsDefaultUserAsync() => RunAsUserAsync(
        [],
        [Permissions.TodoLists.Read, Permissions.TodoLists.Write, Permissions.TodoItems.Read, Permissions.TodoItems.Write]);

    public static Task<string> RunAsAdministratorAsync() => RunAsUserAsync(
        [Roles.Administrator],
        [Permissions.TodoLists.Read, Permissions.TodoLists.Write, Permissions.TodoItems.Read, Permissions.TodoItems.Write]);

    // No real identity provider is involved in functional tests — IUser is mocked directly
    // by WebApiFactory, so "running as" a user is just picking the id/roles/permissions it returns.
    public static Task<string> RunAsUserAsync(string[] roles, string[] permissions)
    {
        _userId = Guid.NewGuid().ToString();
        _roles = [.. roles];
        _permissions = [.. permissions];

        return Task.FromResult(_userId);
    }

    public static async Task ResetState()
    {
        if (FunctionalTestSetup.DbResetter is not null)
        {
            await FunctionalTestSetup.DbResetter.ResetAsync();
        }

        _userId = null;
        _roles = null;
        _permissions = null;
    }

    public static async Task<TEntity?> FindAsync<TEntity>(params object[] keyValues)
        where TEntity : class
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.FindAsync<TEntity>(keyValues);
    }

    public static async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Add(entity);

        await context.SaveChangesAsync();
    }

    public static async Task<int> CountAsync<TEntity>() where TEntity : class
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Set<TEntity>().CountAsync();
    }
}
