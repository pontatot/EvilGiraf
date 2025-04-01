import { ApplicationCreateDto, ApplicationResultDto, DeployResponse } from '../types/api';
import Cookies from 'js-cookie';

const API_URL = import.meta.env.DEV
  ? '/api'
  : import.meta.env.VITE_API_URL;

function getHeaders() {
  const apiKey = Cookies.get('apiKey');
  if (!apiKey) {
    throw new Error('API key not found. Please enter your API key.');
  }
  return {
    'Content-Type': 'application/json',
    'X-API-Key': apiKey,
  };
}

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
      const response = await fetch(`${API_URL}/application`, { headers: getHeaders() });
      return handleResponse(response);
    },
    
    get: async (id: number): Promise<ApplicationResultDto> => {
      const response = await fetch(`${API_URL}/application/${id}`, { headers: getHeaders() });
      return handleResponse(response);
    },
    
    create: async (data: ApplicationCreateDto): Promise<ApplicationResultDto> => {
      const response = await fetch(`${API_URL}/application`, {
        method: 'POST',
        headers: getHeaders(),
        body: JSON.stringify(data),
      });
      return handleResponse(response);
    },
    
    update: async (id: number, data: ApplicationCreateDto): Promise<ApplicationResultDto> => {
      const response = await fetch(`${API_URL}/application/${id}`, {
        method: 'PATCH',
        headers: getHeaders(),
        body: JSON.stringify(data),
      });
      return handleResponse(response);
    },
    
    delete: async (id: number): Promise<void> => {
      const response = await fetch(`${API_URL}/application/${id}`, {
        method: 'DELETE',
        headers: getHeaders(),
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
        headers: getHeaders(),
      });
      if (!response.ok) {
        throw new ApiError(response.status, await response.text());
      }
    },
    
    status: async (id: number): Promise<DeployResponse> => {
      const response = await fetch(`${API_URL}/deploy/${id}`, { headers: getHeaders() });
      return handleResponse(response);
    },
  },
};