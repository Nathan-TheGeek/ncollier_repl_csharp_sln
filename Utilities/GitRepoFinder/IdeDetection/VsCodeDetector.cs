namespace GitRepoFinder.IdeDetection;

using System.Security.Cryptography;
using GitRepoFinder.Platform;
using GitRepoFinder.Plugin.Interface.IdeDetection;

public class VsCodeDetection : IIdeDetector
{
    private bool pathChecked = false;
    private bool onPath;
    private static readonly string[] WINDOWS_CODE_PATHS = {
        "%AppData%\\..\\Local\\Programs\\Microsoft VS Code\\code.exe",
        "C:\\Program Files\\Microsoft VS Code\\Code.exe",
        "C:\\Program Files (x86)\\Microsoft VS Code\\Code.exe"
    };
    private static readonly string[] MACOSX_CODE_PATHS = {
        "/Applications/Visual Studio Code.app"
    };
    private static readonly string[] LINUX_CODE_PATHS = {
        "/snap/bin/code", "/usr/bin/code"
    };

    public string GetDescription()
    {
        return "Open in VSCode";
    }

    public string GetCommand(IdeDetectorArguments args)
    {
        string command = GetExePathForPlatform();
        if (OperatingSystem.IsMacOS() && !onPath)
        {
            return $"open -na \"{command}\"";
        }
        else if (OperatingSystem.IsWindows())
        {
            command = command.Contains(" ") ? $"\"{command}\"" : command;
            return $"{command} \"{args.folderPath}s\"";
        }
        return $"{command}";
    }

    public string GetArguments(IdeDetectorArguments args)
    {
        if (OperatingSystem.IsMacOS() && !onPath)
        {
            return $" --args \"{args.folderPath}\"";
        }
        else if (OperatingSystem.IsWindows())
        {
            return ""; // args pass in the command due to how cmd /C works.s
        }
        return $" \"{args.folderPath}\"";
    }

    public bool IsInstalled()
    {
        return GetExePathForPlatform() != null;
    }

    private string? GetExePathForPlatform()
    {
        if (CheckForVSCodeCommandOnPath())
        {
            return "code";
        }
        if (OperatingSystem.IsWindows())
        {
            foreach (string temp in WINDOWS_CODE_PATHS)
            {
                if (File.Exists(temp))
                {
                    return temp;
                }
            }
        }
        else if (OperatingSystem.IsMacOS())
        {
            foreach (string temp in MACOSX_CODE_PATHS)
            {
                if (Directory.Exists(temp))
                {
                    return temp;
                }
            }
        }
        else if (OperatingSystem.IsLinux())
        {
            foreach (string temp in LINUX_CODE_PATHS)
            {
                if (File.Exists(temp))
                {
                    return temp;
                }
            }
        }
        return null;
    }

    private bool CheckForVSCodeCommandOnPath()
    {
        if (!pathChecked)
        {
            string command = "code --version";
            ShellExecutor exe = ShellExecutor.getSingleInstance();
            ShellExecutorResult result = null;
            Task.WaitAll(new Task[]{ Task.Run(async () => {
                    result = await exe.ExecuteCommand(command, 2000);
                })
            });
            this.onPath = result?.ExitCode == 0;
            this.pathChecked = true;
        }
        return this.onPath;
    }
}