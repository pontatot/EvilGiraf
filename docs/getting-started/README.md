# Getting Started with EvilGiraf

This guide will help you get started with EvilGiraf, from setting up your development environment to running the application locally.

## Prerequisites

- .NET 8.0 SDK
- Node.js 18+ and npm
- Docker
- Kubernetes cluster (for deployment)
- Helm 3.x

## Project Structure

```txt
EvilGiraf/
├── EvilGiraf/              # Backend API
├── EvilGiraf.Front/        # Frontend application
├── EvilGiraf.Tests/        # Backend Unit tests
├── EvilGiraf.IntegrationTests/  # Backend Integration tests
├── helm-chart/             # Helm deployment charts
└── docs/                   # Documentation
```

## Local Development Setup

1. Clone the repository:

   ```bash
   git clone https://github.com/pontatot/EvilGiraf.git
   cd EvilGiraf
   ```

2. Start the backend:

   ```bash
   cd EvilGiraf
   dotnet restore
   dotnet run
   ```

3. Start the frontend:

   ```bash
   cd EvilGiraf.Front
   npm install
   npm run dev
   ```

4. Access the application:
   - Frontend: `http://localhost:5173`
   - API: `http://localhost:5255`
   - Swagger UI: `http://localhost:5255/swagger`

## Code Quality

The project uses several tools to maintain code quality:

- SonarQube for code analysis
- ESLint for frontend code
- .NET analyzers for backend code
- Automated tests (unit and integration)

## Next Steps

- Check out the [API Documentation](../api/README.md) to learn about available endpoints
- Read the [Deployment Guide](../deployment/README.md) for production deployment
- Explore the [Frontend Guide](../frontend/README.md) for UI features
