# Hikru Assessment Application

## Overview
This is a .NET 8.0 web application designed for Hikru's assessment process. The application includes a web interface and backend services built with modern .NET technologies.

## Features
- Modern .NET 8.0 Web API
- Automated CI/CD with GitHub Actions
- Azure Web App deployment ready
- Unit testing setup

## Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or [VS Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) (for deployment)

## Getting Started

### Local Development
1. Clone the repository:
   ```bash
   git clone https://github.com/your-username/AssessmentHikru.git
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
└── Tests/               # Unit tests
```

## Contributing
1. Create a new branch for your feature or bugfix
2. Make your changes
3. Run tests
4. Submit a pull request

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
