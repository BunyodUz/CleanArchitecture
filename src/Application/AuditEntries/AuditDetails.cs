namespace CleanArchitecture.Application.AuditEntries;

/// <summary>Builds the human-readable <c>Details</c> text of audit entries.</summary>
public static class AuditDetails
{
    /// <summary>"email: old@x → new@x; status: enabled → disabled", or <see langword="null"/> when nothing changed.</summary>
    public static string? Changes(params (string Field, string? Before, string? After)[] fields)
    {
        var changes = fields
            .Where(f => !string.Equals(Normalize(f.Before), Normalize(f.After), StringComparison.Ordinal))
            .Select(f => $"{f.Field}: {Display(f.Before)} → {Display(f.After)}")
            .ToList();

        return changes.Count == 0 ? null : string.Join("; ", changes);
    }

    /// <summary>"roles: +Auditor, −Member", or <see langword="null"/> when the sets are equal.</summary>
    public static string? SetChanges(string label, IEnumerable<string> before, IEnumerable<string> after)
    {
        var beforeSet = before.ToHashSet(StringComparer.Ordinal);
        var afterSet = after.ToHashSet(StringComparer.Ordinal);

        var changes = afterSet.Except(beforeSet).Order(StringComparer.Ordinal).Select(x => $"+{x}")
            .Concat(beforeSet.Except(afterSet).Order(StringComparer.Ordinal).Select(x => $"−{x}"))
            .ToList();

        return changes.Count == 0 ? null : $"{label}: {string.Join(", ", changes)}";
    }

    /// <summary>Joins non-empty parts with "; ".</summary>
    public static string? Join(params string?[] parts)
    {
        var present = parts.Where(p => !string.IsNullOrEmpty(p)).ToList();
        return present.Count == 0 ? null : string.Join("; ", present);
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

    private static string Display(string? value) => Normalize(value) ?? "(none)";
}
