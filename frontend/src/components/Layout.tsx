import { Outlet, Link, useLocation } from 'react-router-dom';
import { usePlants } from '../api/hooks';
import './Layout.css';

export function Layout() {
  const location = useLocation();
  const { data: plants } = usePlants();
  const firstPlantId = plants?.[0]?.id;

  return (
    <div className="layout">
      <header className="header">
        <h1>⚡ Solar Monitor</h1>
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
      </header>
      <main className="main">
        <Outlet />
      </main>
      <footer className="footer">
        <p>Solar Monitor - Powered by Huawei FusionSolar API</p>
      </footer>
    </div>
  );
}
