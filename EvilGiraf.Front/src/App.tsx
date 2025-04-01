import { useState, useEffect } from 'react';
import { QueryClient, QueryClientProvider } from 'react-query';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { ErrorBoundary } from './components/ErrorBoundary';
import { Applications } from './pages/Applications';
import { ApplicationDetails } from './pages/ApplicationDetails';
import { ApiKeyInput } from './components/ApiKeyInput';
import Cookies from 'js-cookie';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

function App() {
  const [apiKey, setApiKey] = useState<string | null>(null);

  useEffect(() => {
    const savedApiKey = Cookies.get('apiKey');
    if (savedApiKey) {
      setApiKey(savedApiKey);
    }
  }, []);

  if (!apiKey) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <ApiKeyInput onApiKeySet={setApiKey} />
      </div>
    );
  }
  
  return (
    <ErrorBoundary>
      <QueryClientProvider client={queryClient}>
        <Router>
          <div className="min-h-screen bg-gray-50">
            <div className="container mx-auto px-4 py-8">
              <Routes>
                <Route path="/" element={<Applications />} />
                <Route path="/applications/:id" element={<ApplicationDetails />} />
              </Routes>
            </div>
          </div>
        </Router>
      </QueryClientProvider>
    </ErrorBoundary>
  );
}

export default App;