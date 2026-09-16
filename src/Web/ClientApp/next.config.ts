import type { NextConfig } from "next";

// Static export: the build output is copied into the ASP.NET Core app's wwwroot and served
// by Kestrel — there is no Next.js server in production. See src/Web/Web.csproj
// (PublishRunWebpack target) and README for how this fits into the .NET build/publish.
//
// In dev, Next has no proxy (rewrites()/etc. don't apply to `output: "export"`), so the app
// calls the API cross-origin using an absolute base URL instead (see shared/config/env.ts and
// Program.cs's CORS policy). AppHost injects the API's actual address as services__webapi__*
// when the frontend is launched through Aspire; running `npm run dev` on its own (API started
// separately via `dotnet run --project src/Web`) falls back to the "https" launchSettings.json
// profile's fixed port. Neither applies to a production build, which must stay same-origin.
const isDev = process.env.NODE_ENV !== "production";
const apiBaseUrl = isDev
  ? (process.env.services__webapi__https__0 ?? process.env.services__webapi__http__0 ?? "https://localhost:5001")
  : "";

const nextConfig: NextConfig = {
  output: "export",
  images: { unoptimized: true },
  env: {
    NEXT_PUBLIC_API_BASE_URL: apiBaseUrl,
  },
};

export default nextConfig;
