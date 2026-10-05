"use client";

import { Button, Text } from "@mantine/core";
import { useState } from "react";
import { PageHeader } from "@/shared/ui/PageHeader";

export function CounterView() {
  const [count, setCount] = useState(0);

  return (
    <div>
      <PageHeader
        title="Counter"
        description="This is a simple example of a React component."
        breadcrumbs={[{ label: "Home", href: "/" }, { label: "Counter" }]}
      />
      <Text component="p" aria-live="polite" mb="sm">
        Current count: <strong>{count}</strong>
      </Text>
      <Button onClick={() => setCount((c) => c + 1)}>Increment</Button>
    </div>
  );
}
