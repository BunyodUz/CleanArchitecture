namespace CleanArchitecture.Domain.Constants;

/// <summary>
/// Fine-grained permissions, modelled as client roles of the web client in Keycloak. Users never
/// hold these directly — they're bundled into realm roles (composite roles), and users are
/// assigned roles. Every name here must also exist as a client role in the realm
/// (deploy/keycloak/realm-export.json).
/// </summary>
public abstract class Permissions
{
    public abstract class TodoLists
    {
        public const string Read = "todolists.read";
        public const string Write = "todolists.write";
    }

    public abstract class TodoItems
    {
        public const string Read = "todoitems.read";
        public const string Write = "todoitems.write";
    }

    public abstract class Users
    {
        public const string Read = "users.read";
        public const string Write = "users.write";
    }

    public abstract class Roles
    {
        public const string Read = "roles.read";
        public const string Write = "roles.write";
    }

    public abstract class Audit
    {
        public const string Read = "audit.read";
    }
}
