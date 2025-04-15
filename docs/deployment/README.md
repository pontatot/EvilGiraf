# EvilGiraf Deployment Guide

This guide explains how to deploy EvilGiraf using Helm charts in a Kubernetes environment.

## Prerequisites

- Kubernetes cluster (1.20+)
- Helm 3.x
- kubectl configured with cluster access
- Container registry access

## Helm Chart Structure

The Helm chart is located in the `helm-chart` directory and contains the following components:

```txt
helm-chart/
├── Chart.yaml           # Chart metadata
├── values.yaml          # Default configuration values
├── templates/           # Kubernetes manifest templates
└── charts/             # Dependent charts
```

## Configuration

The deployment can be configured through the `values.yaml` file or by passing values during installation. Key configuration options include:

```yaml
# Backend API configuration
api:
  replicaCount: 1
  image:
    repository: harbor.pyxis.dopolytech.fr/main/evil-giraf
    pullPolicy: Always
    tag: "latest"
  baseImage:
    repository: harbor.pyxis.dopolytech.fr/main/evil-giraf-base
    pullPolicy: Always
    tag: "latest"
  resources:
    requests:
      cpu: 100m
      memory: 128Mi
    limits:
      cpu: 500m
      memory: 512Mi
  # update the ingress to fit your needs
  ingress:
    hosts:
      - host: evilgiraf.pyxis.dopolytech.fr
        paths:
          - path: /api
            pathType: Prefix
          - path: /swagger
            pathType: Prefix
    tls:
      - secretName: evilgiraf-tls-secret
        hosts:
          - evilgiraf.pyxis.dopolytech.fr
  auth:
    # replace this with the API Key to use
    apiKey: "***"
  # the docker registry is used to internally store built docker image from git applications
  dockerRegistry:
    url: "***"
    username: ""
    password: ""

# Frontend configuration
frontend:
  replicaCount: 1
  image:
    repository: your-registry/evilgiraf-front
    tag: latest
    pullPolicy: IfNotPresent
  resources:
    requests:
      cpu: 100m
      memory: 128Mi
    limits:
      cpu: 200m
      memory: 256Mi
  # update the ingress to fit your needs
  ingress:
    hosts:
      - host: evilgiraf.pyxis.dopolytech.fr
        paths:
          - path: /
            pathType: Prefix
    tls:
      - secretName: evilgiraf-tls-secret
        hosts:
          - evilgiraf.pyxis.dopolytech.fr

# Database configuration
# replace the postgres credentials
postgresql:
  auth:
    username: evil_giraf
    password: '***'
    database: evil_giraf
```

## Deployment Steps

1. Update charts dependencies:

   ```bash
   helm dependency update ./helm-chart
   ```

2. Install the chart:

   ```bash
   helm install evilgiraf ./helm-chart \
     --namespace evilgiraf \
     --create-namespace
   ```

3. Verify the deployment:

   ```bash
   kubectl get pods -n evilgiraf
   kubectl get services -n evilgiraf
   ```

## Upgrading

To upgrade an existing deployment:

```bash
helm upgrade evilgiraf ./helm-chart \
  --namespace evilgiraf
```

## Scaling

The application can be scaled horizontally by adjusting the `replicaCount` values:

```bash
helm upgrade evilgiraf ./helm-chart \
  --namespace evilgiraf \
  --set api.replicaCount=3 \
  --set frontend.replicaCount=3
```

## Troubleshooting

Common issues and solutions:

1. Pods not starting:

   ```bash
   kubectl describe pod <pod-name> -n evilgiraf
   kubectl logs <pod-name> -n evilgiraf
   ```

2. Database connection issues:
   - Check database pod status
   - Review database logs

3. Ingress issues:
   - Verify ingress controller is installed
   - Check ingress resource status
   - Validate host configuration

## Backup and Restore

### Database Backup

```bash
kubectl exec -n evilgiraf $(kubectl get pod -n evilgiraf -l app.kubernetes.io/name=postgresql -o jsonpath="{.items[0].metadata.name}") \
  -- pg_dump -U evilgiraf evilgiraf > backup.sql
```

### Database Restore

```bash
kubectl exec -i -n evilgiraf $(kubectl get pod -n evilgiraf -l app.kubernetes.io/name=postgresql -o jsonpath="{.items[0].metadata.name}") \
  -- psql -U evilgiraf evilgiraf < backup.sql
```

## Production Best Practices

1. Use specific image tags instead of `latest`
2. Configure resource limits
3. Enable horizontal pod autoscaling
4. Set up proper monitoring and alerting
5. Implement backup strategies
6. Use production-grade database
7. Configure proper logging
