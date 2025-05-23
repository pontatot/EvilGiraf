import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from 'react-query';
import { ArrowLeft, RefreshCw, Trash2, Edit2, Power } from 'lucide-react';
import { api } from '../lib/api';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { ApplicationCreateDto, ApplicationType } from '../types/api';
import { useState } from 'react';

export function ApplicationDetails() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [isEditing, setIsEditing] = useState(false);
  const [editedApp, setEditedApp] = useState<ApplicationCreateDto>({});
  const [newVariable, setNewVariable] = useState('');

  const {
    data: application,
    isLoading: isLoadingApp,
    error: appError,
  } = useQuery(['application', id], () => api.applications.get(Number(id)));

  const {
    data: deployStatus,
    isLoading: isLoadingStatus,
    error: statusError,
  } = useQuery(
    ['deployStatus', id],
    () => api.deployments.status(Number(id)),
    { refetchInterval: 5000 }
  );

  const deployMutation = useMutation(
    () => api.deployments.deploy(Number(id)),
    {
      onSuccess: async () => {
        let attempts = 0;
        const maxAttempts = 30;
        while (attempts < maxAttempts) {
          await queryClient.invalidateQueries(['deployStatus', id]);
          const status = await api.deployments.status(Number(id));
          if (status !== null) {
            break;
          }
          attempts++;
          await new Promise(resolve => setTimeout(resolve, 2000));
        }
      },
    }
  );

  const undeployMutation = useMutation(
    () => api.deployments.undeploy(Number(id)),
    {
      onSuccess: async () => {
        let attempts = 0;
        const maxAttempts = 30;
        while (attempts < maxAttempts) {
          await queryClient.invalidateQueries(['deployStatus', id]);
          const status = await api.deployments.status(Number(id));
          if (status === null) {
            break;
          }
          attempts++;
          await new Promise(resolve => setTimeout(resolve, 2000));
        }
      },
    }
  );

  const deleteMutation = useMutation(
    () => api.applications.delete(Number(id)),
    {
      onSuccess: () => {
        queryClient.invalidateQueries(['applications']);
        navigate('/');
      },
    }
  );

  const updateMutation = useMutation(
    (data: ApplicationCreateDto) => api.applications.update(Number(id), data),
    {
      onSuccess: () => {
        queryClient.invalidateQueries(['application', id]);
        setIsEditing(false);
        setEditedApp({});
        setNewVariable('');
      },
    }
  );

  const handleDelete = () => {
    if (window.confirm('Are you sure you want to delete this application? This action cannot be undone.')) {
      deleteMutation.mutate();
    }
  };

  const handleEdit = () => {
    if (application) {
      setEditedApp({
        name: application.name,
        type: application.type,
        link: application.link,
        version: application.version,
        port: application.port === null ? -1 : application.port,
        domainName: application.domainName,
        variables: application.variables || [],
      });
      setIsEditing(true);
    }
  };

  const handleSave = () => {
    updateMutation.mutate(editedApp);
  };

  const handleCancel = () => {
    setIsEditing(false);
    setEditedApp({});
    setNewVariable('');
  };

  const handleAddVariable = () => {
    if (newVariable.trim() && newVariable.includes('=')) {
      setEditedApp({
        ...editedApp,
        variables: [...(editedApp.variables || []), newVariable.trim()]
      });
      setNewVariable('');
    }
  };

  const handleRemoveVariable = (index: number) => {
    const updatedVariables = [...(editedApp.variables || [])];
    updatedVariables.splice(index, 1);
    setEditedApp({
      ...editedApp,
      variables: updatedVariables
    });
  };

  if (isLoadingApp) return <LoadingSpinner />;
  if (appError) return <div className="text-red-500">Error: {(appError as Error).message}</div>;

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <button
          onClick={() => navigate('/')}
          className="flex items-center gap-2 text-gray-600 hover:text-gray-800"
        >
          <ArrowLeft className="h-5 w-5" />
          Back to Applications
        </button>
        <div className="flex gap-2">
          {!isEditing ? (
            <>
              <button
                onClick={handleEdit}
                className="flex items-center gap-2 text-blue-500 hover:text-blue-700"
              >
                <Edit2 className="h-5 w-5" />
                Edit
              </button>
              <button
                onClick={handleDelete}
                disabled={deleteMutation.isLoading}
                className="flex items-center gap-2 text-red-500 hover:text-red-700 disabled:opacity-50"
              >
                <Trash2 className="h-5 w-5" />
                {deleteMutation.isLoading ? 'Deleting...' : 'Delete'}
              </button>
            </>
          ) : (
            <button
              onClick={handleCancel}
              className="bg-gray-100 text-gray-700 px-4 py-2 rounded-lg flex items-center gap-2 hover:bg-gray-200 border border-gray-300"
            >
              Cancel
            </button>
          )}
        </div>
      </div>

      <div className="bg-white rounded-lg shadow p-6">
        <h1 className="text-2xl font-bold mb-6">
          {isEditing ? (
            <input
              type="text"
              value={editedApp.name || ''}
              onChange={(e) => setEditedApp({ ...editedApp, name: e.target.value })}
              className="w-full p-2 border rounded"
              placeholder="Application Name"
            />
          ) : (
            application?.name || 'Unnamed Application'
          )}
        </h1>

        <div className="grid gap-6 md:grid-cols-2">
          <div>
            <h2 className="text-lg font-semibold mb-4">Application Details</h2>
            <div className="space-y-2">
              <p>
                <span className="font-medium">ID:</span> {application?.id}
              </p>
              {isEditing ? (
                <>
                  <div>
                    <span className="font-medium">Type:</span>
                    <select
                      value={editedApp.type || application?.type}
                      onChange={(e) => setEditedApp({ ...editedApp, type: e.target.value as ApplicationType })}
                      className="ml-2 p-1 border rounded"
                    >
                      <option value={ApplicationType.Docker}>Docker</option>
                      <option value={ApplicationType.Git}>Git</option>
                    </select>
                  </div>
                  <div>
                    <span className="font-medium">Version:</span>
                    <input
                      type="text"
                      value={editedApp.version || ''}
                      onChange={(e) => setEditedApp({ ...editedApp, version: e.target.value })}
                      className="ml-2 p-1 border rounded"
                      placeholder="Version"
                    />
                  </div>
                  <div>
                    <span className="font-medium">Link:</span>
                    <input
                      type="text"
                      value={editedApp.link || ''}
                      onChange={(e) => setEditedApp({ ...editedApp, link: e.target.value })}
                      className="ml-2 p-1 border rounded"
                      placeholder="Link"
                    />
                  </div>
                  <div>
                    <span className="font-medium">Port:</span>
                    <input
                      type="number"
                      value={editedApp.port === -1 || editedApp.port === null || editedApp.port === undefined ? '' : editedApp.port.toString()}
                      onChange={(e) => {
                        const port = e.target.value ? parseInt(e.target.value) : -1;
                        setEditedApp({ ...editedApp, port });
                      }}
                      className="ml-2 p-1 border rounded"
                      placeholder="e.g. 80"
                    />
                  </div>
                  <div>
                    <span className="font-medium">Domain Name:</span>
                    <input
                      type="text"
                      value={editedApp.domainName || ''}
                      onChange={(e) => {
                        const value = e.target.value.replace(/^https?:\/\//, '');
                        setEditedApp({ ...editedApp, domainName: value });
                      }}
                      className={`ml-2 p-1 border rounded ${(!editedApp.port || editedApp.port === -1) ? 'bg-gray-100' : ''}`}
                      placeholder="e.g. app.example.com"
                      disabled={!editedApp.port || editedApp.port === -1}
                    />
                    <span className="text-sm text-gray-500 ml-2">
                      {(!editedApp.port || editedApp.port === -1) 
                        ? '(Port must be set to use domain name)'
                        : '(Enter domain without http:// or https:// that points to the server)'}
                    </span>
                  </div>
                  <div>
                    <span className="font-medium">Environment Variables:</span>
                    <div className="mt-1 flex">
                      <input
                        type="text"
                        value={newVariable}
                        onChange={(e) => setNewVariable(e.target.value)}
                        placeholder="KEY=VALUE"
                        className="ml-2 p-1 border rounded-l w-full"
                      />
                      <button
                        onClick={handleAddVariable}
                        className="bg-blue-500 text-white px-3 py-1 rounded-r hover:bg-blue-600"
                      >
                        Add
                      </button>
                    </div>
                    <p className="text-sm text-gray-500 ml-2 mt-1">
                      Format: KEY=VALUE (e.g., DATABASE_URL=postgres://user:pass@host:5432/db)
                    </p>
                    {editedApp.variables && editedApp.variables.length > 0 && (
                      <div className="mt-2 space-y-1 ml-2">
                        {editedApp.variables.map((variable, index) => (
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
                </>
              ) :
                <>
                  <p>
                    <span className="font-medium">Type:</span>{' '}
                    {application?.type.toString()}
                  </p>
                  <p>
                    <span className="font-medium">Version:</span> {application?.version || 'N/A'}
                  </p>
                  {application?.link && (
                    <p>
                      <span className="font-medium">Link:</span>{' '}
                      {application.link}
                    </p>
                  )}
                  <p>
                    <span className="font-medium">Port:</span>{' '}
                    {application?.port !== null && application?.port !== undefined && application?.port !== -1
                      ? application.port
                      : 'No port exposed'}
                  </p>
                  <p>
                    <span className="font-medium">Domain Name:</span>{' '}
                    {application?.domainName ? (
                      <a
                        href={`https://${application.domainName}`}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="text-blue-500 hover:text-blue-700 hover:underline"
                      >
                        {application.domainName}
                      </a>
                    ) : (
                      'Not set'
                    )}
                    {!application?.domainName && (
                      <span className="text-sm text-gray-500 ml-2">
                        {(!application?.port || application?.port === -1)
                          ? '(Port must be set to use domain name)'
                          : '(Enter domain without http:// or https:// that points to the server)'}
                      </span>
                    )}
                  </p>
                  {application?.variables && application.variables.length > 0 && (
                    <div>
                      <span className="font-medium">Environment Variables:</span>
                      <div className="mt-2 space-y-1">
                        {application.variables.map((variable, index) => (
                          <div key={index} className="bg-gray-50 p-2 rounded">
                            <span className="text-sm font-mono">{variable}</span>
                          </div>
                        ))}
                      </div>
                    </div>
                  )}
                </>
              }
            </div>
          </div>

          <div>
            <h2 className="text-lg font-semibold mb-4">Deployment Status</h2>
            {isLoadingStatus ? (
              <LoadingSpinner />
            ) : statusError ? (
              <div className="text-red-500">Error loading status: {(statusError as Error).message}</div>
            ) : !deployStatus ? (
              <div className="text-gray-600">
                <p>This application is not currently deployed.</p>
                <button
                  onClick={() => deployMutation.mutate()}
                  disabled={deployMutation.isLoading}
                  className="mt-4 bg-blue-500 text-white px-4 py-2 rounded-lg flex items-center gap-2 hover:bg-blue-600 disabled:opacity-50"
                >
                  <RefreshCw className={`h-5 w-5 ${deployMutation.isLoading ? 'animate-spin' : ''}`} />
                  {deployMutation.isLoading ? 'Deploying...' : 'Deploy'}
                </button>
              </div>
            ) : (
              <div className="space-y-2">
                <p>
                  <span className="font-medium">Available Replicas:</span>{' '}
                  {deployStatus.status.availableReplicas || 0}
                </p>
                <p>
                  <span className="font-medium">Ready Replicas:</span>{' '}
                  {deployStatus.status.readyReplicas || 0}
                </p>
                <p>
                  <span className="font-medium">Total Replicas:</span>{' '}
                  {deployStatus.status.replicas || 0}
                </p>
                {deployStatus.status.conditions?.map((condition, index) => (
                  <div key={index} className="mt-2">
                    <p className="font-medium">{condition.type}</p>
                    <p className="text-sm text-gray-600">{condition.message}</p>
                    <p className="text-sm text-gray-500">
                      Last updated: {new Date(condition.lastUpdateTime!).toLocaleString()}
                    </p>
                  </div>
                ))}
                <div className="flex gap-2 mt-4">
                  <button
                    onClick={() => deployMutation.mutate()}
                    disabled={deployMutation.isLoading}
                    className="bg-blue-500 text-white px-4 py-2 rounded-lg flex items-center gap-2 hover:bg-blue-600 disabled:opacity-50"
                  >
                    <RefreshCw className={`h-5 w-5 ${deployMutation.isLoading ? 'animate-spin' : ''}`} />
                    {deployMutation.isLoading ? 'Deploying...' : 'Redeploy'}
                  </button>
                  <button
                    onClick={() => undeployMutation.mutate()}
                    disabled={undeployMutation.isLoading}
                    className="bg-red-500 text-white px-4 py-2 rounded-lg flex items-center gap-2 hover:bg-red-600 disabled:opacity-50"
                  >
                    <Power className={`h-5 w-5 ${undeployMutation.isLoading ? 'animate-spin' : ''}`} />
                    {undeployMutation.isLoading ? 'Undeploying...' : 'Undeploy'}
                  </button>
                </div>
              </div>
            )}
          </div>
        </div>
        {isEditing && (
          <div className="flex justify-end gap-2 mt-6">
            <button
              onClick={handleSave}
              disabled={updateMutation.isLoading}
              className="px-4 py-2 bg-blue-500 text-white rounded-md hover:bg-blue-600 disabled:opacity-50"
            >
              {updateMutation.isLoading ? 'Saving...' : 'Save'}
            </button>
          </div>
        )}
      </div>
    </div>
  );
}