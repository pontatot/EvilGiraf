import { ApplicationCreateDto, ApplicationResultDto, DeployResponse } from '../types/api';

const API_URL = import.meta.env.DEV
  ? '/api'
  : import.meta.env.VITE_API_URL;
const API_KEY = import.meta.env.VITE_API_KEY;

const headers = {
  'Content-Type': 'application/json',
  'X-API-Key': API_KEY,
};

export class ApiError extends Error {
  constructor(public status: number, message: string) {
    super(message);
  }
}

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const error = await response.text();
    throw new ApiError(response.status, error);
  }
  return response.json();
}

export const api = {
  applications: {
    list: async (): Promise<ApplicationResultDto[]> => {
      const response = await fetch(`${API_URL}/application`, { headers });
      return handleResponse(response);
    },
    
    get: async (id: number): Promise<ApplicationResultDto> => {
      const response = await fetch(`${API_URL}/application/${id}`, { headers });
      return handleResponse(response);
    },
    
    create: async (data: ApplicationCreateDto): Promise<ApplicationResultDto> => {
      const response = await fetch(`${API_URL}/application`, {
        method: 'POST',
        headers,
        body: JSON.stringify(data),
      });
      return handleResponse(response);
    },
    
    update: async (id: number, data: ApplicationCreateDto): Promise<ApplicationResultDto> => {
      const response = await fetch(`${API_URL}/application/${id}`, {
        method: 'PATCH',
        headers,
        body: JSON.stringify(data),
      });
      return handleResponse(response);
    },
    
    delete: async (id: number): Promise<void> => {
      const response = await fetch(`${API_URL}/application/${id}`, {
        method: 'DELETE',
        headers,
      });
      if (!response.ok) {
        throw new ApiError(response.status, await response.text());
      }
    },
  },
  
  deployments: {
    deploy: async (id: number): Promise<void> => {
      const response = await fetch(`${API_URL}/deploy/${id}`, {
        method: 'POST',
        headers,
      });
      if (!response.ok) {
        throw new ApiError(response.status, await response.text());
      }
    },
    
    status: async (id: number): Promise<DeployResponse> => {
      const response = await fetch(`${API_URL}/deploy/${id}`, { headers });
      return handleResponse(response);
    },
  },
};