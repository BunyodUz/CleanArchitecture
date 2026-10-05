namespace CleanArchitecture.Infrastructure.Identity;

public class KeycloakAdminOptions
{
    public const string SectionName = "Keycloak:AdminApi";

    public string BaseUrl { get; set; } = string.Empty;

    public string Realm { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>The client whose client roles are this app's permissions (the web client).</summary>
    public string PermissionsClientId { get; set; } = string.Empty;
}
