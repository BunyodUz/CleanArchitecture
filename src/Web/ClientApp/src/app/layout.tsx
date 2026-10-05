import "@mantine/core/styles.css";
import "@mantine/notifications/styles.css";
import "@mantine/spotlight/styles.css";
import { ColorSchemeScript, mantineHtmlProps } from "@mantine/core";
import type { Metadata, Viewport } from "next";
import type { ReactNode } from "react";
import { SessionExpiredModal } from "@/features/auth";
import { Providers } from "@/shared/ui/Providers";
import { ClientInit } from "./ClientInit";

export const metadata: Metadata = {
  title: "Clean Architecture",
  description: "A full-stack application built with ASP.NET Core, Next.js, Effector and Mantine.",
};

export const viewport: Viewport = {
  width: "device-width",
  initialScale: 1,
};

// The page frame lives in the (main) and admin route-group layouts, so each area can choose
// its own shell; this root layout only holds what every page shares.
export default function RootLayout({ children }: { children: ReactNode }) {
  return (
    <html lang="en" {...mantineHtmlProps}>
      <head>
        <ColorSchemeScript defaultColorScheme="auto" />
        <link rel="icon" href="/favicon.png" />
        <link rel="manifest" href="/manifest.webmanifest" />
      </head>
      <body>
        <Providers>
          <ClientInit />
          <SessionExpiredModal />
          {children}
        </Providers>
      </body>
    </html>
  );
}
