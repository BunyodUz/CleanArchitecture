"use client";

import { useEffect } from "react";
import { fetchCurrentUserFx } from "@/entities/session";

export function SessionInit() {
  useEffect(() => {
    fetchCurrentUserFx();
  }, []);

  return null;
}
