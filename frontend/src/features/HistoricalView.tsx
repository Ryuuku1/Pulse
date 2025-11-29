import { useParams } from 'react-router-dom';
import { useState, useMemo } from 'react';
import { usePlantSummary, useTimeseries } from '../api/hooks';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { MetricType } from '../types/api';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Legend,
} from 'recharts';
import { format, subDays, startOfDay, endOfDay } from 'date-fns';
import './HistoricalView.css';

type TimeRange = 'today' | 'yesterday' | 'last7days' | 'last30days';

export function HistoricalView() {
  const { plantId } = useParams<{ plantId: string }>();
  const { data: summary } = usePlantSummary(plantId);
  const [timeRange, setTimeRange] = useState<TimeRange>('today');
  const [metricType, setMetricType] = useState<MetricType>(MetricType.Power);

  const getDateRange = (range: TimeRange) => {
    const now = new Date();
    switch (range) {
      case 'today':
        return { from: startOfDay(now), to: endOfDay(now) };
      case 'yesterday':
        return { from: startOfDay(subDays(now, 1)), to: endOfDay(subDays(now, 1)) };
      case 'last7days':
        return { from: startOfDay(subDays(now, 7)), to: endOfDay(now) };
      case 'last30days':
        return { from: startOfDay(subDays(now, 30)), to: endOfDay(now) };
    }
  };

  const { from, to } = getDateRange(timeRange);
  const { data: timeseries, isLoading, error } = useTimeseries(plantId, metricType, from, to);

  const chartData = timeseries?.map((point) => ({
    time: format(new Date(point.timestampUtc), 'MMM dd HH:mm'),
    value: point.value,
  }));

  const stats = useMemo(() => {
    if (!timeseries || timeseries.length === 0) return null;
    const values = timeseries.map((p) => p.value);
    const min = Math.min(...values);
    const max = Math.max(...values);
    const avg = values.reduce((a, b) => a + b, 0) / values.length;
    const last = values[values.length - 1];
    return { min, max, avg, last };
  }, [timeseries]);

  const metricLabel =
    metricType === MetricType.Power
      ? 'Power (kW)'
      : metricType === MetricType.Energy
        ? 'Energy (kWh)'
        : metricType === MetricType.Voltage
          ? 'Voltage (V)'
          : 'Temperature (C)';

  return (
    <div className="historical-view">
      <div className="view-header">
        <div>
          <p className="eyebrow">Trends</p>
          <h1>Historical Data</h1>
          {summary && <p className="plant-name">{summary.plant.name}</p>}
        </div>
        <div className="range-chip">
          <span>Range</span>
          <strong>
            {format(from, 'MMM dd')} - {format(to, 'MMM dd')}
          </strong>
        </div>
      </div>

      <div className="controls">
        <div className="control-group">
          <label>Time Range</label>
          <div className="button-group">
            <button className={timeRange === 'today' ? 'active' : ''} onClick={() => setTimeRange('today')}>
              Today
            </button>
            <button
              className={timeRange === 'yesterday' ? 'active' : ''}
              onClick={() => setTimeRange('yesterday')}
            >
              Yesterday
            </button>
            <button
              className={timeRange === 'last7days' ? 'active' : ''}
              onClick={() => setTimeRange('last7days')}
            >
              Last 7 Days
            </button>
            <button
              className={timeRange === 'last30days' ? 'active' : ''}
              onClick={() => setTimeRange('last30days')}
            >
              Last 30 Days
            </button>
          </div>
        </div>

        <div className="control-group">
          <label>Metric</label>
          <select value={metricType} onChange={(e) => setMetricType(Number(e.target.value) as MetricType)}>
            <option value={MetricType.Power}>Power (kW)</option>
            <option value={MetricType.Energy}>Energy (kWh)</option>
            <option value={MetricType.Voltage}>Voltage (V)</option>
            <option value={MetricType.Temperature}>Temperature (C)</option>
          </select>
        </div>
      </div>

      {isLoading && <LoadingSpinner message="Loading historical data..." />}

      {stats && (
        <div className="kpi-grid">
          <div className="kpi-card">
            <span>Last</span>
            <strong>{stats.last.toFixed(2)}</strong>
            <small>{metricLabel}</small>
          </div>
          <div className="kpi-card">
            <span>Average</span>
            <strong>{stats.avg.toFixed(2)}</strong>
            <small>{metricLabel}</small>
          </div>
          <div className="kpi-card">
            <span>Peak</span>
            <strong>{stats.max.toFixed(2)}</strong>
            <small>{metricLabel}</small>
          </div>
          <div className="kpi-card">
            <span>Lowest</span>
            <strong>{stats.min.toFixed(2)}</strong>
            <small>{metricLabel}</small>
          </div>
        </div>
      )}

      {error && <div className="error">Error loading data: {error.message}</div>}

      {!isLoading && !error && (!chartData || chartData.length === 0) && (
        <div className="no-data">
          <p>No historical data available for the selected time range.</p>
          <p className="hint">
            Historical collection stores arriving real-time points. Let the poller run a bit longer to
            build up a timeline.
          </p>
        </div>
      )}

      {chartData && chartData.length > 0 && (
        <div className="chart-container">
          <h3>{metricLabel} over time</h3>
          <ResponsiveContainer width="100%" height={400}>
            <LineChart data={chartData}>
              <CartesianGrid strokeDasharray="3 3" stroke="rgba(255,255,255,0.08)" />
              <XAxis dataKey="time" stroke="rgba(232,237,247,0.75)" interval="preserveStartEnd" />
              <YAxis stroke="rgba(232,237,247,0.75)" />
              <Tooltip />
              <Legend />
              <Line
                type="monotone"
                dataKey="value"
                stroke="#4db4ff"
                strokeWidth={2}
                dot={false}
                name={metricLabel}
              />
            </LineChart>
          </ResponsiveContainer>
        </div>
      )}
    </div>
  );
}
