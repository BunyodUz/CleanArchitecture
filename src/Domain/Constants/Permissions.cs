namespace CleanArchitecture.Domain.Constants;

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
}
