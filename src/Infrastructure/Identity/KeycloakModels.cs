using System.Text.Json.Serialization;

namespace CleanArchitecture.Infrastructure.Identity;

/// <summary>Keycloak Admin REST API's UserRepresentation — only the fields this app uses.</summary>
internal sealed class UserRepresentation
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("emailVerified")]
    public bool? EmailVerified { get; set; }

    [JsonPropertyName("credentials")]
    public List<CredentialRepresentation>? Credentials { get; set; }
}

/// <summary>Keycloak Admin REST API's CredentialRepresentation, used to set/reset a password.</summary>
internal sealed class CredentialRepresentation
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "password";

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("temporary")]
    public bool Temporary { get; set; }
}

/// <summary>Keycloak Admin REST API's RoleRepresentation — role-mapping endpoints need both Id and Name.</summary>
internal sealed class RoleRepresentation
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
