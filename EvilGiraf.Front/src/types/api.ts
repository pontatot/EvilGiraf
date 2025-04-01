export interface ApplicationCreateDto {
  name?: string;
  type?: ApplicationType;
  link?: string;
  version?: string;
}

export interface ApplicationResultDto {
  id: number;
  name?: string;
  type: ApplicationType;
  link?: string;
  version?: string;
}

export enum ApplicationType {
  Docker = 'Docker',
  Git = 'Git',
}

export interface DeployResponse {
  status: V1DeploymentStatus;
}

export interface V1DeploymentStatus {
  availableReplicas?: number;
  collisionCount?: number;
  conditions?: V1DeploymentCondition[];
  observedGeneration?: number;
  readyReplicas?: number;
  replicas?: number;
  unavailableReplicas?: number;
  updatedReplicas?: number;
}

export interface V1DeploymentCondition {
  lastTransitionTime?: string;
  lastUpdateTime?: string;
  message?: string;
  reason?: string;
  status?: string;
  type?: string;
}