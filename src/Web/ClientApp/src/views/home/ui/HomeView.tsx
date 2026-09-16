"use client";

import { Anchor, List, Text, Title } from "@mantine/core";

export function HomeView() {
  return (
    <div>
      <Title order={1}>Welcome</Title>
      <Text>
        A full-stack application with a{" "}
        <Anchor href="https://react.dev/" target="_blank" rel="noreferrer">
          React
        </Anchor>{" "}
        frontend and an{" "}
        <Anchor href="https://get.asp.net/" target="_blank" rel="noreferrer">
          ASP.NET Core
        </Anchor>{" "}
        backend, built with:
      </Text>
      <List mt="sm">
        <List.Item>
          <Anchor href="https://get.asp.net/" target="_blank" rel="noreferrer">
            ASP.NET Core
          </Anchor>{" "}
          and{" "}
          <Anchor href="https://learn.microsoft.com/en-us/dotnet/csharp/" target="_blank" rel="noreferrer">
            C#
          </Anchor>{" "}
          for cross-platform server-side code
        </List.Item>
        <List.Item>
          <Anchor href="https://nextjs.org/" target="_blank" rel="noreferrer">
            Next.js
          </Anchor>{" "}
          and{" "}
          <Anchor href="https://react.dev/" target="_blank" rel="noreferrer">
            React
          </Anchor>{" "}
          for client-side code, statically exported and served by ASP.NET Core
        </List.Item>
        <List.Item>
          <Anchor href="https://mantine.dev/" target="_blank" rel="noreferrer">
            Mantine
          </Anchor>{" "}
          for UI components and styling
        </List.Item>
        <List.Item>
          <Anchor href="https://effector.dev/" target="_blank" rel="noreferrer">
            Effector
          </Anchor>{" "}
          for state management
        </List.Item>
      </List>
      <Text mt="md">To help you get started:</Text>
      <List mt="sm">
        <List.Item>
          <strong>Client-side navigation.</strong> Click <em>Counter</em> then <em>Home</em> to return here.
        </List.Item>
        <List.Item>
          <strong>Feature-Sliced Design.</strong> The <code>ClientApp</code> source is organized into{" "}
          <code>app</code>, <code>views</code>, <code>widgets</code>, <code>features</code>,{" "}
          <code>entities</code> and <code>shared</code> layers.
        </List.Item>
        <List.Item>
          <strong>Static production builds.</strong> <code>dotnet publish</code> runs{" "}
          <code>next build</code> and copies the exported site into <code>wwwroot</code>, served
          directly by Kestrel — there is no Node.js server in production.
        </List.Item>
      </List>
      <Text mt="md">
        The <code>ClientApp</code> subdirectory is a Next.js application. Open a command prompt
        there to run <code>npm</code> commands such as <code>npm run dev</code> or{" "}
        <code>npm install</code>.
      </Text>
    </div>
  );
}
