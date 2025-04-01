import { useState, useEffect } from 'react';
import { QueryClient, QueryClientProvider } from 'react-query';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { ErrorBoundary } from './components/ErrorBoundary';
import { Applications } from './pages/Applications';
import { ApplicationDetails } from './pages/ApplicationDetails';
import { ApiKeyInput } from './components/ApiKeyInput';
import Cookies from 'js-cookie';
import { LogOut } from 'lucide-react';
import { ApiError } from './lib/api';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
      onError: (error: unknown) => {
        // If the error is an API error with status 401 (Unauthorized), trigger logout
        if (error instanceof ApiError && error.status === 401) {
          Cookies.remove('apiKey');
          // Set the error message before reloading
          const errorMessage = error.message || 'Invalid API key. Please try again.';
          localStorage.setItem('apiError', errorMessage);
          window.location.reload();
        }
      },
    },
  },
});

function App() {
  const [apiKey, setApiKey] = useState<string | null>(null);
  const [apiError, setApiError] = useState<string | null>(null);

  useEffect(() => {
    const savedApiKey = Cookies.get('apiKey');
    if (savedApiKey) {
      setApiKey(savedApiKey);
    }
    // Check for any stored error message
    const storedError = localStorage.getItem('apiError');
    if (storedError) {
      setApiError(storedError);
      localStorage.removeItem('apiError'); // Clear the error after displaying it
    }
  }, []);

  const handleLogout = () => {
    Cookies.remove('apiKey');
    setApiKey(null);
    setApiError(null);
  };

  if (!apiKey) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <ApiKeyInput onApiKeySet={setApiKey} error={apiError} />
      </div>
    );
  }
  
  return (
    <ErrorBoundary>
      <QueryClientProvider client={queryClient}>
        <Router>
          <div className="min-h-screen bg-gray-50">
            <nav className="bg-white shadow-sm">
              <div className="container mx-auto px-4 py-3">
                <div className="flex justify-between items-center">
                  <h1 className="text-xl font-semibold text-gray-800">EvilGiraf</h1>
                  <button
                    onClick={handleLogout}
                    className="flex items-center gap-2 text-gray-600 hover:text-gray-800"
                  >
                    <LogOut className="h-5 w-5" />
                    Logout
                  </button>
                </div>
              </div>
            </nav>
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