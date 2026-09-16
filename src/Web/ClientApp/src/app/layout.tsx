import "@mantine/core/styles.css";
import { ColorSchemeScript, mantineHtmlProps } from "@mantine/core";
import type { Metadata, Viewport } from "next";
import type { ReactNode } from "react";
import { Layout } from "@/widgets/layout";
import { Providers } from "@/shared/ui/Providers";
import { SessionInit } from "./SessionInit";

export const metadata: Metadata = {
  title: "Clean Architecture",
  description: "A full-stack application built with ASP.NET Core, Next.js, Effector and Mantine.",
};

export const viewport: Viewport = {
  width: "device-width",
  initialScale: 1,
};

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
          <SessionInit />
          <Layout>{children}</Layout>
        </Providers>
      </body>
    </html>
  );
}
