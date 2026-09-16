import {
  AccountClient,
  TodoItemsClient,
  TodoListsClient,
  WeatherForecastsClient,
} from "../../web-api-client";
import { API_BASE_URL } from "@/shared/config/env";

// nswag.json sets requestCredentials: "include", but NSwag's Fetch template (v14.7.1) doesn't
// actually emit it — every generated method builds its RequestInit without a `credentials` key.
// Without it, the BFF's auth cookie never gets attached to cross-origin requests in dev. Rather
// than depend on codegen output that's silently ignoring a documented option, every client here
// is given this wrapper explicitly instead of relying on the generated default (`window`).
const httpClient = {
  fetch(url: RequestInfo, init?: RequestInit): Promise<Response> {
    return fetch(url, { ...init, credentials: "include" });
  },
};

// Single place that knows the generated clients need the API base URL — everything else
// imports from here instead of constructing a client directly.
export const accountClient = new AccountClient(API_BASE_URL, httpClient);
export const todoListsClient = new TodoListsClient(API_BASE_URL, httpClient);
export const todoItemsClient = new TodoItemsClient(API_BASE_URL, httpClient);
export const weatherForecastsClient = new WeatherForecastsClient(API_BASE_URL, httpClient);
