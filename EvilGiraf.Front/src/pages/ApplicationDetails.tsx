import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from 'react-query';
import { ArrowLeft, RefreshCw, Trash2 } from 'lucide-react';
import { api } from '../lib/api';
import { LoadingSpinner } from '../components/LoadingSpinner';

export function ApplicationDetails() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

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

  const deleteMutation = useMutation(
    () => api.applications.delete(Number(id)),
    {
      onSuccess: () => {
        queryClient.invalidateQueries(['applications']);
        navigate('/');
      },
    }
  );

  const handleDelete = () => {
    if (window.confirm('Are you sure you want to delete this application? This action cannot be undone.')) {
      deleteMutation.mutate();
    }
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
        <button
          onClick={handleDelete}
          disabled={deleteMutation.isLoading}
          className="flex items-center gap-2 text-red-500 hover:text-red-700 disabled:opacity-50"
        >
          <Trash2 className="h-5 w-5" />
          {deleteMutation.isLoading ? 'Deleting...' : 'Delete Application'}
        </button>
      </div>

      <div className="bg-white rounded-lg shadow p-6">
        <h1 className="text-2xl font-bold mb-6">{application?.name || 'Unnamed Application'}</h1>

        <div className="grid gap-6 md:grid-cols-2">
          <div>
            <h2 className="text-lg font-semibold mb-4">Application Details</h2>
            <div className="space-y-2">
              <p>
                <span className="font-medium">ID:</span> {application?.id}
              </p>
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
              </div>
            )}

            <button
              onClick={() => deployMutation.mutate()}
              disabled={deployMutation.isLoading}
              className="mt-4 bg-blue-500 text-white px-4 py-2 rounded-lg flex items-center gap-2 hover:bg-blue-600 disabled:opacity-50"
            >
              <RefreshCw className={`h-5 w-5 ${deployMutation.isLoading ? 'animate-spin' : ''}`} />
              {deployMutation.isLoading ? 'Deploying...' : 'Deploy'}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}