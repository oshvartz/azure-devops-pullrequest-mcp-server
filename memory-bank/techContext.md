# Technical Context: Azure DevOps PR API Tester

## Technology Stack
- **Framework**: .NET 8.0
- **Language**: C#
- **Project Type**: Console Application
- **External APIs**: Azure DevOps REST API

## Development Setup
1. **Required Tools**
   - Visual Studio 2022 or VS Code
   - .NET 8.0 SDK
   - Git for version control

2. **Project Configuration**
   - Target Framework: net8.0
   - Output Type: Executable
   - Platform: Cross-platform

## Dependencies
From AzureDevopsPRApiTester.csproj:
- System.Data.SqlClient (for database connectivity)
- Runtime dependencies for different platforms (win-x64, win-x86, win-arm64, unix)

## Project Structure
```
AzureDevopsPRApiTester/
├── Models/
│   └── PullRequestDetails.cs
├── Services/
│   └── AzureDevOpsClient.cs
├── Utils/
│   └── UrlParser.cs
├── Program.cs
└── AzureDevopsPRApiTester.csproj
```

## Technical Constraints
1. **API Limitations**
   - Azure DevOps API rate limits
   - Authentication requirements
   - API versioning considerations

2. **Runtime Requirements**
   - .NET 8.0 runtime
   - Internet connectivity for API access
   - Appropriate authentication credentials

## Development Patterns
1. **Code Organization**
   - Services folder for business logic
   - Models folder for data structures
   - Utils folder for helper functions

2. **Coding Standards**
   - C# naming conventions
   - Clear separation of concerns
   - Strong typing
   - Exception handling

3. **Build and Runtime**
   - Debug configuration available
   - Multiple platform support
   - Native runtime dependencies included
