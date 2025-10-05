using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace GitRepoFinder.Platform;

public class ShellExecutor
{
    private static ShellExecutor __singleInstance;
    private string shell;
    private string args;

    /// <summary>
    /// Supports running 
    /// </summary>
    private ShellExecutor()
    {
        // Check if the current operating system is Windows
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Windows Configuration: Use cmd.exe with the /C argument
            shell = "cmd.exe";
            args = "/C \"{command}\"";
        }
        else
        {
            // macOS/Linux Configuration: Use /bin/bash with the -c argument
            shell = "/bin/bash";
            args = "-c \"{command}\"";
        }
    }

    /// <summary>
    /// Executes a shell command and captures its output and error streams.
    /// Supports Windows (via cmd.exe) and Unix-like systems (macOS/Linux via /bin/bash).
    /// [Copied and modified from a sample provided by Gemini]
    /// </summary>
    /// <param name="command">The command string to execute (e.g., "ls -la" or "dir").</param>
    /// <param name="timeout">The timeout to use in miliseconds. 10,000 (10 seconds) default. 0 no timout (not recommended). </param>
    /// <returns>ShellExecutorResult results of executing the command.</returns>
    public ShellExecutorResult ExecuteCommand(string command, int timeout = 10000)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = shell,
            Arguments = args.Replace("{command}", command),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Use a StringBuilder to capture the output efficiently
        var standardOutput = new StringBuilder();
        var standardError = new StringBuilder();

        using (var process = new Process { StartInfo = startInfo })
        {
            try
            {
                // Set up event handlers to capture output asynchronously
                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null) standardOutput.AppendLine(e.Data);
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null) standardError.AppendLine(e.Data);
                };

                // Start the process
                process.Start();

                // Begin reading the output and error streams
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // Wait for the process to exit
                if (timeout > 0 && !process.WaitForExit(timeout))
                {
                    // If the timeout is hit, kill the process
                    process.Kill();
                    standardError.AppendLine("[Error]: Command execution timed out.");
                    return new ShellExecutorResult(1, standardOutput.ToString().Trim(), standardError.ToString().Trim());
                }

                // Wait for any asynchronous output data to finish being processed
                process.WaitForExit();

                return new ShellExecutorResult(process.ExitCode, standardOutput.ToString().Trim(), standardError.ToString().Trim());
            }
            catch (Exception ex)
            {
                standardError.AppendLine($"An exception occurred: {ex.Message}");
                return new ShellExecutorResult(1, standardOutput.ToString().Trim(), standardError.ToString().Trim());
            }
        }
    }

    public static ShellExecutor getSingleInstance()
    {
        if (__singleInstance == null)
        {
            __singleInstance = new ShellExecutor();
        }
        return __singleInstance;
    }
}