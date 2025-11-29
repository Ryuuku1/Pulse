import { useParams } from 'react-router-dom';
import { useState, useEffect, useMemo } from 'react';
import { useRealtimeMetrics, usePlantSummary } from '../api/hooks';
import { MetricCard } from '../components/MetricCard';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend } from 'recharts';
import { format } from 'date-fns';
import './RealtimeDashboard.css';

interface DataPoint {
  time: string;
  pv: number;
  grid?: number | null;
  battery?: number | null;
}

export function RealtimeDashboard() {
  const { plantId } = useParams<{ plantId: string }>();
  const { data: summary } = usePlantSummary(plantId);
  const { data: metrics, isLoading, error } = useRealtimeMetrics(plantId);
  const [historicalData, setHistoricalData] = useState<DataPoint[]>([]);

  useEffect(() => {
    if (metrics && metrics.pvPowerKw !== undefined) {
      setHistoricalData((prev) => {
        const newPoint: DataPoint = {
          time: format(new Date(metrics.timestampUtc), 'HH:mm:ss'),
          pv: metrics.pvPowerKw || 0,
          grid: metrics.gridPowerKw ?? null,
          battery: metrics.batteryPowerKw ?? null,
        };
        return [...prev, newPoint].slice(-60);
      });
    }
  }, [metrics]);

  const chartLines = useMemo(
    () => ({
      showGrid: historicalData.some((p) => p.grid !== undefined && p.grid !== null),
      showBattery: historicalData.some((p) => p.battery !== undefined && p.battery !== null),
    }),
    [historicalData]
  );

  if (isLoading && !metrics) return <LoadingSpinner message="Loading real-time data..." />;
  if (error) return <div className="error">Error loading metrics: {error.message}</div>;
  if (!metrics) return <div className="error">No data available</div>;

  return (
    <div className="realtime-dashboard">
      <div className="dashboard-header">
        <div>
          <p className="eyebrow">Live plant</p>
          <h1>Real-time Monitoring</h1>
          {summary && <p className="plant-name">{summary.plant.name}</p>}
        </div>
        <div className="live-pill">
          <span className="pulse" />
          <div>
            <strong>Live</strong>
            <span>Refreshed every 5s</span>
          </div>
        </div>
      </div>

      <div className="metrics-overview">
        <MetricCard
          title="PV Power"
          value={metrics.pvPowerKw?.toFixed(2) ?? '0.00'}
          unit="kW"
          subtitle="Inverter output"
          highlight
        />
        <MetricCard
          title={metrics.gridPowerKw && metrics.gridPowerKw < 0 ? 'Grid Import' : 'Grid Export'}
          value={Math.abs(metrics.gridPowerKw ?? 0).toFixed(2)}
          unit="kW"
          subtitle="Net grid flow"
          trend={metrics.gridPowerKw && metrics.gridPowerKw < 0 ? 'down' : 'up'}
        />
        <MetricCard
          title="Battery"
          value={Math.abs(metrics.batteryPowerKw ?? 0).toFixed(2)}
          unit="kW"
          subtitle={metrics.batteryPowerKw && metrics.batteryPowerKw < 0 ? 'Discharging' : 'Charging'}
        />
        <MetricCard
          title="State of Charge"
          value={metrics.stateOfChargePercent?.toFixed(0) ?? '0'}
          unit="%"
          subtitle="Battery SoC"
        />
        <MetricCard
          title="Grid Voltage"
          value={metrics.gridVoltageV?.toFixed(1) ?? '0'}
          unit="V"
          subtitle="Point of common coupling"
        />
        <MetricCard
          title="Frequency"
          value={metrics.gridFrequencyHz?.toFixed(2) ?? '0'}
          unit="Hz"
          subtitle="Grid frequency"
        />
        <MetricCard
          title="Temperature"
          value={metrics.temperatureC?.toFixed(1) ?? '0'}
          unit="C"
          subtitle="Inverter/internal"
        />
        <MetricCard
          title="Today"
          value={metrics.dayEnergyKwh?.toFixed(2) ?? '0.00'}
          unit="kWh"
          subtitle="Energy produced today"
        />
      </div>

      {historicalData.length > 0 && (
        <div className="chart-container">
          <div className="chart-header">
            <div>
              <h3>Live Power Trend</h3>
              <p>Rolling window of the last 5 minutes</p>
            </div>
            <div className="chart-legend">
              <span className="legend-dot pv" /> PV
              {chartLines.showGrid && (
                <>
                  <span className="legend-dot grid" /> Grid
                </>
              )}
              {chartLines.showBattery && (
                <>
                  <span className="legend-dot battery" /> Battery
                </>
              )}
            </div>
          </div>
          <ResponsiveContainer width="100%" height={320}>
            <LineChart data={historicalData}>
              <CartesianGrid strokeDasharray="3 3" stroke="rgba(255,255,255,0.08)" />
              <XAxis dataKey="time" stroke="rgba(232,237,247,0.75)" />
              <YAxis stroke="rgba(232,237,247,0.75)" />
              <Tooltip />
              <Legend />
              <Line type="monotone" dataKey="pv" name="PV" stroke="#4db4ff" strokeWidth={2} dot={false} />
              {chartLines.showGrid && (
                <Line type="monotone" dataKey="grid" name="Grid" stroke="#7c3aed" strokeWidth={2} dot={false} />
              )}
              {chartLines.showBattery && (
                <Line type="monotone" dataKey="battery" name="Battery" stroke="#22c55e" strokeWidth={2} dot={false} />
              )}
            </LineChart>
          </ResponsiveContainer>
        </div>
      )}

      <div className="timestamp">
        Last updated: {new Date(metrics.timestampUtc).toLocaleString()}
      </div>
    </div>
  );
}
