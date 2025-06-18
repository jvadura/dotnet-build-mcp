# DotNet Build Server for MCP

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=.net)](https://dotnet.microsoft.com/)
[![MCP](https://img.shields.io/badge/MCP-Compatible-blue)](https://modelcontextprotocol.io/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows-0078D4)](https://www.microsoft.com/windows)

A Model Context Protocol (MCP) server that bridges Claude Code in WSL with the Windows .NET SDK, enabling seamless .NET development across the Windows/WSL boundary.

## 🚀 Features

- **Full .NET SDK Integration**: Build, run, test, and publish .NET projects from WSL
- **WSL Path Translation**: Automatic conversion between WSL paths (`/mnt/c/...`) and Windows paths (`C:\...`)
- **Package Management**: Add, remove, and manage NuGet packages
- **Entity Framework Support**: Create and manage database migrations
- **Project Management**: Create projects, manage references, and run tests
- **Real-time Streaming**: Build outputs and logs streamed in real-time
- **Async Operations**: All long-running operations execute asynchronously

## 📋 Prerequisites

- Windows 10/11 with WSL2 installed
- .NET 9.0 SDK installed on Windows host
- Claude Code CLI installed in WSL

## 🛠️ Installation

### 1. Clone and Build

```bash
# On Windows host
git clone https://github.com/yourusername/dotnet-build-server.git
cd dotnet-build-server
dotnet build
```

### 2. Run the Server

```bash
# On Windows host
dotnet run --project DotNetBuildServer
```

The server will start listening on `http://*:5000`

### 3. Connect from WSL

After starting the server on Windows, it will display available endpoints. Switch to your WSL terminal and run:

```bash
# In WSL terminal, use one of the IPs shown by the Windows server
claude mcp add dotnet-build --transport sse http://[YOUR_WINDOWS_IP]:5000/sse

# Example:
claude mcp add dotnet-build --transport sse http://10.0.0.186:5000/sse
```

> **⚠️ Important**: 
> - The URL must end with `/sse` for the SSE transport to work correctly
> - The `claude` command only works in WSL, not in Windows
> - Use the IP address displayed by the Windows server that matches your network

## 📖 Usage

Once connected, you can use Claude Code to work with .NET projects seamlessly:

```bash
# Build a project
claude "Build my WPF application at /mnt/c/Projects/MyApp"

# Add a NuGet package
claude "Add MaterialDesignThemes package to my project"

# Create and apply migrations
claude "Create a new migration for adding user table"

# Run tests with coverage
claude "Run all tests in my solution with code coverage"
```

## 🔧 Available Tools

### Build Tools
- **BuildProject** - Build a .NET project with specified configuration
- **RebuildProject** - Clean and rebuild a project
- **CleanProject** - Clean build artifacts
- **RunProject** - Run a .NET application
- **PublishProject** - Publish for deployment
- **GetProjectInfo** - Get project details and configuration

### Project Management Tools
- **AddPackage** - Add NuGet packages
- **RemovePackage** - Remove NuGet packages
- **ListPackages** - List all packages
- **RestorePackages** - Restore NuGet packages
- **CreateProject** - Create new .NET projects
- **AddProjectReference** - Add project references
- **RemoveProjectReference** - Remove project references
- **ListProjectReferences** - List all references
- **RunTests** - Run unit tests with optional coverage

### Entity Framework Tools
- **AddMigration** - Create new EF migrations
- **RemoveMigration** - Remove the last migration
- **UpdateDatabase** - Apply migrations to database
- **ListMigrations** - List all migrations

## 🏗️ Architecture

```
┌─────────────┐         ┌──────────────────┐         ┌─────────────────┐
│   WSL/Linux │  HTTP   │  Windows Host    │         │  .NET SDK       │
│   Claude    │ ──────> │  DotNet Build    │ ──────> │  MSBuild        │
│   Code CLI  │  :5000  │  Server (MCP)    │         │  dotnet CLI     │
└─────────────┘         └──────────────────┘         └─────────────────┘
```

## 🔒 Security Considerations

> **⚠️ Warning**: This server is designed for local development only.

- No authentication or authorization
- Binds to all network interfaces
- No path traversal protection
- Full access to local file system

**Do not expose this server to public networks.**

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built for [Claude Code](https://claude.ai/code) by Anthropic
- Uses the [Model Context Protocol](https://modelcontextprotocol.io/)
- Powered by [.NET 9.0](https://dotnet.microsoft.com/)

## 📧 Support

For issues, questions, or contributions, please use the [GitHub Issues](https://github.com/yourusername/dotnet-build-server/issues) page.