namespace CleanArchitecture.Application.Common.Security;

/// <summary>
/// Specifies the class this attribute is applied to requires authorization.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class AuthorizeAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizeAttribute"/> class. 
    /// </summary>
    public AuthorizeAttribute() { }

    /// <summary>
    /// Gets or sets a comma delimited list of roles that are allowed to access the resource.
    /// Roles are coarse-grained (e.g. "Administrator") and best suited to whole-app gates.
    /// </summary>
    public string Roles { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a comma delimited list of permissions required to access the resource
    /// (e.g. "todolists.write"). This is the primary mechanism for "can this user do X"
    /// checks — see <see cref="Domain.Constants.Permissions"/>.
    /// </summary>
    public string Permissions { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the policy name that determines access to the resource. Policies are
    /// evaluated via <see cref="Microsoft.AspNetCore.Authorization.IAuthorizationService"/> and
    /// are the escape hatch for resource-based/ownership checks that a static permission claim
    /// can't express on its own.
    /// </summary>
    public string Policy { get; set; } = string.Empty;
}
