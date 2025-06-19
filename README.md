# Hikru Assessment Application

## Overview
A comprehensive recruitment management system built with .NET 8.0 and React. The application provides functionality for managing job positions, recruiters, and departments with a modern web interface and robust backend services.
This application is a part of the Hikru assessment process, it was created using .NET 8.0 and React, and it is ready to be deployed to Azure.
Built using Windsurf AI, Visual Studio Community 2022, and GitHub.

PROD AZURE URL: https://happy-stone-0deafcf10.1.azurestaticapps.net
PROD AZURE API URL: http://hikru-recruitment-api.azurewebsites.net/api/positions


Author: Davis Penaranda

## Project Structure

### Backend Components

1. **Lib.Repository**
   - Core domain models and interfaces
   - Contains entity classes: `Position`, `Recruiter`, `Department`
   - Defines the `IPositionRepository` interface for data access.
   - Contains the PositionRepository.cs file for data access.
   - Contains the Program.cs file for database connection and local initialization.

2. **SQLiteConnectivity**
   - Implements data access layer using SQLite
   - Provides database context and repository implementations
   - Handles database migrations and schema management
   - Contains the InitializeDatabase.sql file for database initialization.
   - Contains the PositionRepository.cs file for data access.
   - Contains the Program.cs file for database connection and local initialization.

3. **API.Test**
   - Unit and integration tests for the application
   - Uses xUnit testing framework
   - Includes test cases for repository and service layers
   - Dotnet clean, dotnet build and dotnet test commands.

4. **RestWebServices**
   - RESTful API endpoints for the application
   - Implements controllers for positions, recruiters, and departments
   - Handles request/response serialization and validation
   - It can be run locally using dotnet run --project RestWebServices, from the solution root directory.

5. **OracleConnectivity**
   - Optional Oracle database connectivity layer
   - Provides alternative data access implementation for Oracle databases
   - Can be used for enterprise deployments requiring Oracle.
   - INESTABLE, NOT RECOMMENDED FOR PRODUCTION.
   - It was ready for testing locally and Oracle autonomous database.
   -Multiple issues for deployment, then using SQL Lite now.

### Frontend (WebApp)
- Built with React and TypeScript
- Modern, responsive UI with Material-UI components
- Implements CRUD operations for positions
- Real-time data updates
- Form validation and error handling
- This project is integrated with Azure Static Web Apps and GitHub Actions for continuous deployment (CI/CD).
- Run locally using: dotnet clean, dotnet build, dotnet run 

## Features
- **Position Management**
  - Create, read, update, and delete job positions
  - Track position status (draft, open, closed, archived)
  - Associate positions with recruiters and departments

- **User Interface**
  - Interactive data tables with sorting and filtering
  - Form validation and error handling
  - Responsive design for desktop and mobile

- **API**
  - RESTful endpoints for all operations
  - JSON-based request/response format
  - Proper HTTP status codes and error handling

- **Testing**
  - Unit tests for business logic
  - Integration tests for API endpoints
  - Test coverage reporting

## Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) (v16+)
- [SQLite](https://www.sqlite.org/index.html) (for local development)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or [VS Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) (for deployment)

## Getting Started

### Local Development
1. Clone the repository:
   ```bash
   git clone https://github.com/your-username/AssessmentHikru.git
   cd AssessmentHikru

2. Set up the database:
   - SQLite database will be created automatically on first run
   - For Oracle, update connection strings in appsettings.json

3. Run the backend:
   ```bash
   cd RestWebServices
   dotnet run
   ```

4. Run the frontend:
   ```bash
   cd ../WebApp
   npm install
   npm run dev
   ```

5. Access the application at `http://localhost:5173`

## Testing
Run unit tests:
```bash
cd API.Test
dotnet test
```

## Deployment

The application is configured for deployment to Azure Web Apps. Update the deployment settings in the GitHdub Actions workflow file (`.github/workflows/deploy.yml`).

## Configuration

### Environment Variables
- `ASPNETCORE_ENVIRONMENT`: Set to `Development`, `Staging`, or `Production`
- `ConnectionStrings:DefaultConnection`: Database connection string
- `ApiSettings:BaseUrl`: Base URL for API requests from the frontend

## Contributing
1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a new Pull Request
   cd AssessmentHikru
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Build the solution:
   ```bash
   dotnet build
   ```

4. Run the application:
   ```bash
   dotnet run --project WebApp
   ```
   The application will be available at `https://localhost:5001`

### Running Tests
```bash
dotnet test
```

## Deployment

### Prerequisites for Deployment
- Azure subscription
- Azure Web App resource created
- Azure Service Principal with Contributor access

### GitHub Secrets Configuration
Add the following secrets to your GitHub repository (Settings > Secrets > Actions):
- `AZURE_CLIENT_ID`: Azure AD application client ID
- `AZURE_TENANT_ID`: Azure AD tenant ID
- `AZURE_SUBSCRIPTION_ID`: Azure subscription ID
- `AZURE_CLIENT_SECRET`: Azure AD application client secret

### Deployment Process
The application is automatically deployed to Azure Web App when changes are pushed to the `main` branch. The deployment workflow includes:
1. Building the application
2. Running tests
3. Publishing the application
4. Deploying to Azure Web App

## Project Structure
```
AssessmentHikru/
├── .github/workflows/    # GitHub Actions workflows
├── WebApp/              # Main web application
├── Lib.Repository/      # Data access layer
├── Tests/               # Unit tests
├── RestWebServices/     # RESTful API endpoints
├── OracleConnectivity/  # Optional Oracle database connectivity layer
├── SQLiteConnectivity/  # SQLite database connectivity layer
├── API.Test/            # Unit and integration tests
└── README.md            # Project documentation
```
