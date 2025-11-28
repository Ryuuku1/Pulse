import './MetricCard.css';

interface MetricCardProps {
  title: string;
  value: string | number;
  unit?: string;
  icon?: string;
  trend?: 'up' | 'down' | 'neutral';
  subtitle?: string;
}

export function MetricCard({ title, value, unit, icon, trend, subtitle }: MetricCardProps) {
  return (
    <div className="metric-card">
      <div className="metric-header">
        {icon && <span className="metric-icon">{icon}</span>}
        <h3 className="metric-title">{title}</h3>
      </div>
      <div className="metric-value">
        <span className="value">{value}</span>
        {unit && <span className="unit">{unit}</span>}
      </div>
      {trend && (
        <div className={`metric-trend ${trend}`}>
          {trend === 'up' && '↑'}
          {trend === 'down' && '↓'}
          {trend === 'neutral' && '→'}
        </div>
      )}
      {subtitle && <div className="metric-subtitle">{subtitle}</div>}
    </div>
  );
}
