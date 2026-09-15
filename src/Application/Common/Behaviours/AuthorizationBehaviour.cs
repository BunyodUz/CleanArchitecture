using System.Reflection;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Security;
using Microsoft.AspNetCore.Authorization;

namespace CleanArchitecture.Application.Common.Behaviours;

public class AuthorizationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUser _user;
    private readonly IAuthorizationService _authorizationService;

    public AuthorizationBehaviour(
        IUser user,
        IAuthorizationService authorizationService)
    {
        _user = user;
        _authorizationService = authorizationService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>();

        if (authorizeAttributes.Any())
        {
            // Must be authenticated user
            if (_user.Id == null)
            {
                throw new UnauthorizedAccessException();
            }

            // Role-based authorization
            var authorizeAttributesWithRoles = authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(a.Roles));

            if (authorizeAttributesWithRoles.Any())
            {
                var authorized = false;

                foreach (var roles in authorizeAttributesWithRoles.Select(a => a.Roles.Split(',')))
                {
                    foreach (var role in roles)
                    {
                        var isInRole = _user.Roles?.Any(x => role == x) ?? false;
                        if (isInRole)
                        {
                            authorized = true;
                            break;
                        }
                    }
                }

                // Must be a member of at least one role in roles
                if (!authorized)
                {
                    throw new ForbiddenAccessException();
                }
            }

            // Permission-based authorization
            var authorizeAttributesWithPermissions = authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(a.Permissions));

            if (authorizeAttributesWithPermissions.Any())
            {
                foreach (var permissions in authorizeAttributesWithPermissions.Select(a => a.Permissions.Split(',')))
                {
                    // Must hold every permission listed on the attribute
                    var authorized = permissions.All(permission => _user.Permissions?.Contains(permission) ?? false);

                    if (!authorized)
                    {
                        throw new ForbiddenAccessException();
                    }
                }
            }

            // Policy-based authorization — for resource/ownership checks a static claim can't express
            var authorizeAttributesWithPolicies = authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(a.Policy));
            if (authorizeAttributesWithPolicies.Any())
            {
                foreach (var policy in authorizeAttributesWithPolicies.Select(a => a.Policy))
                {
                    var result = await _authorizationService.AuthorizeAsync(_user.Principal!, policy);

                    if (!result.Succeeded)
                    {
                        throw new ForbiddenAccessException();
                    }
                }
            }
        }

        // User is authorized / authorization not required
        return await next();
    }
}
