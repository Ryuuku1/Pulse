import type {
  ApiResponse,
  Plant,
  PlantSummary,
  Device,
  RealtimeMetrics,
  TimeseriesPoint,
  MetricType,
  HealthResponse,
} from '../types/api';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

class ApiError extends Error {
  public statusCode?: number;

  constructor(
    message: string,
    statusCode?: number
  ) {
    super(message);
    this.name = 'ApiError';
    this.statusCode = statusCode;
  }
}

async function fetchApi<T>(endpoint: string, options?: RequestInit): Promise<T> {
  const url = `${API_BASE_URL}${endpoint}`;

  try {
    const response = await fetch(url, {
      ...options,
      headers: {
        'Content-Type': 'application/json',
        ...options?.headers,
      },
    });

    if (!response.ok) {
      throw new ApiError(
        `API request failed: ${response.statusText}`,
        response.status
      );
    }

    const data = await response.json();
    return data;
  } catch (error) {
    if (error instanceof ApiError) {
      throw error;
    }
    throw new ApiError(`Network error: ${error instanceof Error ? error.message : 'Unknown error'}`);
  }
}

export const api = {
  // Health check
  health: async (): Promise<HealthResponse> => {
    return fetchApi<HealthResponse>('/health');
  },

  // Plants
  getPlants: async (): Promise<Plant[]> => {
    const response = await fetchApi<ApiResponse<Plant[]>>('/api/plants');
    if (!response.success || !response.data) {
      throw new ApiError(response.error || 'Failed to fetch plants');
    }
    return response.data;
  },

  getPlantSummary: async (plantId: string): Promise<PlantSummary> => {
    const response = await fetchApi<ApiResponse<PlantSummary>>(`/api/plants/${plantId}`);
    if (!response.success || !response.data) {
      throw new ApiError(response.error || 'Failed to fetch plant summary');
    }
    return response.data;
  },

  getDevices: async (plantId: string): Promise<Device[]> => {
    const response = await fetchApi<ApiResponse<Device[]>>(`/api/plants/${plantId}/devices`);
    if (!response.success || !response.data) {
      throw new ApiError(response.error || 'Failed to fetch devices');
    }
    return response.data;
  },

  // Metrics
  getPlantRealtimeMetrics: async (plantId: string): Promise<RealtimeMetrics> => {
    const response = await fetchApi<ApiResponse<RealtimeMetrics>>(
      `/api/metrics/plants/${plantId}/realtime`
    );
    if (!response.success || !response.data) {
      throw new ApiError(response.error || 'Failed to fetch realtime metrics');
    }
    return response.data;
  },

  getPlantTimeseries: async (
    plantId: string,
    metricType: MetricType,
    from: Date,
    to: Date
  ): Promise<TimeseriesPoint[]> => {
    const params = new URLSearchParams({
      metricType: metricType.toString(),
      from: from.toISOString(),
      to: to.toISOString(),
    });

    const response = await fetchApi<ApiResponse<TimeseriesPoint[]>>(
      `/api/metrics/plants/${plantId}/timeseries?${params}`
    );

    if (!response.success || !response.data) {
      throw new ApiError(response.error || 'Failed to fetch timeseries data');
    }
    return response.data;
  },
};

export { ApiError };
