import { useQuery, type UseQueryResult } from '@tanstack/react-query';
import { api } from './client';
import type {
  Plant,
  PlantSummary,
  Device,
  RealtimeMetrics,
  TimeseriesPoint,
  MetricType,
} from '../types/api';

// Query keys
export const queryKeys = {
  plants: ['plants'] as const,
  plantSummary: (plantId: string) => ['plants', plantId, 'summary'] as const,
  devices: (plantId: string) => ['plants', plantId, 'devices'] as const,
  realtimeMetrics: (plantId: string) => ['plants', plantId, 'realtime'] as const,
  timeseries: (plantId: string, metricType: MetricType, from: Date, to: Date) =>
    ['plants', plantId, 'timeseries', metricType, from.toISOString(), to.toISOString()] as const,
};

// Hooks
export function usePlants(): UseQueryResult<Plant[], Error> {
  return useQuery({
    queryKey: queryKeys.plants,
    queryFn: api.getPlants,
    staleTime: 30000, // 30 seconds
    refetchInterval: 30000, // Refetch every 30 seconds
  });
}

export function usePlantSummary(plantId: string | undefined): UseQueryResult<PlantSummary, Error> {
  return useQuery({
    queryKey: queryKeys.plantSummary(plantId!),
    queryFn: () => api.getPlantSummary(plantId!),
    enabled: !!plantId,
    staleTime: 10000, // 10 seconds
    refetchInterval: 10000, // Refetch every 10 seconds for near real-time
  });
}

export function useDevices(plantId: string | undefined): UseQueryResult<Device[], Error> {
  return useQuery({
    queryKey: queryKeys.devices(plantId!),
    queryFn: () => api.getDevices(plantId!),
    enabled: !!plantId,
    staleTime: 60000, // 1 minute
  });
}

export function useRealtimeMetrics(
  plantId: string | undefined
): UseQueryResult<RealtimeMetrics, Error> {
  return useQuery({
    queryKey: queryKeys.realtimeMetrics(plantId!),
    queryFn: () => api.getPlantRealtimeMetrics(plantId!),
    enabled: !!plantId,
    staleTime: 5000, // 5 seconds
    refetchInterval: 5000, // Refetch every 5 seconds for real-time feel
  });
}

export function useTimeseries(
  plantId: string | undefined,
  metricType: MetricType,
  from: Date,
  to: Date
): UseQueryResult<TimeseriesPoint[], Error> {
  return useQuery({
    queryKey: queryKeys.timeseries(plantId!, metricType, from, to),
    queryFn: () => api.getPlantTimeseries(plantId!, metricType, from, to),
    enabled: !!plantId,
    staleTime: 60000, // 1 minute
  });
}
