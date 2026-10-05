"use client";

import {
  ActionIcon,
  Center,
  Popover,
  SegmentedControl,
  Stack,
  Switch,
  Text,
  Tooltip,
  useMantineColorScheme,
  type MantineColorScheme,
} from "@mantine/core";
import { useUnit } from "effector-react";
import { Monitor, Moon, SlidersHorizontal, Sun } from "lucide-react";
import { $collapsedByDefault, collapsedByDefaultChanged } from "../model";

const label = (Icon: typeof Sun, text: string) => (
  <Center style={{ gap: 6 }}>
    <Icon size={14} />
    <span>{text}</span>
  </Center>
);

export function UiSettingsMenu() {
  const { colorScheme, setColorScheme } = useMantineColorScheme();
  const [collapsedByDefault, setCollapsedByDefault] = useUnit([$collapsedByDefault, collapsedByDefaultChanged]);

  return (
    <Popover position="bottom-end" width={280} shadow="md" withArrow>
      <Popover.Target>
        <Tooltip label="UI settings" openDelay={400}>
          <ActionIcon variant="subtle" color="gray" size="lg" aria-label="UI settings">
            <SlidersHorizontal size={18} />
          </ActionIcon>
        </Tooltip>
      </Popover.Target>
      <Popover.Dropdown>
        <Stack gap="md">
          <div>
            <Text fw={600} fz="sm" mb={6}>
              Theme
            </Text>
            <SegmentedControl
              fullWidth
              size="xs"
              value={colorScheme}
              onChange={(value) => setColorScheme(value as MantineColorScheme)}
              data={[
                { value: "auto", label: label(Monitor, "System") },
                { value: "light", label: label(Sun, "Light") },
                { value: "dark", label: label(Moon, "Dark") },
              ]}
            />
          </div>
          <Switch
            label="Start with the sidebar collapsed"
            checked={collapsedByDefault}
            onChange={(event) => setCollapsedByDefault(event.currentTarget.checked)}
          />
        </Stack>
      </Popover.Dropdown>
    </Popover>
  );
}
