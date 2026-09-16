import { createEffect, createEvent, createStore, sample } from "effector";
import { todoItemsClient, todoListsClient } from "@/shared/api/client";
import {
  CreateTodoItemCommand,
  CreateTodoListCommand,
  type TodoItemDto,
  type TodoListDto,
  UpdateTodoItemCommand,
  UpdateTodoItemDetailCommand,
  UpdateTodoListCommand,
} from "@/web-api-client";

// Plain domain shapes, decoupled from the generated DTO classes so the store never has to carry
// their prototype methods (toJSON, init, …) around — only the data those methods would read.
export interface Todo {
  id: number;
  listId: number;
  title: string;
  done: boolean;
  priority: number;
  note?: string;
}

export interface TodoList {
  id: number;
  title: string;
  colour: string;
  items: Todo[];
}

export interface PriorityLevel {
  id: number;
  title: string;
}

export interface Colour {
  code: string;
  name: string;
}

const toTodo = (dto: TodoItemDto): Todo => ({
  id: dto.id!,
  listId: dto.listId!,
  title: dto.title ?? "",
  done: dto.done ?? false,
  priority: dto.priority ?? 0,
  note: dto.note,
});

const toTodoList = (dto: TodoListDto): TodoList => ({
  id: dto.id!,
  title: dto.title ?? "",
  colour: dto.colour ?? "",
  items: (dto.items ?? []).map(toTodo),
});

export const fetchTodosFx = createEffect(() => todoListsClient.getTodoLists());

export const selectList = createEvent<number>();

export const $lists = createStore<TodoList[]>([]).on(fetchTodosFx.doneData, (_, vm) =>
  (vm.lists ?? []).map(toTodoList),
);

export const $priorityLevels = createStore<PriorityLevel[]>([]).on(fetchTodosFx.doneData, (_, vm) =>
  (vm.priorityLevels ?? []).map((l) => ({ id: l.id!, title: l.title ?? "" })),
);

export const $colours = createStore<Colour[]>([]).on(fetchTodosFx.doneData, (_, vm) =>
  (vm.colours ?? []).map((c) => ({ code: c.code ?? "", name: c.name ?? "" })),
);

export const $selectedListId = createStore<number | null>(null).on(selectList, (_, id) => id);

sample({
  clock: fetchTodosFx.doneData,
  filter: (vm) => (vm.lists?.length ?? 0) > 0,
  fn: (vm) => vm.lists![0].id!,
  target: $selectedListId,
});

// ── Lists ──────────────────────────────────────────────────────────────────

export const createListFx = createEffect(async (params: { title: string; colour: string }) => {
  const id = await todoListsClient.createTodoList(
    new CreateTodoListCommand({ title: params.title, colour: params.colour }),
  );
  const newList: TodoList = { id, title: params.title, colour: params.colour, items: [] };
  return newList;
});

export const updateListFx = createEffect(async (params: { id: number; title: string; colour: string }) => {
  await todoListsClient.updateTodoList(
    params.id,
    new UpdateTodoListCommand({ id: params.id, title: params.title, colour: params.colour }),
  );
  return params;
});

export const deleteListFx = createEffect(async (id: number) => {
  await todoListsClient.deleteTodoList(id);
  return id;
});

$lists.on(createListFx.doneData, (lists, newList) => [...lists, newList]);
$lists.on(updateListFx.doneData, (lists, { id, title, colour }) =>
  lists.map((l) => (l.id === id ? { ...l, title, colour } : l)),
);
$lists.on(deleteListFx.doneData, (lists, id) => lists.filter((l) => l.id !== id));

sample({
  clock: createListFx.doneData,
  fn: (newList) => newList.id,
  target: selectList,
});

sample({
  clock: deleteListFx.doneData,
  source: $lists,
  fn: (lists, deletedId) => lists.find((l) => l.id !== deletedId)?.id ?? null,
  target: $selectedListId,
});

// ── Items ──────────────────────────────────────────────────────────────────

export const createItemFx = createEffect(async (params: { listId: number; title: string }) => {
  const id = await todoItemsClient.createTodoItem(
    new CreateTodoItemCommand({ listId: params.listId, title: params.title }),
  );
  const newItem: Todo = { id, listId: params.listId, title: params.title, done: false, priority: 0 };
  return newItem;
});

export const updateItemFx = createEffect(async (item: Todo) => {
  await todoItemsClient.updateTodoItem(
    item.id,
    new UpdateTodoItemCommand({ id: item.id, title: item.title, done: item.done }),
  );
  return item;
});

export const deleteItemFx = createEffect(async (item: Todo) => {
  await todoItemsClient.deleteTodoItem(item.id);
  return item;
});

export interface UpdateItemDetailParams {
  item: Todo;
  listId: number;
  priority: number;
  note: string | undefined;
}

export const updateItemDetailFx = createEffect(async (params: UpdateItemDetailParams) => {
  await todoItemsClient.updateTodoItemDetail(
    params.item.id,
    new UpdateTodoItemDetailCommand({
      id: params.item.id,
      listId: params.listId,
      priority: params.priority,
      note: params.note,
    }),
  );
  return params;
});

$lists.on(createItemFx.doneData, (lists, newItem) =>
  lists.map((l) => (l.id === newItem.listId ? { ...l, items: [...l.items, newItem] } : l)),
);

$lists.on(updateItemFx.doneData, (lists, item) =>
  lists.map((l) =>
    l.id === item.listId ? { ...l, items: l.items.map((i) => (i.id === item.id ? item : i)) } : l,
  ),
);

$lists.on(deleteItemFx.doneData, (lists, item) =>
  lists.map((l) => (l.id === item.listId ? { ...l, items: l.items.filter((i) => i.id !== item.id) } : l)),
);

$lists.on(updateItemDetailFx.doneData, (lists, { item, listId, priority, note }) => {
  const isMoving = item.listId !== listId;
  return lists.map((l) => {
    if (l.id === item.listId && isMoving) {
      return { ...l, items: l.items.filter((i) => i.id !== item.id) };
    }
    if (l.id === listId && isMoving) {
      return { ...l, items: [...l.items, { ...item, listId, priority, note }] };
    }
    if (l.id === item.listId) {
      return { ...l, items: l.items.map((i) => (i.id === item.id ? { ...i, priority, note } : i)) };
    }
    return l;
  });
});

export const $todosLoading = fetchTodosFx.pending;
