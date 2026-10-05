import { createEvent, createStore, sample } from "effector";

const STORAGE_KEY = "ca.ui.sidebarCollapsedByDefault";

export const uiSettingsLoaded = createEvent();
export const sidebarToggled = createEvent();
export const collapsedByDefaultChanged = createEvent<boolean>();

/** The user's saved preference: start each visit with the sidebar collapsed. */
export const $collapsedByDefault = createStore(false).on(collapsedByDefaultChanged, (_, value) => value);

/** The sidebar's current state on desktop (phones use the slide-in menu instead). */
export const $sidebarCollapsed = createStore(false)
  .on(sidebarToggled, (collapsed) => !collapsed)
  .on(collapsedByDefaultChanged, (_, value) => value);

// Read once on the client; localStorage can be unavailable (private mode, blocked storage).
sample({
  clock: uiSettingsLoaded,
  fn: () => {
    try {
      return localStorage.getItem(STORAGE_KEY) === "true";
    } catch {
      return false;
    }
  },
  target: [$collapsedByDefault, $sidebarCollapsed],
});

collapsedByDefaultChanged.watch((value) => {
  try {
    localStorage.setItem(STORAGE_KEY, String(value));
  } catch {
    // Preference just won't persist.
  }
});
