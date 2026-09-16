"use client";

import { Table, Text, Title } from "@mantine/core";
import { useUnit } from "effector-react";
import { useEffect } from "react";
import { $forecasts, $forecastsError, $forecastsLoading, fetchForecastsFx } from "@/entities/weather";

export function WeatherView() {
  const [forecasts, loading, error] = useUnit([$forecasts, $forecastsLoading, $forecastsError]);

  useEffect(() => {
    fetchForecastsFx();
  }, []);

  return (
    <div>
      <Title order={1}>Weather</Title>
      <Text>This component demonstrates fetching data from the server.</Text>
      {loading && <Text aria-busy="true">Fetching your weather forecast&hellip;</Text>}
      {error && <Text c="red">Unable to load weather forecasts. Please try again later.</Text>}
      {!loading && !error && (
        <Table mt="md">
          <Table.Thead>
            <Table.Tr>
              <Table.Th>Date</Table.Th>
              <Table.Th>Temp. (C)</Table.Th>
              <Table.Th>Temp. (F)</Table.Th>
              <Table.Th>Summary</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {forecasts.map((forecast, index) => (
              <Table.Tr key={forecast.date?.toISOString() ?? index}>
                <Table.Td>
                  {forecast.date?.toLocaleDateString("en-US", {
                    month: "short",
                    day: "numeric",
                    year: "numeric",
                  })}
                </Table.Td>
                <Table.Td>{forecast.temperatureC}</Table.Td>
                <Table.Td>{forecast.temperatureF}</Table.Td>
                <Table.Td>{forecast.summary}</Table.Td>
              </Table.Tr>
            ))}
          </Table.Tbody>
        </Table>
      )}
    </div>
  );
}
