import { Link } from 'react-router-dom';
import { usePlants, usePlantSummary } from '../api/hooks';
import { MetricCard } from '../components/MetricCard';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { PlantStatus } from '../types/api';
import './PlantOverview.css';

function statusTone(status: PlantStatus) {
  switch (status) {
    case PlantStatus.Connected:
      return { label: 'Online', color: '#4ade80' };
    case PlantStatus.Disconnected:
    case PlantStatus.Offline:
      return { label: 'Offline', color: '#f87171' };
    case PlantStatus.Fault:
      return { label: 'Attention', color: '#fbbf24' };
    default:
      return { label: 'Unknown', color: '#cbd5e1' };
  }
}

function PlantCard({ plantId }: { plantId: string }) {
  const { data: summary, isLoading, error } = usePlantSummary(plantId);

  if (isLoading) return <LoadingSpinner message="Loading plant data..." />;
  if (error) return <div className="error">Error loading plant: {error.message}</div>;
  if (!summary) return null;

  const { plant, currentMetrics, energySummary, totalDevices, activeDevices } = summary;
  const status = statusTone(plant.status);
  const utilization =
    plant.installedCapacityKw && currentMetrics?.pvPowerKw
      ? Math.min((currentMetrics.pvPowerKw / plant.installedCapacityKw) * 100, 160)
      : null;

  return (
    <div className="plant-card">
      <div className="plant-card-top">
        <div>
          <div className="plant-title-row">
            <h2>{plant.name}</h2>
            <span className="plant-status" style={{ borderColor: status.color, color: status.color }}>
              {status.label}
            </span>
          </div>
          <p className="plant-location">{plant.address || 'Local network'}</p>
        </div>
        <div className="action-buttons">
          <Link to={`/realtime/${plantId}`} className="btn btn-primary">
            Real-time
          </Link>
          <Link to={`/historical/${plantId}`} className="btn btn-secondary">
            History
          </Link>
        </div>
      </div>

      <div className="plant-meta">
        {plant.installedCapacityKw && (
          <div className="meta-chip">
            <span>Capacity</span>
            <strong>{plant.installedCapacityKw.toFixed(1)} kW</strong>
          </div>
        )}
        <div className="meta-chip">
          <span>Devices</span>
          <strong>
            {activeDevices} / {totalDevices} active
          </strong>
        </div>
        {currentMetrics?.timestampUtc && (
          <div className="meta-chip soft">
            <span>Updated</span>
            <strong>{new Date(currentMetrics.timestampUtc).toLocaleTimeString()}</strong>
          </div>
        )}
      </div>

      {utilization !== null && (
        <div className="utilization">
          <div className="util-header">
            <span>Capacity use</span>
            <span>{utilization.toFixed(0)}%</span>
          </div>
          <div className="util-bar">
            <div className="util-fill" style={{ width: `${utilization}%` }} />
          </div>
        </div>
      )}

      <div className="metrics-grid">
        {currentMetrics?.pvPowerKw !== undefined && (
          <MetricCard
            title="Current Power"
            value={currentMetrics.pvPowerKw.toFixed(2)}
            unit="kW"
            subtitle="PV output now"
            highlight
          />
        )}
        {currentMetrics?.gridPowerKw !== undefined && (
          <MetricCard
            title={currentMetrics.gridPowerKw >= 0 ? 'Grid Export' : 'Grid Import'}
            value={Math.abs(currentMetrics.gridPowerKw).toFixed(2)}
            unit="kW"
            subtitle="Net grid flow"
            trend={currentMetrics.gridPowerKw >= 0 ? 'up' : 'down'}
          />
        )}
        {currentMetrics?.batteryPowerKw !== undefined && (
          <MetricCard
            title={currentMetrics.batteryPowerKw >= 0 ? 'Battery Charge' : 'Battery Discharge'}
            value={Math.abs(currentMetrics.batteryPowerKw).toFixed(2)}
            unit="kW"
            subtitle="Storage flow"
          />
        )}
        {currentMetrics?.stateOfChargePercent !== undefined && (
          <MetricCard
            title="Battery SoC"
            value={currentMetrics.stateOfChargePercent.toFixed(0)}
            unit="%"
            subtitle="State of charge"
          />
        )}
        {currentMetrics?.gridVoltageV !== undefined && (
          <MetricCard
            title="Grid Voltage"
            value={currentMetrics.gridVoltageV.toFixed(1)}
            unit="V"
            subtitle="At point of common coupling"
          />
        )}
        {currentMetrics?.temperatureC !== undefined && (
          <MetricCard
            title="Inverter Temp"
            value={currentMetrics.temperatureC.toFixed(1)}
            unit="C"
            subtitle="Heatsink/internal sensor"
          />
        )}
      </div>

      {energySummary && (
        <div className="energy-band">
          <div>
            <span>Today</span>
            <strong>{energySummary.energyTodayKwh.toFixed(1)} kWh</strong>
          </div>
          <div>
            <span>Month</span>
            <strong>{energySummary.energyMonthKwh.toFixed(0)} kWh</strong>
          </div>
          <div>
            <span>Year</span>
            <strong>{energySummary.energyYearKwh.toFixed(0)} kWh</strong>
          </div>
          <div>
            <span>Lifetime</span>
            <strong>{energySummary.energyTotalKwh.toFixed(0)} kWh</strong>
          </div>
        </div>
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
          No solar plants are available. Check your Modbus/Huawei configuration and ensure polling
          has synced data.
        </p>
      </div>
    );
  }

  const totalCapacity = plants.reduce(
    (acc, plant) => acc + (plant.installedCapacityKw ?? 0),
    0
  );
  const onlineCount = plants.filter((p) => p.status === PlantStatus.Connected).length;

  return (
    <div className="plant-overview">
      <div className="overview-hero">
        <div>
          <p className="eyebrow">Portfolio</p>
          <h1>All Plants</h1>
          <p className="subtitle">
            Live system health, current output, and energy performance across your fleet.
          </p>
        </div>
        <div className="hero-stats">
          <div className="hero-stat">
            <span>Plants Online</span>
            <strong>
              {onlineCount} / {plants.length}
            </strong>
          </div>
          <div className="hero-stat">
            <span>Installed Capacity</span>
            <strong>{totalCapacity.toFixed(1)} kW</strong>
          </div>
        </div>
      </div>

      <div className="plants-container">
        {plants.map((plant) => (
          <PlantCard key={plant.id} plantId={plant.id} />
        ))}
      </div>
    </div>
  );
}
