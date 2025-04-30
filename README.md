# Azure DevOps Pull Request MCP Server

A Model Context Protocol (MCP) server implementation focused on Azure DevOps pull request interactions. This server provides tools for accessing and manipulating Azure DevOps pull requests, built using the ModelContextProtocol NuGet package.

## Overview

This MCP server is designed as a learning project to demonstrate integration with Azure DevOps pull request APIs through the Model Context Protocol. It provides tools for:

- Retrieving pull request details
- Fetching pull request threads/comments
- Creating new comment threads on specific code locations

## Project Status

**DISCLAIMER: This project is still a Work in Progress (WIP)**

### Upcoming Features
- [ ] Package server as a dotnet tool for easier installation and usage
- [ ] Enhanced thread support:
  - [ ] Ability to respond to existing threads
  - [ ] Add comments to existing threads
  - [ ] Better thread management capabilities

## Usage Examples

### PR Review Workflow
To perform a pull request review, you can use the following prompt template:

```
I want us to review together PR https://dev.azure.com/org/project/_git/repo/pullrequest/123 
First let's have high level review of the PR context and then let's drill down to changed files 
and what was changed there - please share if you have any suggestions or you notice any issues. 
Please verify you are in the local folder of the repo we are doing PR on and switch to the PR 
branch to be able to look at files.
```

This will trigger the following workflow:
1. Retrieve PR details using GetPrDetails tool
2. Analyze PR context and scope
3. Review changed files in local repo
4. Provide suggestions using CreatePrThread tool

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

### Example Review Session
```
Prompts you can use during the review:

1. Get PR Overview:
"Let's review PR https://dev.azure.com/org/project/_git/repo/pullrequest/123"

2. Focus on Specific File:
"Can you analyze the changes in src/file.cs?"

3. Add Comment:
"Please add a comment on line 45 of src/file.cs suggesting to use async/await"

4. Get Existing Comments:
"Show me all the review comments in this PR"
```

Each of these prompts will utilize the appropriate MCP tools:
- GetPrDetails for PR information
- GetPrThreads for existing comments
- CreatePrThread for adding new comments

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

3. Add the MCP server to your VSCode settings by modifying the MCP settings JSON file.

Add the following configuration to the `mcpServers` object in your configuration file:

```json
{
  "mcpServers": {
    "azure-devops-pr": {
      "autoApprove": [],
      "disabled": false,
      "timeout": 60,
      "command": "dotnet",
      "args": ["path/to/bin/Release/net8.0/AzureDevopsPullrequestMcpServer.dll"],
      "env": {
        "AZURE_DEVOPS_PAT": "your-pat-token"
      },
      "transportType": "stdio"
    }
  }
}
```

Replace:
- `path/to` with the actual path to your built DLL
- `your-pat-token` with your Azure DevOps Personal Access Token

Configuration options:
- `timeout`: Operation timeout in seconds (default: 60)
- `transportType`: Communication protocol (using "stdio")
- `disabled`: Whether the server is disabled
- `autoApprove`: List of operations that don't require explicit approval

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
