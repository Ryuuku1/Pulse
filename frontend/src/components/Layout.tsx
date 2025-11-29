import { Outlet, Link, useLocation } from 'react-router-dom';
import { usePlants } from '../api/hooks';
import { PlantStatus } from '../types/api';
import './Layout.css';

export function Layout() {
  const location = useLocation();
  const { data: plants } = usePlants();
  const firstPlantId = plants?.[0]?.id;
  const totalPlants = plants?.length ?? 0;
  const connectedPlants =
    plants?.filter((p) => p.status === PlantStatus.Connected).length ?? 0;

  return (
    <div className="layout">
      <header className="header">
        <div className="header-top">
          <div className="brand">
            <div className="brand-mark">Pulse</div>
            <div>
              <h1>Solar Monitor</h1>
              <p className="brand-subtitle">Live hybrid telemetry (API + Modbus)</p>
            </div>
          </div>
          <nav className="nav">
            <Link to="/overview" className={location.pathname === '/overview' ? 'active' : ''}>
              Overview
            </Link>
            {firstPlantId && (
              <>
                <Link
                  to={`/realtime/${firstPlantId}`}
                  className={location.pathname.includes('/realtime') ? 'active' : ''}
                >
                  Real-time
                </Link>
                <Link
                  to={`/historical/${firstPlantId}`}
                  className={location.pathname.includes('/historical') ? 'active' : ''}
                >
                  Historical
                </Link>
              </>
            )}
          </nav>
        </div>
        <div className="header-meta">
          <div className="meta-pill">
            <span>Plants</span>
            <strong>{totalPlants}</strong>
          </div>
          <div className="meta-pill">
            <span>Online</span>
            <strong>{connectedPlants}</strong>
          </div>
          <div className="meta-pill accent">
            <span>Polling</span>
            <strong>Every 1-5s</strong>
          </div>
        </div>
      </header>
      <main className="main">
        <Outlet />
      </main>
      <footer className="footer">
        <p>Pulse Solar Monitor — resilient edge polling with cloud fallback</p>
      </footer>
    </div>
  );
}
