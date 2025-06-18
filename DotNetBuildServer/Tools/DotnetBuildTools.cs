using DotNetBuildServer.Utils;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace DotNetBuildServer.Tools;

[McpServerToolType]
public class DotnetBuildTools
{
    [McpServerTool, Description("Build a .NET project using dotnet build command. Provide the full path to a .csproj or .sln file. Supports both WSL paths (/mnt/e/...) and Windows paths (E:\\...). Set suppressWarnings=true to minimize output with --nologo -v q --property WarningLevel=0 /clp:ErrorsOnly.")]
    public static async Task<string> BuildProject(string projectPath, string configuration = "Release", string? additionalArgs = null, bool suppressWarnings = false)
    {
        Console.WriteLine($"[BuildProject] Original project path: '{projectPath}'");
        projectPath = PathConverter.ConvertPath(projectPath);
        Console.WriteLine($"[BuildProject] Converted project path: '{projectPath}'");
        
        if (suppressWarnings)
        {
            var warningSuppressionArgs = "--nologo -v q --property WarningLevel=0 /clp:ErrorsOnly";
            additionalArgs = string.IsNullOrEmpty(additionalArgs) 
                ? warningSuppressionArgs 
                : $"{additionalArgs} {warningSuppressionArgs}";
        }
        
        return await ExecuteDotnetCommand("build", projectPath, configuration, additionalArgs);
    }

    [McpServerTool, Description("Rebuild a .NET project (clean + build). Performs a full rebuild by cleaning first then building. Supports both WSL and Windows paths. Set suppressWarnings=true to minimize output with --nologo -v q --property WarningLevel=0 /clp:ErrorsOnly.")]
    public static async Task<string> RebuildProject(string projectPath, string configuration = "Release", string? additionalArgs = null, bool suppressWarnings = false)
    {
        projectPath = PathConverter.ConvertPath(projectPath);
        
        if (suppressWarnings)
        {
            var warningSuppressionArgs = "--nologo -v q --property WarningLevel=0 /clp:ErrorsOnly";
            additionalArgs = string.IsNullOrEmpty(additionalArgs) 
                ? warningSuppressionArgs 
                : $"{additionalArgs} {warningSuppressionArgs}";
        }
        
        return await ExecuteDotnetCommand("rebuild", projectPath, configuration, additionalArgs);
    }

    [McpServerTool, Description("Clean build artifacts for a .NET project. Removes all compiled output files (bin/obj directories). Supports both WSL and Windows paths.")]
    public static async Task<string> CleanProject(string projectPath, string configuration = "Release")
    {
        projectPath = PathConverter.ConvertPath(projectPath);
        return await ExecuteDotnetCommand("clean", projectPath, configuration, null);
    }

    [McpServerTool, Description("Run a .NET project using dotnet run command. Executes the project with optional command-line arguments. Supports both WSL and Windows paths.")]
    public static async Task<string> RunProject(string projectPath, string? arguments = null, string configuration = "Debug")
    {
        try
        {
            projectPath = PathConverter.ConvertPath(projectPath);
            var args = $"run --project \"{projectPath}\" --configuration {configuration}";
            if (!string.IsNullOrEmpty(arguments))
            {
                args += $" -- {arguments}";
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = args,
                WorkingDirectory = Path.GetDirectoryName(projectPath) ?? ".",
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

            // For run command, we'll wait for a reasonable time but may need to kill it
            await process.WaitForExitAsync();

            var result = new StringBuilder();
            result.AppendLine($"--- Run Result ---");
            result.AppendLine($"Exit Code: {process.ExitCode}");
            
            if (outputBuilder.Length > 0)
            {
                result.AppendLine("\n--- Output ---");
                result.AppendLine(outputBuilder.ToString().Trim());
            }
            
            if (errorBuilder.Length > 0)
            {
                result.AppendLine("\n--- Errors ---");
                result.AppendLine(errorBuilder.ToString().Trim());
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"Error running project: {ex.Message}";
        }
    }

    [McpServerTool, Description("Publish a .NET project for deployment. Creates a self-contained or framework-dependent deployment package. Supports both WSL and Windows paths.")]
    public static async Task<string> PublishProject(string projectPath, string? outputPath = null, string configuration = "Release", string? runtime = null)
    {
        projectPath = PathConverter.ConvertPath(projectPath);
        outputPath = PathConverter.ConvertPath(outputPath);
        
        var args = new StringBuilder();
        args.Append($"publish \"{projectPath}\" --configuration {configuration}");
        
        if (!string.IsNullOrEmpty(outputPath))
        {
            args.Append($" --output \"{outputPath}\"");
        }
        
        if (!string.IsNullOrEmpty(runtime))
        {
            args.Append($" --runtime {runtime}");
        }

        return await ExecuteDotnetCommandDirect(args.ToString(), projectPath);
    }

    [McpServerTool, Description("Get project information including target framework, output type, and available configurations. Reads project metadata from the .csproj file. Supports both WSL and Windows paths.")]
    public static string GetProjectInfo(string projectPath)
    {
        try
        {
            Console.WriteLine($"[GetProjectInfo] Original project path: '{projectPath}'");
            var originalPath = projectPath;
            projectPath = PathConverter.ConvertPath(projectPath);
            Console.WriteLine($"[GetProjectInfo] Converted project path: '{projectPath}'");
            Console.WriteLine($"[GetProjectInfo] File exists check: {File.Exists(projectPath)}");
            
            if (!File.Exists(projectPath))
            {
                return $"Error: Project file not found at '{originalPath}' (converted to '{projectPath}')";
            }

            var result = new StringBuilder();
            result.AppendLine($"Project: {Path.GetFileName(projectPath)}");
            result.AppendLine($"Location: {projectPath}");
            result.AppendLine();

            // Read basic project info from csproj
            var content = File.ReadAllText(projectPath);
            
            // Extract target framework
            var targetFrameworkMatch = Regex.Match(content, @"<TargetFramework>(.+?)</TargetFramework>");
            if (targetFrameworkMatch.Success)
            {
                result.AppendLine($"Target Framework: {targetFrameworkMatch.Groups[1].Value}");
            }

            var targetFrameworksMatch = Regex.Match(content, @"<TargetFrameworks>(.+?)</TargetFrameworks>");
            if (targetFrameworksMatch.Success)
            {
                result.AppendLine($"Target Frameworks: {targetFrameworksMatch.Groups[1].Value}");
            }

            // Extract output type
            var outputTypeMatch = Regex.Match(content, @"<OutputType>(.+?)</OutputType>");
            if (outputTypeMatch.Success)
            {
                result.AppendLine($"Output Type: {outputTypeMatch.Groups[1].Value}");
            }

            // Check for WPF
            if (content.Contains("<UseWPF>true</UseWPF>"))
            {
                result.AppendLine("Project Type: WPF Application");
            }

            result.AppendLine("\nCommon configurations:");
            result.AppendLine("  - Debug");
            result.AppendLine("  - Release");

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"Error getting project info: {ex.Message}";
        }
    }

    private static async Task<string> ExecuteDotnetCommand(string command, string projectPath, string configuration, string? additionalArgs)
    {
        try
        {
            string arguments = command.ToLower() switch
            {
                "build" => $"build \"{projectPath}\" --configuration {configuration}",
                "rebuild" => $"build \"{projectPath}\" --configuration {configuration} --no-incremental",
                "clean" => $"clean \"{projectPath}\" --configuration {configuration}",
                _ => throw new ArgumentException($"Unknown command: {command}")
            };

            if (!string.IsNullOrEmpty(additionalArgs))
            {
                arguments += $" {additionalArgs}";
            }

            return await ExecuteDotnetCommandDirect(arguments, projectPath);
        }
        catch (Exception ex)
        {
            return $"Error executing {command}: {ex.Message}";
        }
    }

    private static async Task<string> ExecuteDotnetCommandDirect(string arguments, string? projectPath = null)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };

        // Set working directory to project directory if projectPath is provided
        if (!string.IsNullOrEmpty(projectPath))
        {
            var projectDir = Path.GetDirectoryName(projectPath);
            if (!string.IsNullOrEmpty(projectDir))
            {
                processStartInfo.WorkingDirectory = projectDir;
            }
        }

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