import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { PlantOverview } from './features/PlantOverview';
import { RealtimeDashboard } from './features/RealtimeDashboard';
import { HistoricalView } from './features/HistoricalView';
import { Layout } from './components/Layout';
import './App.css';

// Create a client
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 2,
      refetchOnWindowFocus: false,
    },
  },
});

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<Layout />}>
            <Route index element={<Navigate to="/overview" replace />} />
            <Route path="overview" element={<PlantOverview />} />
            <Route path="realtime/:plantId" element={<RealtimeDashboard />} />
            <Route path="historical/:plantId" element={<HistoricalView />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  );
}

export default App;
