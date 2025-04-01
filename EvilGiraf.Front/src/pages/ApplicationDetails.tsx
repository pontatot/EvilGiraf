import React from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from 'react-query';
import { ArrowLeft, RefreshCw } from 'lucide-react';
import { api } from '../lib/api';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { ApplicationType } from '../types/api';

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

  if (isLoadingApp) return <LoadingSpinner />;
  if (appError) return <div className="text-red-500">Error: {(appError as Error).message}</div>;

  return (
    <div>
      <button
        onClick={() => navigate('/')}
        className="mb-6 flex items-center gap-2 text-gray-600 hover:text-gray-800"
      >
        <ArrowLeft className="h-5 w-5" />
        Back to Applications
      </button>

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
                {application?.type === ApplicationType.Type0 ? 'Type 0' : 'Type 1'}
              </p>
              <p>
                <span className="font-medium">Version:</span> {application?.version || 'N/A'}
              </p>
              {application?.link && (
                <p>
                  <span className="font-medium">Link:</span>{' '}
                  <a
                    href={application.link}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-blue-500 hover:text-blue-600"
                  >
                    {application.link}
                  </a>
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
            ) : (
              <div className="space-y-2">
                <p>
                  <span className="font-medium">Available Replicas:</span>{' '}
                  {deployStatus?.status.availableReplicas || 0}
                </p>
                <p>
                  <span className="font-medium">Ready Replicas:</span>{' '}
                  {deployStatus?.status.readyReplicas || 0}
                </p>
                <p>
                  <span className="font-medium">Total Replicas:</span>{' '}
                  {deployStatus?.status.replicas || 0}
                </p>
                {deployStatus?.status.conditions?.map((condition, index) => (
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