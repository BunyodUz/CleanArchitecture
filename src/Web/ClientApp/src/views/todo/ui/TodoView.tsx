"use client";

import {
  ActionIcon,
  Box,
  Button,
  Checkbox,
  ColorSwatch,
  Group,
  Modal,
  Select,
  Stack,
  Text,
  Textarea,
  TextInput,
  Title,
  UnstyledButton,
} from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import { useUnit } from "effector-react";
import { MoreHorizontal, Plus, Settings } from "lucide-react";
import { useEffect, useRef, useState } from "react";
import {
  $colours,
  $lists,
  $priorityLevels,
  $selectedListId,
  $todosLoading,
  createItemFx,
  createListFx,
  deleteItemFx,
  deleteListFx,
  fetchTodosFx,
  selectList,
  type Todo,
  updateItemDetailFx,
  updateItemFx,
  updateListFx,
} from "@/entities/todo";
import { getFieldError } from "@/shared/lib/api-error";

export function TodoView() {
  const [lists, colours, priorityLevels, selectedListId, loading] = useUnit([
    $lists,
    $colours,
    $priorityLevels,
    $selectedListId,
    $todosLoading,
  ]);

  useEffect(() => {
    fetchTodosFx();
  }, []);

  const selectedList = lists.find((l) => l.id === selectedListId) ?? null;

  // ── New list dialog ────────────────────────────────────────────────────
  const [newListOpened, { open: openNewListRaw, close: closeNewList }] = useDisclosure(false);
  const [newListTitle, setNewListTitle] = useState("");
  const [newListColour, setNewListColour] = useState("");
  const [newListError, setNewListError] = useState("");

  const showNewListDialog = () => {
    setNewListTitle("");
    setNewListColour(colours[0]?.code ?? "");
    setNewListError("");
    openNewListRaw();
  };

  const commitNewList = async () => {
    if (!newListTitle.trim()) return;
    try {
      await createListFx({ title: newListTitle.trim(), colour: newListColour });
      closeNewList();
    } catch (e) {
      setNewListError(getFieldError(e, "Title") ?? "Failed to create list.");
    }
  };

  // ── List options dialog ────────────────────────────────────────────────
  const [listOptionsOpened, { open: openListOptions, close: closeListOptions }] = useDisclosure(false);
  const [listOptionsTitle, setListOptionsTitle] = useState("");
  const [listOptionsColour, setListOptionsColour] = useState("");

  const showListOptionsDialog = () => {
    if (!selectedList) return;
    setListOptionsTitle(selectedList.title);
    setListOptionsColour(selectedList.colour || colours[0]?.code || "");
    openListOptions();
  };

  const updateListOptions = async () => {
    if (!selectedList) return;
    await updateListFx({ id: selectedList.id, title: listOptionsTitle, colour: listOptionsColour });
    closeListOptions();
  };

  // ── Delete list dialog ─────────────────────────────────────────────────
  const [deleteListOpened, { open: openDeleteList, close: closeDeleteList }] = useDisclosure(false);

  const confirmDeleteList = () => {
    closeListOptions();
    openDeleteList();
  };

  const deleteListConfirmed = async () => {
    if (!selectedList) return;
    await deleteListFx(selectedList.id);
    closeDeleteList();
  };

  // ── Item details dialog ────────────────────────────────────────────────
  const [itemDetailsOpened, { open: openItemDetails, close: closeItemDetailsRaw }] = useDisclosure(false);
  const [selectedItem, setSelectedItem] = useState<Todo | null>(null);
  const [itemListId, setItemListId] = useState<number | null>(null);
  const [itemPriority, setItemPriority] = useState<number | null>(null);
  const [itemNote, setItemNote] = useState("");

  const showItemDetailsDialog = (item: Todo) => {
    setSelectedItem(item);
    setItemListId(item.listId);
    setItemPriority(item.priority);
    setItemNote(item.note ?? "");
    openItemDetails();
  };

  const closeItemDetailsDialog = () => {
    closeItemDetailsRaw();
    setSelectedItem(null);
  };

  const updateItemDetails = async () => {
    if (!selectedItem || itemListId === null || itemPriority === null) return;
    await updateItemDetailFx({
      item: selectedItem,
      listId: itemListId,
      priority: itemPriority,
      note: itemNote || undefined,
    });
    closeItemDetailsDialog();
  };

  const deleteSelectedItem = async () => {
    if (!selectedItem) return;
    await deleteItemFx(selectedItem);
    closeItemDetailsDialog();
  };

  // ── Inline item title editing ──────────────────────────────────────────
  const [editingItemId, setEditingItemId] = useState<number | null>(null);
  const [editValue, setEditValue] = useState("");
  const editCancelledRef = useRef(false);

  const editItem = (item: Todo) => {
    setEditValue(item.title);
    setEditingItemId(item.id);
  };

  const cancelEdit = () => {
    editCancelledRef.current = true;
    setEditingItemId(null);
  };

  const commitEdit = async () => {
    const item = selectedList?.items.find((i) => i.id === editingItemId);
    setEditingItemId(null);
    if (!item) return;
    if (!editValue.trim()) {
      await deleteItemFx(item);
      return;
    }
    await updateItemFx({ ...item, title: editValue.trim() });
  };

  const updateCheckbox = (item: Todo, done: boolean) => {
    updateItemFx({ ...item, done });
  };

  // ── New item ────────────────────────────────────────────────────────────
  const [addingItem, setAddingItem] = useState(false);
  const [newItemTitle, setNewItemTitle] = useState("");
  const newItemCancelledRef = useRef(false);

  const commitNewItem = async () => {
    setAddingItem(false);
    const title = newItemTitle.trim();
    setNewItemTitle("");
    if (!title || selectedListId === null) return;
    await createItemFx({ listId: selectedListId, title });
  };

  if (loading && lists.length === 0) {
    return <Text aria-busy="true">Loading&hellip;</Text>;
  }

  return (
    <div>
      <Title order={1}>Tasks</Title>
      <Text mb="md">Manage your todo lists and tasks.</Text>

      <Group align="flex-start" gap="xl" wrap="nowrap">
        {/* Sidebar */}
        <Stack w={220} gap="xs">
          <Group justify="space-between">
            <Title order={2} size="h4">
              Lists
            </Title>
            <ActionIcon variant="subtle" aria-label="New list" onClick={showNewListDialog}>
              <Plus size={20} strokeWidth={2} />
            </ActionIcon>
          </Group>
          <Stack gap={4}>
            {lists.map((list) => (
              <UnstyledButton
                key={list.id}
                onClick={() => selectList(list.id)}
                p="xs"
                style={{
                  borderRadius: 4,
                  background: list.id === selectedListId ? "var(--mantine-color-gray-1)" : undefined,
                }}
              >
                <Group gap="xs" wrap="nowrap">
                  <ColorSwatch color={list.colour} size={14} />
                  <Text flex={1}>{list.title}</Text>
                  <Text size="sm" c="dimmed">
                    {list.items.filter((t) => !t.done).length}
                  </Text>
                </Group>
              </UnstyledButton>
            ))}
          </Stack>
        </Stack>

        {/* Items panel */}
        {selectedList && (
          <Stack flex={1} gap="xs">
            <Group justify="space-between">
              <Title order={2} size="h4" c={selectedList.colour}>
                {selectedList.title}
              </Title>
              <ActionIcon variant="subtle" aria-label="List options" onClick={showListOptionsDialog}>
                <Settings size={20} strokeWidth={2} />
              </ActionIcon>
            </Group>

            {selectedList.items.map((item) => (
              <Group key={item.id} wrap="nowrap">
                <Checkbox
                  checked={item.done}
                  onChange={(e) => updateCheckbox(item, e.currentTarget.checked)}
                />
                {editingItemId === item.id ? (
                  <TextInput
                    flex={1}
                    value={editValue}
                    onChange={(e) => setEditValue(e.currentTarget.value)}
                    onKeyDown={(e) => {
                      if (e.key === "Enter") e.currentTarget.blur();
                      if (e.key === "Escape") cancelEdit();
                    }}
                    onBlur={() => {
                      if (editCancelledRef.current) {
                        editCancelledRef.current = false;
                        return;
                      }
                      commitEdit();
                    }}
                    maxLength={200}
                    autoFocus
                  />
                ) : (
                  <Text
                    flex={1}
                    td={item.done ? "line-through" : undefined}
                    c={item.done ? "dimmed" : undefined}
                    onClick={() => editItem(item)}
                    style={{ cursor: "pointer" }}
                  >
                    {item.title}
                  </Text>
                )}
                <ActionIcon
                  variant="subtle"
                  aria-label="Item details"
                  onClick={() => showItemDetailsDialog(item)}
                >
                  <MoreHorizontal size={20} strokeWidth={2} />
                </ActionIcon>
              </Group>
            ))}

            <Group wrap="nowrap">
              <Checkbox disabled />
              {addingItem ? (
                <TextInput
                  flex={1}
                  placeholder="New task…"
                  value={newItemTitle}
                  onChange={(e) => setNewItemTitle(e.currentTarget.value)}
                  onKeyDown={(e) => {
                    if (e.key === "Enter") commitNewItem();
                    if (e.key === "Escape") {
                      newItemCancelledRef.current = true;
                      setAddingItem(false);
                      setNewItemTitle("");
                    }
                  }}
                  onBlur={() => {
                    if (newItemCancelledRef.current) {
                      newItemCancelledRef.current = false;
                      return;
                    }
                    commitNewItem();
                  }}
                  maxLength={200}
                  autoFocus
                />
              ) : (
                <Text c="dimmed" onClick={() => setAddingItem(true)} style={{ cursor: "pointer" }}>
                  New task&hellip;
                </Text>
              )}
            </Group>
          </Stack>
        )}
      </Group>

      {/* New list dialog */}
      <Modal opened={newListOpened} onClose={closeNewList} title="New List">
        <Stack>
          <TextInput
            label="Title"
            placeholder="List title…"
            value={newListTitle}
            onChange={(e) => setNewListTitle(e.currentTarget.value)}
            error={newListError}
            onKeyDown={(e) => e.key === "Enter" && commitNewList()}
            maxLength={200}
            autoFocus
          />
          <Box>
            <Text size="sm" fw={500} mb={4}>
              Colour
            </Text>
            <Group gap="xs">
              {colours.map((c) => (
                <ColorSwatch
                  key={c.code}
                  color={c.code}
                  component="button"
                  aria-label={c.name}
                  style={{
                    cursor: "pointer",
                    outline: newListColour === c.code ? "2px solid var(--mantine-color-blue-6)" : undefined,
                    outlineOffset: 2,
                  }}
                  onClick={() => setNewListColour(c.code)}
                />
              ))}
            </Group>
          </Box>
          <Group justify="flex-end">
            <Button variant="default" onClick={closeNewList}>
              Cancel
            </Button>
            <Button onClick={commitNewList}>Create</Button>
          </Group>
        </Stack>
      </Modal>

      {/* List options dialog */}
      <Modal opened={listOptionsOpened} onClose={closeListOptions} title="List Options">
        <Stack>
          <TextInput
            label="Title"
            placeholder="List name…"
            value={listOptionsTitle}
            onChange={(e) => setListOptionsTitle(e.currentTarget.value)}
            onKeyDown={(e) => e.key === "Enter" && updateListOptions()}
            maxLength={200}
          />
          <Box>
            <Text size="sm" fw={500} mb={4}>
              Colour
            </Text>
            <Group gap="xs">
              {colours.map((c) => (
                <ColorSwatch
                  key={c.code}
                  color={c.code}
                  component="button"
                  aria-label={c.name}
                  style={{
                    cursor: "pointer",
                    outline:
                      listOptionsColour === c.code ? "2px solid var(--mantine-color-blue-6)" : undefined,
                    outlineOffset: 2,
                  }}
                  onClick={() => setListOptionsColour(c.code)}
                />
              ))}
            </Group>
          </Box>
          <Group justify="space-between">
            <Button color="red" variant="subtle" onClick={confirmDeleteList}>
              Delete
            </Button>
            <Group>
              <Button variant="default" onClick={closeListOptions}>
                Cancel
              </Button>
              <Button onClick={updateListOptions}>Update</Button>
            </Group>
          </Group>
        </Stack>
      </Modal>

      {/* Delete list confirmation */}
      <Modal opened={deleteListOpened} onClose={closeDeleteList} title={`Delete "${selectedList?.title}"?`}>
        <Stack>
          <Text>All items will be permanently deleted.</Text>
          <Group justify="flex-end">
            <Button variant="default" onClick={closeDeleteList}>
              Cancel
            </Button>
            <Button color="red" onClick={deleteListConfirmed}>
              Delete
            </Button>
          </Group>
        </Stack>
      </Modal>

      {/* Item details dialog */}
      <Modal opened={itemDetailsOpened} onClose={closeItemDetailsDialog} title="Item Details">
        <Stack>
          <Select
            label="List"
            data={lists.map((l) => ({ value: String(l.id), label: l.title }))}
            value={itemListId !== null ? String(itemListId) : null}
            onChange={(value) => value && setItemListId(Number(value))}
            allowDeselect={false}
          />
          <Select
            label="Priority"
            data={priorityLevels.map((p) => ({ value: String(p.id), label: p.title }))}
            value={itemPriority !== null ? String(itemPriority) : null}
            onChange={(value) => value && setItemPriority(Number(value))}
            allowDeselect={false}
          />
          <Textarea label="Note" rows={3} value={itemNote} onChange={(e) => setItemNote(e.currentTarget.value)} />
          <Group justify="space-between">
            <Button color="red" variant="subtle" onClick={deleteSelectedItem}>
              Delete
            </Button>
            <Group>
              <Button variant="default" onClick={closeItemDetailsDialog}>
                Cancel
              </Button>
              <Button onClick={updateItemDetails}>Update</Button>
            </Group>
          </Group>
        </Stack>
      </Modal>
    </div>
  );
}
