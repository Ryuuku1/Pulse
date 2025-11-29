import './MetricCard.css';

interface MetricCardProps {
  title: string;
  value: string | number;
  unit?: string;
  icon?: string;
  trend?: 'up' | 'down' | 'neutral';
  subtitle?: string;
  highlight?: boolean;
}

export function MetricCard({
  title,
  value,
  unit,
  icon,
  trend,
  subtitle,
  highlight = false,
}: MetricCardProps) {
  return (
    <div className={`metric-card ${highlight ? 'highlight' : ''}`}>
      <div className="metric-header">
        {icon && <span className="metric-icon">{icon}</span>}
        <div className="metric-title">{title}</div>
      </div>
      <div className="metric-value">
        <span className="value">{value}</span>
        {unit && <span className="unit">{unit}</span>}
      </div>
      {trend && (
        <div className={`metric-trend ${trend}`}>
          {trend === 'up' && '^'}
          {trend === 'down' && 'v'}
          {trend === 'neutral' && '-'}
        </div>
      )}
      {subtitle && <div className="metric-subtitle">{subtitle}</div>}
    </div>
  );
}
