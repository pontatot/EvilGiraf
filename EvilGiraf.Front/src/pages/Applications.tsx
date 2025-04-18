import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from 'react-query';
import { Link } from 'react-router-dom';
import { Plus, Trash2 } from 'lucide-react';
import { api } from '../lib/api';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { ApplicationType, ApplicationCreateDto } from '../types/api';

export function Applications() {
  const queryClient = useQueryClient();
  const [isCreating, setIsCreating] = useState(false);
  const [newApp, setNewApp] = useState<ApplicationCreateDto>({
    name: '',
    link: '',
    version: '',
    port: null,
    domainName: null,
    variables: [],
  });
  const [newVariable, setNewVariable] = useState('');

  const { data: applications, isLoading, error } = useQuery(
    'applications',
    () => api.applications.list()
  );

  const createMutation = useMutation(
    (data: ApplicationCreateDto) => api.applications.create(data),
    {
      onSuccess: () => {
        queryClient.invalidateQueries('applications');
        setIsCreating(false);
        setNewApp({ name: '', link: '', version: '', port: null, domainName: null, variables: [] });
        setNewVariable('');
      },
    }
  );

  const deleteMutation = useMutation(
    (id: number) => api.applications.delete(id),
    {
      onSuccess: () => queryClient.invalidateQueries('applications'),
    }
  );

  const handleAddVariable = () => {
    if (newVariable.trim() && newVariable.includes('=')) {
      setNewApp({
        ...newApp,
        variables: [...(newApp.variables || []), newVariable.trim()]
      });
      setNewVariable('');
    }
  };

  const handleRemoveVariable = (index: number) => {
    const updatedVariables = [...(newApp.variables || [])];
    updatedVariables.splice(index, 1);
    setNewApp({
      ...newApp,
      variables: updatedVariables
    });
  };

  if (isLoading) return <LoadingSpinner />;
  if (error) return <div className="text-red-500">Error: {(error as Error).message}</div>;

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold">Applications</h1>
        {!isCreating ? (
          <button
            onClick={() => setIsCreating(true)}
            className="bg-blue-500 text-white px-4 py-2 rounded-lg flex items-center gap-2 hover:bg-blue-600"
          >
            <Plus className="h-5 w-5" />
            New Application
          </button>
        ) : (
          <button
            onClick={() => setIsCreating(false)}
            className="bg-gray-100 text-gray-700 px-4 py-2 rounded-lg flex items-center gap-2 hover:bg-gray-200 border border-gray-300"
          >
            Cancel
          </button>
        )}
      </div>

      {isCreating && (
        <div className="mb-6 bg-white p-6 rounded-lg shadow">
          <h2 className="text-xl font-semibold mb-4">Create New Application</h2>
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700">Name</label>
              <input
                type="text"
                value={newApp.name}
                onChange={(e) => setNewApp({ ...newApp, name: e.target.value })}
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">Type</label>
              <select
                value={newApp.type}
                onChange={(e) => setNewApp({ ...newApp, type: e.target.value as ApplicationType })}
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
              >
                <option value={ApplicationType.Docker}>Docker</option>
                <option value={ApplicationType.Git}>Git</option>
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">Link</label>
              <input
                type="text"
                value={newApp.link}
                onChange={(e) => setNewApp({ ...newApp, link: e.target.value })}
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">Version</label>
              <input
                type="text"
                value={newApp.version}
                onChange={(e) => setNewApp({ ...newApp, version: e.target.value })}
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">Port</label>
              <input
                type="number"
                value={(newApp.port !== null && newApp.port !== undefined) ? newApp.port : ''}
                onChange={(e) => {
                  const port = e.target.value ? parseInt(e.target.value) : null;
                  setNewApp({ ...newApp, port });
                }}
                placeholder="e.g. 80"
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">Domain Name</label>
              <input
                type="text"
                value={newApp.domainName || ''}
                onChange={(e) => {
                  const value = e.target.value.replace(/^https?:\/\//, '');
                  setNewApp({ ...newApp, domainName: value });
                }}
                placeholder="e.g. app.example.com"
                disabled={newApp.port === null || newApp.port === undefined}
                className={`mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 ${
                  !newApp.port ? 'bg-gray-100' : ''
                }`}
              />
              <p className="mt-1 text-sm text-gray-500">
                {!newApp.port
                  ? '(Port must be set to use domain name)'
                  : '(Enter domain without http:// or https:// that points to the server)'}
              </p>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">Environment Variables</label>
              <div className="mt-1 flex">
                <input
                  type="text"
                  value={newVariable}
                  onChange={(e) => setNewVariable(e.target.value)}
                  placeholder="KEY=VALUE"
                  className="block w-full rounded-l-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                />
                <button
                  onClick={handleAddVariable}
                  className="bg-blue-500 text-white px-3 py-2 rounded-r-md hover:bg-blue-600"
                >
                  Add
                </button>
              </div>
              <p className="mt-1 text-sm text-gray-500">
                Format: KEY=VALUE (e.g., DATABASE_URL=postgres://user:pass@host:5432/db)
              </p>
              {newApp.variables && newApp.variables.length > 0 && (
                <div className="mt-2 space-y-1">
                  {newApp.variables.map((variable, index) => (
                    <div key={index} className="flex items-center justify-between bg-gray-50 p-2 rounded">
                      <span className="text-sm font-mono">{variable}</span>
                      <button
                        onClick={() => handleRemoveVariable(index)}
                        className="text-red-500 hover:text-red-700"
                      >
                        <Trash2 className="h-4 w-4" />
                      </button>
                    </div>
                  ))}
                </div>
              )}
            </div>
            <div className="flex justify-end gap-2">
              <button
                onClick={() => createMutation.mutate(newApp)}
                className="px-4 py-2 bg-blue-500 text-white rounded-md hover:bg-blue-600"
                disabled={createMutation.isLoading}
              >
                {createMutation.isLoading ? 'Creating...' : 'Create'}
              </button>
            </div>
          </div>
        </div>
      )}

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {applications?.map((app) => (
          <div key={app.id} className="bg-white p-6 rounded-lg shadow">
            <div className="flex justify-between items-start">
              <h2 className="text-xl font-semibold">{app.name || 'Unnamed Application'}</h2>
              <button
                onClick={() => deleteMutation.mutate(app.id)}
                className="text-red-500 hover:text-red-600"
              >
                <Trash2 className="h-5 w-5" />
              </button>
            </div>
            <div className="mt-2 space-y-2">
              <p className="text-gray-600">Type: {app.type.toString()}</p>
              <p className="text-gray-600">Version: {app.version || 'N/A'}</p>
              {app.link && (
                <p className="text-gray-600">Link: {app.link}</p>
              )}
              {app.variables && app.variables.length > 0 && (
                <p className="text-gray-600">Environment Variables: {app.variables.length}</p>
              )}
            </div>
            <Link
              to={`/applications/${app.id}`}
              className="mt-4 block text-center bg-gray-100 text-gray-700 px-4 py-2 rounded hover:bg-gray-200"
            >
              View Details
            </Link>
          </div>
        ))}
      </div>
    </div>
  );
}