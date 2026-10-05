"use client";

import { useEffect } from "react";
import { fetchCurrentUserFx } from "@/entities/session";
import { uiSettingsLoaded } from "@/features/ui-settings";

/** One-time client start-up, mounted once in the root layout so it survives layout switches. */
export function ClientInit() {
  useEffect(() => {
    fetchCurrentUserFx();
    uiSettingsLoaded();
  }, []);

  return null;
}
