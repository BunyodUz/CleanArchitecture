// Mirrors src/Domain/Constants/Permissions.cs. The API is what actually enforces these — the
// frontend only uses them to hide navigation and actions the current user can't perform.
export const PERMISSIONS = {
  todoLists: { read: "todolists.read", write: "todolists.write" },
  todoItems: { read: "todoitems.read", write: "todoitems.write" },
  users: { read: "users.read", write: "users.write" },
  roles: { read: "roles.read", write: "roles.write" },
} as const;

export const ADMINISTRATOR_ROLE = "Administrator";
