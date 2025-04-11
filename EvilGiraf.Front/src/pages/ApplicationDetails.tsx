import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from 'react-query';
import { ArrowLeft, RefreshCw, Trash2, Edit2, Save, X, Power } from 'lucide-react';
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
      onSuccess: () => {
        queryClient.invalidateQueries(['deployStatus', id]);
      },
    }
  );

  const undeployMutation = useMutation(
    () => api.deployments.undeploy(Number(id)),
    {
      onSuccess: () => {
        queryClient.invalidateQueries(['deployStatus', id]);
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
        ports: application.ports,
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
            <>
              <button
                onClick={handleCancel}
                className="flex items-center gap-2 text-gray-500 hover:text-gray-700"
              >
                <X className="h-5 w-5" />
                Cancel
              </button>
              <button
                onClick={handleSave}
                disabled={updateMutation.isLoading}
                className="flex items-center gap-2 text-green-500 hover:text-green-700 disabled:opacity-50"
              >
                <Save className="h-5 w-5" />
                {updateMutation.isLoading ? 'Saving...' : 'Save'}
              </button>
            </>
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
                    <span className="font-medium">Ports:</span>
                    <input
                      type="text"
                      value={editedApp.ports?.join(', ') || ''}
                      onChange={(e) => {
                        const ports = e.target.value
                          .split(',')
                          .map(port => parseInt(port.trim()))
                          .filter(port => !isNaN(port));
                        setEditedApp({ ...editedApp, ports });
                      }}
                      className="ml-2 p-1 border rounded"
                      placeholder="e.g. 80, 443, 8080"
                    />
                  </div>
                </>
              ) : (
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
                    <span className="font-medium">Ports:</span>{' '}
                    {application?.ports && application.ports.length > 0
                      ? application.ports.join(', ')
                      : 'No ports exposed'}
                  </p>
                </>
              )}
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
      </div>
    </div>
  );
}