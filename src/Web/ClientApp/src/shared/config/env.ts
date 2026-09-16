// Static export means there is no server at runtime, so this is inlined at build time.
// It's set (via .env.development) for `next dev`, and left unset for `next build`, where
// the exported site is served same-origin by Kestrel and a relative base URL is correct.
export const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL ?? "";
