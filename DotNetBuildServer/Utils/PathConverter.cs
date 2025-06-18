using System.Runtime.InteropServices;

namespace DotNetBuildServer.Utils;

public static class PathConverter
{
    /// <summary>
    /// Converts WSL paths to Windows paths if needed.
    /// Handles paths like /mnt/c/... to C:\... or /mnt/e/... to E:\...
    /// </summary>
    public static string ConvertPath(string path)
    {
        Console.WriteLine($"[PathConverter] Input path: '{path}'");
        
        if (string.IsNullOrEmpty(path))
        {
            Console.WriteLine("[PathConverter] Path is null or empty, returning as-is");
            return path;
        }

        Console.WriteLine($"[PathConverter] Is Windows OS: {RuntimeInformation.IsOSPlatform(OSPlatform.Windows)}");
        Console.WriteLine($"[PathConverter] Starts with /mnt/: {path.StartsWith("/mnt/")}");
        
        // If we're on Windows and the path looks like a WSL path
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && path.StartsWith("/mnt/"))
        {
            Console.WriteLine($"[PathConverter] Path length: {path.Length}");
            if (path.Length > 6)
            {
                Console.WriteLine($"[PathConverter] Character at position 5: '{path[5]}'");
                Console.WriteLine($"[PathConverter] Character at position 6: '{path[6]}'");
            }
            
            // Handle /mnt/c/path/to/file -> C:\path\to\file
            if (path.Length > 6 && path[6] == '/')
            {
                var driveLetter = char.ToUpper(path[5]);
                var remainingPath = path.Substring(7).Replace('/', '\\');
                var convertedPath = $"{driveLetter}:\\{remainingPath}";
                Console.WriteLine($"[PathConverter] Drive letter: '{driveLetter}'");
                Console.WriteLine($"[PathConverter] Remaining path: '{remainingPath}'");
                Console.WriteLine($"[PathConverter] Converted path: '{convertedPath}'");
                return convertedPath;
            }
            else
            {
                Console.WriteLine("[PathConverter] Path doesn't match expected WSL format");
            }
        }

        // If it's already a Windows path or we're not on Windows, return as-is
        Console.WriteLine("[PathConverter] Returning path unchanged");
        return path;
    }

    /// <summary>
    /// Converts an array of paths
    /// </summary>
    public static string[] ConvertPaths(params string[] paths)
    {
        return paths.Select(ConvertPath).ToArray();
    }
}