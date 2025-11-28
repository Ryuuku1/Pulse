import { useParams } from 'react-router-dom';
import { useState, useEffect } from 'react';
import { useRealtimeMetrics, usePlantSummary } from '../api/hooks';
import { MetricCard } from '../components/MetricCard';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import { format } from 'date-fns';
import './RealtimeDashboard.css';

interface DataPoint {
  time: string;
  power: number;
}

export function RealtimeDashboard() {
  const { plantId } = useParams<{ plantId: string }>();
  const { data: summary } = usePlantSummary(plantId);
  const { data: metrics, isLoading, error } = useRealtimeMetrics(plantId);
  const [historicalData, setHistoricalData] = useState<DataPoint[]>([]);

  // Track historical data points for the chart
  useEffect(() => {
    if (metrics && metrics.pvPowerKw !== undefined) {
      setHistoricalData((prev) => {
        const newPoint = {
          time: format(new Date(metrics.timestampUtc), 'HH:mm:ss'),
          power: metrics.pvPowerKw || 0,
        };

        // Keep last 30 points (approx 2.5 minutes at 5-second intervals)
        const updated = [...prev, newPoint].slice(-30);
        return updated;
      });
    }
  }, [metrics]);

  if (isLoading && !metrics) return <LoadingSpinner message="Loading real-time data..." />;
  if (error) return <div className="error">Error loading metrics: {error.message}</div>;
  if (!metrics) return <div className="error">No data available</div>;

  return (
    <div className="realtime-dashboard">
      <div className="dashboard-header">
        <h1>Real-time Monitoring</h1>
        {summary && <h2>{summary.plant.name}</h2>}
        <p className="live-indicator">
          <span className="pulse"></span> Live updating every 5 seconds
        </p>
      </div>

      <div className="metrics-overview">
        {metrics.pvPowerKw !== undefined && (
          <MetricCard
            title="PV Power"
            value={metrics.pvPowerKw.toFixed(2)}
            unit="kW"
            icon="⚡"
          />
        )}

        {metrics.gridPowerKw !== undefined && (
          <MetricCard
            title={metrics.gridPowerKw >= 0 ? 'Grid Export' : 'Grid Import'}
            value={Math.abs(metrics.gridPowerKw).toFixed(2)}
            unit="kW"
            icon={metrics.gridPowerKw >= 0 ? '↗' : '↙'}
            trend={metrics.gridPowerKw >= 0 ? 'up' : 'down'}
          />
        )}

        {metrics.dayEnergyKwh !== undefined && (
          <MetricCard title="Today's Energy" value={metrics.dayEnergyKwh.toFixed(2)} unit="kWh" icon="📅" />
        )}

        {metrics.efficiencyPercent !== undefined && (
          <MetricCard
            title="Efficiency"
            value={metrics.efficiencyPercent.toFixed(1)}
            unit="%"
            icon="📈"
          />
        )}

        {metrics.gridVoltageV !== undefined && (
          <MetricCard title="Grid Voltage" value={metrics.gridVoltageV.toFixed(1)} unit="V" icon="🔌" />
        )}

        {metrics.temperatureC !== undefined && (
          <MetricCard
            title="Temperature"
            value={metrics.temperatureC.toFixed(1)}
            unit="°C"
            icon="🌡️"
          />
        )}
      </div>

      {historicalData.length > 0 && (
        <div className="chart-container">
          <h3>Power Output (Last 2.5 minutes)</h3>
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={historicalData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="time" />
              <YAxis label={{ value: 'Power (kW)', angle: -90, position: 'insideLeft' }} />
              <Tooltip />
              <Line
                type="monotone"
                dataKey="power"
                stroke="#667eea"
                strokeWidth={2}
                dot={false}
                isAnimationActive={false}
              />
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
