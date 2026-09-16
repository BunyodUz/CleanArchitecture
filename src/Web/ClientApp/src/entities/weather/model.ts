import { createEffect, createStore } from "effector";
import { weatherForecastsClient } from "@/shared/api/client";
import type { WeatherForecast } from "@/web-api-client";

export const fetchForecastsFx = createEffect(() => weatherForecastsClient.getWeatherForecasts());

export const $forecasts = createStore<WeatherForecast[]>([]).on(
  fetchForecastsFx.doneData,
  (_, forecasts) => forecasts,
);

export const $forecastsLoading = fetchForecastsFx.pending;
export const $forecastsError = createStore(false)
  .on(fetchForecastsFx.fail, () => true)
  .reset(fetchForecastsFx);
