// TypeScript types matching backend DTOs

export enum PlantStatus {
  Unknown = 0,
  Connected = 1,
  Disconnected = 2,
  Fault = 3,
  Offline = 4,
}

export enum DeviceStatus {
  Unknown = 0,
  Normal = 1,
  Fault = 2,
  Offline = 3,
  Standby = 4,
  Shutdown = 5,
}

export enum DeviceType {
  Unknown = 0,
  StringInverter = 1,
  ResidentialInverter = 2,
  Battery = 3,
  GridMeter = 4,
  PowerSensor = 5,
  EMI = 6,
  ESS = 7,
}

export enum MetricType {
  Power = 1,
  Energy = 2,
  Voltage = 3,
  Current = 4,
  Temperature = 5,
  Frequency = 6,
  StateOfCharge = 7,
  Efficiency = 8,
}

export interface Plant {
  id: string;
  name: string;
  address?: string;
  installedCapacityKw?: number;
  latitude?: number;
  longitude?: number;
  installationDate?: string;
  status: PlantStatus;
  lastUpdateTime?: string;
}

export interface Device {
  id: string;
  plantId: string;
  name: string;
  type: DeviceType;
  model?: string;
  serialNumber?: string;
  firmwareVersion?: string;
  status: DeviceStatus;
  lastCommunicationTime?: string;
}

export interface RealtimeMetrics {
  timestampUtc: string;
  pvPowerKw?: number;
  gridPowerKw?: number;
  loadPowerKw?: number;
  batteryPowerKw?: number;
  stateOfChargePercent?: number;
  efficiencyPercent?: number;
  dayEnergyKwh?: number;
  gridVoltageV?: number;
  gridFrequencyHz?: number;
  pvVoltageV?: number;
  temperatureC?: number;
}

export interface EnergySummary {
  timestampUtc: string;
  energyTodayKwh: number;
  energyMonthKwh: number;
  energyYearKwh: number;
  energyTotalKwh: number;
  gridImportTodayKwh?: number;
  gridExportTodayKwh?: number;
  batteryChargeTodayKwh?: number;
  batteryDischargeTodayKwh?: number;
}

export interface PlantSummary {
  plant: Plant;
  currentMetrics?: RealtimeMetrics;
  energySummary?: EnergySummary;
  totalDevices: number;
  activeDevices: number;
}

export interface TimeseriesPoint {
  timestampUtc: string;
  metricType: MetricType;
  value: number;
  unit?: string;
}

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: string;
}

export interface HealthResponse {
  status: string;
  timestamp: string;
}
