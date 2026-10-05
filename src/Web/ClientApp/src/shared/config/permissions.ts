// Mirrors src/Domain/Constants/Permissions.cs. The API is what actually enforces these — the
// frontend only uses them to hide navigation and actions the current user can't perform.
export const PERMISSIONS = {
  todoLists: { read: "todolists.read", write: "todolists.write" },
  todoItems: { read: "todoitems.read", write: "todoitems.write" },
  users: { read: "users.read", write: "users.write" },
  roles: { read: "roles.read", write: "roles.write" },
} as const;

/** Plain-language meaning of each permission, as shown to end users (mirrors the realm's descriptions). */
export const PERMISSION_DESCRIPTIONS: Record<string, string> = {
  "todolists.read": "See todo lists",
  "todolists.write": "Create, rename and delete todo lists",
  "todoitems.read": "See tasks",
  "todoitems.write": "Add, edit and complete tasks",
  "users.read": "See the user directory",
  "users.write": "Create and edit users, reset passwords",
  "roles.read": "See roles and what they grant",
  "roles.write": "Create and edit roles, assign roles to users",
};

export const ADMINISTRATOR_ROLE = "Administrator";
