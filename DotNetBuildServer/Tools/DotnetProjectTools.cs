using DotNetBuildServer.Utils;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace DotNetBuildServer.Tools;

[McpServerToolType]
public class DotnetProjectTools
{
    [McpServerTool, Description("Add a NuGet package to a .NET project. Specify the project path and package name. Optionally specify a version. Supports both WSL and Windows paths.")]
    public static async Task<string> AddPackage(string projectPath, string packageName, string? version = null)
    {
        try
        {
            projectPath = PathConverter.ConvertPath(projectPath);
            var args = $"add \"{projectPath}\" package {packageName}";
            if (!string.IsNullOrEmpty(version))
            {
                args += $" --version {version}";
            }

            var projectDir = Path.GetDirectoryName(projectPath);
            return await ExecuteDotnetCommand(args, projectDir);
        }
        catch (Exception ex)
        {
            return $"Error adding package: {ex.Message}";
        }
    }

    [McpServerTool, Description("Remove a NuGet package from a .NET project. Specify the project path and package name to remove. Supports both WSL and Windows paths.")]
    public static async Task<string> RemovePackage(string projectPath, string packageName)
    {
        try
        {
            projectPath = PathConverter.ConvertPath(projectPath);
            var args = $"remove \"{projectPath}\" package {packageName}";
            var projectDir = Path.GetDirectoryName(projectPath);
            return await ExecuteDotnetCommand(args, projectDir);
        }
        catch (Exception ex)
        {
            return $"Error removing package: {ex.Message}";
        }
    }

    [McpServerTool, Description("List all NuGet packages in a .NET project. Shows package names, requested and resolved versions. Supports both WSL and Windows paths.")]
    public static async Task<string> ListPackages(string projectPath)
    {
        try
        {
            projectPath = PathConverter.ConvertPath(projectPath);
            var args = $"list \"{projectPath}\" package";
            var projectDir = Path.GetDirectoryName(projectPath);
            return await ExecuteDotnetCommand(args, projectDir);
        }
        catch (Exception ex)
        {
            return $"Error listing packages: {ex.Message}";
        }
    }

    [McpServerTool, Description("Restore NuGet packages for a .NET project. Downloads and installs all dependencies specified in the project file. Supports both WSL and Windows paths.")]
    public static async Task<string> RestorePackages(string projectPath)
    {
        try
        {
            projectPath = PathConverter.ConvertPath(projectPath);
            var args = $"restore \"{projectPath}\"";
            var projectDir = Path.GetDirectoryName(projectPath);
            return await ExecuteDotnetCommand(args, projectDir);
        }
        catch (Exception ex)
        {
            return $"Error restoring packages: {ex.Message}";
        }
    }

    [McpServerTool, Description("Create a new Entity Framework migration. Generates migration files for database schema changes. Optionally specify DbContext and output directory. Supports both WSL and Windows paths.")]
    public static async Task<string> AddMigration(string projectPath, string migrationName, string? dbContext = null, string? outputDir = null)
    {
        try
        {
            projectPath = PathConverter.ConvertPath(projectPath);
            outputDir = PathConverter.ConvertPath(outputDir);
            var projectDir = Path.GetDirectoryName(projectPath) ?? ".";
            var args = $"ef migrations add {migrationName} --project \"{projectPath}\"";
            
            if (!string.IsNullOrEmpty(dbContext))
            {
                args += $" --context {dbContext}";
            }
            
            if (!string.IsNullOrEmpty(outputDir))
            {
                args += $" --output-dir \"{outputDir}\"";
            }

            return await ExecuteDotnetCommand(args, projectDir);
        }
        catch (Exception ex)
        {
            return $"Error adding migration: {ex.Message}";
        }
    }

    [McpServerTool, Description("Remove the last Entity Framework migration. Deletes the most recent migration files. Optionally specify DbContext. Supports both WSL and Windows paths.")]
    public static async Task<string> RemoveMigration(string projectPath, string? dbContext = null)
    {
        try
        {
            projectPath = PathConverter.ConvertPath(projectPath);
            var projectDir = Path.GetDirectoryName(projectPath) ?? ".";
            var args = $"ef migrations remove --project \"{projectPath}\"";
            
            if (!string.IsNullOrEmpty(dbContext))
            {
                args += $" --context {dbContext}";
            }

            return await ExecuteDotnetCommand(args, projectDir);
        }
        catch (Exception ex)
        {
            return $"Error removing migration: {ex.Message}";
        }
    }

    [McpServerTool, Description("Update database to apply Entity Framework migrations. Can update to latest or specific migration. Optionally specify DbContext. Supports both WSL and Windows paths.")]
    public static async Task<string> UpdateDatabase(string projectPath, string? migration = null, string? dbContext = null)
    {
        try
        {
            projectPath = PathConverter.ConvertPath(projectPath);
            var projectDir = Path.GetDirectoryName(projectPath) ?? ".";
            var args = $"ef database update";
            
            if (!string.IsNullOrEmpty(migration))
            {
                args += $" {migration}";
            }
            
            args += $" --project \"{projectPath}\"";
            
            if (!string.IsNullOrEmpty(dbContext))
            {
                args += $" --context {dbContext}";
            }

            return await ExecuteDotnetCommand(args, projectDir);
        }
        catch (Exception ex)
        {
            return $"Error updating database: {ex.Message}";
        }
    }

    [McpServerTool, Description("List all Entity Framework migrations. Shows applied and pending migrations. Optionally specify DbContext. Supports both WSL and Windows paths.")]
    public static async Task<string> ListMigrations(string projectPath, string? dbContext = null)
    {
        try
        {
            projectPath = PathConverter.ConvertPath(projectPath);
            var projectDir = Path.GetDirectoryName(projectPath) ?? ".";
            var args = $"ef migrations list --project \"{projectPath}\"";
            
            if (!string.IsNullOrEmpty(dbContext))
            {
                args += $" --context {dbContext}";
            }

            return await ExecuteDotnetCommand(args, projectDir);
        }
        catch (Exception ex)
        {
            return $"Error listing migrations: {ex.Message}";
        }
    }

    [McpServerTool, Description("Create a new .NET project. Specify project type (e.g., wpf, console, classlib), name, and optional output path and framework. Supports both WSL and Windows paths.")]
    public static async Task<string> CreateProject(string projectType, string projectName, string? outputPath = null, string? framework = null)
    {
        try
        {
            outputPath = PathConverter.ConvertPath(outputPath);
            var args = $"new {projectType} --name {projectName}";
            
            if (!string.IsNullOrEmpty(outputPath))
            {
                args += $" --output \"{outputPath}\"";
            }
            
            if (!string.IsNullOrEmpty(framework))
            {
                args += $" --framework {framework}";
            }

            // Use outputPath as working directory if provided, otherwise use current directory
            var workingDir = !string.IsNullOrEmpty(outputPath) ? outputPath : null;
            return await ExecuteDotnetCommand(args, workingDir);
        }
        catch (Exception ex)
        {
            return $"Error creating project: {ex.Message}";
        }
    }

    [McpServerTool, Description("Add a project reference to another project. Creates a dependency between two projects in the same solution. Both paths support WSL and Windows formats.")]
    public static async Task<string> AddProjectReference(string projectPath, string referencedProjectPath)
    {
        try
        {
            projectPath = PathConverter.ConvertPath(projectPath);
            referencedProjectPath = PathConverter.ConvertPath(referencedProjectPath);
            var args = $"add \"{projectPath}\" reference \"{referencedProjectPath}\"";
            var projectDir = Path.GetDirectoryName(projectPath);
            return await ExecuteDotnetCommand(args, projectDir);
        }
        catch (Exception ex)
        {
            return $"Error adding project reference: {ex.Message}";
        }
    }

    [McpServerTool, Description("Remove a project reference. Removes the dependency between two projects. Both paths support WSL and Windows formats.")]
    public static async Task<string> RemoveProjectReference(string projectPath, string referencedProjectPath)
    {
        try
        {
            projectPath = PathConverter.ConvertPath(projectPath);
            referencedProjectPath = PathConverter.ConvertPath(referencedProjectPath);
            var args = $"remove \"{projectPath}\" reference \"{referencedProjectPath}\"";
            var projectDir = Path.GetDirectoryName(projectPath);
            return await ExecuteDotnetCommand(args, projectDir);
        }
        catch (Exception ex)
        {
            return $"Error removing project reference: {ex.Message}";
        }
    }

    [McpServerTool, Description("List all project references. Shows all projects referenced by the specified project. Supports both WSL and Windows paths.")]
    public static async Task<string> ListProjectReferences(string projectPath)
    {
        try
        {
            projectPath = PathConverter.ConvertPath(projectPath);
            var args = $"list \"{projectPath}\" reference";
            var projectDir = Path.GetDirectoryName(projectPath);
            return await ExecuteDotnetCommand(args, projectDir);
        }
        catch (Exception ex)
        {
            return $"Error listing project references: {ex.Message}";
        }
    }

    [McpServerTool, Description("Run unit tests in a .NET test project. Optionally filter tests, specify configuration, and collect code coverage. Supports both WSL and Windows paths.")]
    public static async Task<string> RunTests(string projectPath, string? configuration = "Debug", string? filter = null, bool collectCoverage = false)
    {
        try
        {
            projectPath = PathConverter.ConvertPath(projectPath);
            var args = $"test \"{projectPath}\" --configuration {configuration}";
            
            if (!string.IsNullOrEmpty(filter))
            {
                args += $" --filter \"{filter}\"";
            }
            
            if (collectCoverage)
            {
                args += " --collect:\"Code Coverage\"";
            }

            var projectDir = Path.GetDirectoryName(projectPath);
            return await ExecuteDotnetCommand(args, projectDir);
        }
        catch (Exception ex)
        {
            return $"Error running tests: {ex.Message}";
        }
    }

    private static async Task<string> ExecuteDotnetCommand(string arguments, string? workingDirectory = null)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = arguments,
            WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };

        using var process = new Process { StartInfo = processStartInfo };
        
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (sender, e) => { if (e.Data != null) outputBuilder.AppendLine(e.Data); };
        process.ErrorDataReceived += (sender, e) => { if (e.Data != null) errorBuilder.AppendLine(e.Data); };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        var result = new StringBuilder();
        result.AppendLine($"Command: dotnet {arguments}");
        result.AppendLine($"Exit Code: {process.ExitCode}");
        
        var output = outputBuilder.ToString().Trim();
        var errors = errorBuilder.ToString().Trim();
        
        if (!string.IsNullOrEmpty(output))
        {
            result.AppendLine("\n--- Output ---");
            result.AppendLine(output);
        }
        
        if (!string.IsNullOrEmpty(errors))
        {
            result.AppendLine("\n--- Errors ---");
            result.AppendLine(errors);
        }

        return result.ToString();
    }
}