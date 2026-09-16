# CleanArchitecture Client

This project uses [Next.js](https://nextjs.org/) (App Router, static export) with React 19 and
TypeScript, [Mantine](https://mantine.dev/) for UI components, and
[Effector](https://effector.dev/) for state management. The source is organized using
[Feature-Sliced Design](https://feature-sliced.design/).

## Available Scripts

### `npm run dev`

Runs the app in development mode. Regenerates the API client first (`generate-api`), then starts
the Next.js dev server. There is no dev-time proxy — the app calls the API using an absolute base
URL (see [Environment Variables](#environment-variables)) and the API allows credentialed
cross-origin requests in development (see `Program.cs`).

### `npm run build`

Regenerates the API client, then produces a static export in the `out/` folder. This is what
`dotnet publish` runs as part of building the Web project — the exported site is copied into
`wwwroot` and served directly by Kestrel; there is no Node.js server in production.

### `npm run lint`

Runs ESLint on the `src` directory.

### `npm run generate-api`

Regenerates `src/web-api-client.ts` from the backend's OpenAPI document using NSwag. Runs
automatically before `dev` and `build`.

## Project Structure

Feature-Sliced Design layers, from most to least specific:

- `src/app/` - Next.js App Router routes (`layout.tsx`, `page.tsx` per route) and app-wide setup
- `src/views/` - one folder per page/route, composing widgets, features and entities (FSD's
  "pages" layer, renamed to avoid colliding with Next's reserved `app/`/`pages/` folder names)
- `src/widgets/` - composite, self-contained UI blocks (e.g. `layout`, `nav-menu`)
- `src/features/` - user-facing actions with associated logic (e.g. `auth`)
- `src/entities/` - business entities and their Effector models (`session`, `todo`, `weather`)
- `src/shared/` - reusable, business-agnostic code (`api` client wiring, `config`, `ui`, `lib`)

Each slice exposes its public API through an `index.ts` — import from the slice root
(`@/entities/todo`), not its internal files.

- `public/` - static assets (favicon, manifest)
- `next.config.ts` - static export configuration and dev-time API base URL resolution

## Environment Variables

`NEXT_PUBLIC_API_BASE_URL` is set by `next.config.ts`, not a `.env` file: in production it's
empty (the exported site is served same-origin by Kestrel), and in development it resolves to
the API's address — either injected by AppHost (when run through Aspire) or a fixed fallback
matching the Web project's `https` launch profile.

## Learn More

- [Next.js Documentation](https://nextjs.org/docs)
- [Mantine Documentation](https://mantine.dev/)
- [Effector Documentation](https://effector.dev/)
- [Feature-Sliced Design](https://feature-sliced.design/)
