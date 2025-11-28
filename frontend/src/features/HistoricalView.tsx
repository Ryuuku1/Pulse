import { useParams } from 'react-router-dom';
import { useState } from 'react';
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
        return {
          from: startOfDay(subDays(now, 1)),
          to: endOfDay(subDays(now, 1)),
        };
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

  return (
    <div className="historical-view">
      <div className="view-header">
        <h1>Historical Data</h1>
        {summary && <h2>{summary.plant.name}</h2>}
      </div>

      <div className="controls">
        <div className="control-group">
          <label>Time Range:</label>
          <div className="button-group">
            <button
              className={timeRange === 'today' ? 'active' : ''}
              onClick={() => setTimeRange('today')}
            >
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
          <label>Metric:</label>
          <select
            value={metricType}
            onChange={(e) => setMetricType(Number(e.target.value) as MetricType)}
          >
            <option value={MetricType.Power}>Power (kW)</option>
            <option value={MetricType.Energy}>Energy (kWh)</option>
            <option value={MetricType.Voltage}>Voltage (V)</option>
            <option value={MetricType.Temperature}>Temperature (°C)</option>
          </select>
        </div>
      </div>

      {isLoading && <LoadingSpinner message="Loading historical data..." />}

      {error && <div className="error">Error loading data: {error.message}</div>}

      {!isLoading && !error && (!chartData || chartData.length === 0) && (
        <div className="no-data">
          <p>No historical data available for the selected time range.</p>
          <p className="hint">
            Note: Historical data collection is currently being implemented. The system stores
            real-time data points as they arrive from the polling service.
          </p>
        </div>
      )}

      {chartData && chartData.length > 0 && (
        <div className="chart-container">
          <h3>
            {metricType === MetricType.Power && 'Power Output Over Time'}
            {metricType === MetricType.Energy && 'Energy Production Over Time'}
            {metricType === MetricType.Voltage && 'Voltage Over Time'}
            {metricType === MetricType.Temperature && 'Temperature Over Time'}
          </h3>
          <ResponsiveContainer width="100%" height={400}>
            <LineChart data={chartData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="time" angle={-45} textAnchor="end" height={100} />
              <YAxis />
              <Tooltip />
              <Legend />
              <Line
                type="monotone"
                dataKey="value"
                stroke="#667eea"
                strokeWidth={2}
                name={
                  metricType === MetricType.Power
                    ? 'Power (kW)'
                    : metricType === MetricType.Energy
                      ? 'Energy (kWh)'
                      : metricType === MetricType.Voltage
                        ? 'Voltage (V)'
                        : 'Temperature (°C)'
                }
              />
            </LineChart>
          </ResponsiveContainer>
        </div>
      )}
    </div>
  );
}
