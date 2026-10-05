"use client";

import { Alert, Skeleton, Stack, Table } from "@mantine/core";
import { useUnit } from "effector-react";
import { CloudOff } from "lucide-react";
import { useEffect } from "react";
import { $forecasts, $forecastsError, $forecastsLoading, fetchForecastsFx } from "@/entities/weather";
import { PageHeader } from "@/shared/ui/PageHeader";

export function WeatherView() {
  const [forecasts, loading, error] = useUnit([$forecasts, $forecastsLoading, $forecastsError]);

  useEffect(() => {
    fetchForecastsFx().catch(() => undefined);
  }, []);

  return (
    <div>
      <PageHeader
        title="Weather"
        description="This component demonstrates fetching data from the server."
        breadcrumbs={[{ label: "Home", href: "/" }, { label: "Weather" }]}
      />
      {loading && (
        <Stack gap="xs" aria-busy="true" aria-label="Fetching your weather forecast">
          {Array.from({ length: 5 }, (_, i) => (
            <Skeleton key={i} h={36} radius="sm" />
          ))}
        </Stack>
      )}
      {error && (
        <Alert color="red" icon={<CloudOff size={18} />} title="Unable to load weather forecasts">
          Please try again later.
        </Alert>
      )}
      {!loading && !error && (
        <Table.ScrollContainer minWidth={480}>
          <Table highlightOnHover>
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
        </Table.ScrollContainer>
      )}
    </div>
  );
}
