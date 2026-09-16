"use client";

import { Button, Text, Title } from "@mantine/core";
import { useState } from "react";

export function CounterView() {
  const [count, setCount] = useState(0);

  return (
    <div>
      <Title order={1}>Counter</Title>
      <Text>This is a simple example of a React component.</Text>
      <Text component="p" aria-live="polite">
        Current count: <strong>{count}</strong>
      </Text>
      <Button onClick={() => setCount((c) => c + 1)}>Increment</Button>
    </div>
  );
}
