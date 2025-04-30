# Azure DevOps Pull Request MCP Server

A Model Context Protocol (MCP) server implementation focused on Azure DevOps pull request interactions. This server provides tools for accessing and manipulating Azure DevOps pull requests, built using the ModelContextProtocol NuGet package.

## Overview

This MCP server is designed as a learning project to demonstrate integration with Azure DevOps pull request APIs through the Model Context Protocol. It provides tools for:

- Retrieving pull request details
- Fetching pull request threads/comments
- Creating new comment threads on specific code locations

## Requirements

- .NET 8.0 SDK
- Azure DevOps Personal Access Token (PAT) with the following scopes:
  - `Code (Read & Write)`
  - `Pull Request Threads (Read & Write)`

## Building and Installing the MCP Server

1. Clone this repository
2. Build the project:
```bash
dotnet build -c Release
```

3. Add the MCP server to your VSCode settings by modifying:
`c:\Users\ofshvart\AppData\Roaming\Code\User\globalStorage\saoudrizwan.claude-dev\settings\cline_mcp_settings.json`

Add the following configuration to the `mcpServers` object:

```json
{
  "mcpServers": {
    "azure-devops-pr": {
      "command": "dotnet",
      "args": ["path/to/bin/Release/net8.0/AzureDevopsPullrequestMcpServer.dll"],
      "env": {
        "AZURE_DEVOPS_PAT": "your-pat-token"
      },
      "disabled": false,
      "autoApprove": []
    }
  }
}
```

Replace:
- `path/to` with the actual path to your built DLL
- `your-pat-token` with your Azure DevOps Personal Access Token

The server will appear in the Connected MCP Servers section as:
```
azure-devops-pr (dotnet path/to/AzureDevopsPullrequestMcpServer.dll)
```

With the following available tools:
- GetPrDetails
- GetPrThreads
- CreatePrThread

## Usage

The application accepts command-line arguments for the PAT and PR URL:

```bash
dotnet run -- --pat <your-pat-token> --pr-url <azure-devops-pr-url>
```

Example:
```bash
dotnet run -- --pat abc123... --pr-url https://dev.azure.com/org/project/_git/repo/pullrequest/123
```

## Available Tools

### GetPrDetails
Retrieves details about a specific pull request.

**Input Schema:**
```json
{
  "prUrl": "string",
  "includeStatuses": ["string"] | null,
  "excludeStatuses": ["string"] | null
}
```

### GetPrThreads
Fetches comment threads from a pull request.

**Input Schema:**
```json
{
  "prUrl": "string",
  "includeStatuses": ["string"] | null,
  "excludeStatuses": ["string"] | null
}
```

### CreatePrThread
Creates a new comment thread at a specific location in code.

**Input Schema:**
```json
{
  "prUrl": "string",
  "content": "string",
  "context": {
    "filePath": "string",
    "startLine": number | null,
    "startOffset": number | null,
    "endLine": number | null,
    "endOffset": number | null
  }
}
```

## Usage Examples

### Creating a New Thread
```csharp
var input = new AdoCreateThreadInput 
{
    PrUrl = "https://dev.azure.com/org/project/_git/repo/pullrequest/123",
    Content = "Consider using a more descriptive variable name",
    Context = new ThreadContext 
    {
        FilePath = "src/file.cs",
        StartLine = 10,
        EndLine = 10
    }
};
```

## Development

Built with:
- C#/.NET
- ModelContextProtocol NuGet package
- Azure DevOps REST APIs

## Project Structure

- `src/Models/` - Data models for pull requests and threads
  - `PullRequestDetails.cs` - PR data structure
  - `PullRequestThread.cs` - Thread and comment models
  - `AdoCreateThreadInput.cs` - Thread creation input model
- `src/Services/` 
  - `AdoPullRequestTool.cs` - Core API communication service
- `src/Utils/`
  - `IUrlParser.cs` - URL processing utilities

## Contributing

This is a learning project demonstrating MCP server implementation. Feel free to use it as a reference for building your own MCP servers or extending its functionality.

## License

MIT License
