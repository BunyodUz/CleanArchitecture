namespace CleanArchitecture.Application.Common.Models;

/// <summary>A user directly assigned a role, without their other roles.</summary>
public record IdentityRoleMemberDto(string Id, string? Username, bool Enabled);
