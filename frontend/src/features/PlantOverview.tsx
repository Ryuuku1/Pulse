import { Link } from 'react-router-dom';
import { usePlants, usePlantSummary } from '../api/hooks';
import { MetricCard } from '../components/MetricCard';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { PlantStatus } from '../types/api';
import './PlantOverview.css';

function PlantCard({ plantId }: { plantId: string }) {
  const { data: summary, isLoading, error } = usePlantSummary(plantId);

  if (isLoading) return <LoadingSpinner message="Loading plant data..." />;
  if (error) return <div className="error">Error loading plant: {error.message}</div>;
  if (!summary) return null;

  const { plant, currentMetrics, energySummary, totalDevices, activeDevices } = summary;

  const getStatusColor = (status: PlantStatus) => {
    switch (status) {
      case PlantStatus.Connected:
        return '#10b981';
      case PlantStatus.Disconnected:
      case PlantStatus.Offline:
        return '#ef4444';
      case PlantStatus.Fault:
        return '#f59e0b';
      default:
        return '#6b7280';
    }
  };

  const getStatusLabel = (status: PlantStatus) => {
    switch (status) {
      case PlantStatus.Connected:
        return 'Connected';
      case PlantStatus.Disconnected:
        return 'Disconnected';
      case PlantStatus.Offline:
        return 'Offline';
      case PlantStatus.Fault:
        return 'Fault';
      default:
        return 'Unknown';
    }
  };

  return (
    <div className="plant-card">
      <div className="plant-header">
        <h2>{plant.name}</h2>
        <span className="plant-status" style={{ background: getStatusColor(plant.status) }}>
          {getStatusLabel(plant.status)}
        </span>
      </div>

      {plant.address && <p className="plant-address">📍 {plant.address}</p>}

      <div className="plant-info">
        {plant.installedCapacityKw && (
          <div className="info-item">
            <span className="label">Installed Capacity:</span>
            <span className="value">{plant.installedCapacityKw.toFixed(2)} kW</span>
          </div>
        )}
        <div className="info-item">
          <span className="label">Devices:</span>
          <span className="value">
            {activeDevices} / {totalDevices} active
          </span>
        </div>
      </div>

      <div className="metrics-grid">
        {currentMetrics?.pvPowerKw !== undefined && (
          <MetricCard
            title="Current Power"
            value={currentMetrics.pvPowerKw.toFixed(2)}
            unit="kW"
            icon="⚡"
          />
        )}

        {energySummary && (
          <>
            <MetricCard
              title="Today"
              value={energySummary.energyTodayKwh.toFixed(1)}
              unit="kWh"
              icon="📅"
            />
            <MetricCard
              title="This Month"
              value={energySummary.energyMonthKwh.toFixed(0)}
              unit="kWh"
              icon="📊"
            />
            <MetricCard
              title="Lifetime"
              value={energySummary.energyTotalKwh.toFixed(0)}
              unit="kWh"
              icon="🏆"
            />
          </>
        )}
      </div>

      <div className="plant-actions">
        <Link to={`/realtime/${plantId}`} className="btn btn-primary">
          View Real-time
        </Link>
        <Link to={`/historical/${plantId}`} className="btn btn-secondary">
          View Historical
        </Link>
      </div>

      {plant.lastUpdateTime && (
        <p className="last-update">
          Last update: {new Date(plant.lastUpdateTime).toLocaleString()}
        </p>
      )}
    </div>
  );
}

export function PlantOverview() {
  const { data: plants, isLoading, error } = usePlants();

  if (isLoading) return <LoadingSpinner message="Loading plants..." />;
  if (error) return <div className="error">Error loading plants: {error.message}</div>;
  if (!plants || plants.length === 0) {
    return (
      <div className="no-plants">
        <h2>No Plants Found</h2>
        <p>
          No solar plants are available. Please check your Huawei API configuration and ensure the
          background polling service has synced data.
        </p>
      </div>
    );
  }

  return (
    <div className="plant-overview">
      <h1>Solar Plants Overview</h1>
      <p className="subtitle">Monitoring {plants.length} solar plant(s)</p>

      <div className="plants-container">
        {plants.map((plant) => (
          <PlantCard key={plant.id} plantId={plant.id} />
        ))}
      </div>
    </div>
  );
}
