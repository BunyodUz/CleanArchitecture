const relative = new Intl.RelativeTimeFormat("en", { numeric: "auto" });
const absolute = new Intl.DateTimeFormat("en", { dateStyle: "medium", timeStyle: "short" });

const UNITS: [Intl.RelativeTimeFormatUnit, number][] = [
  ["year", 365 * 24 * 3600],
  ["month", 30 * 24 * 3600],
  ["week", 7 * 24 * 3600],
  ["day", 24 * 3600],
  ["hour", 3600],
  ["minute", 60],
];

/** "5 minutes ago", "yesterday", "just now". */
export function formatRelativeTime(date: Date, now = new Date()) {
  const seconds = (date.getTime() - now.getTime()) / 1000;
  for (const [unit, size] of UNITS) {
    if (Math.abs(seconds) >= size) return relative.format(Math.round(seconds / size), unit);
  }
  return "just now";
}

/** "Oct 5, 2026, 9:41 AM". */
export const formatDateTime = (date: Date) => absolute.format(date);
