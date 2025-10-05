namespace GitRepoFinder.IdeDetection;

using System.Runtime.InteropServices;
using GitRepoFinder.Platform;

public class VsCodeDetection : IIdeDetector
{
    public string GetDescription()
    {
        return "Open in VSCode";
    }

    public string GetCommand(IdeDetectorArguments args)
    {
        return "code";
    }

    public string GetArguments(IdeDetectorArguments args)
    {
        return $" \"{args.folderPath}\"";
    }

    public bool IsInstalled()
    {
        if (CheckForVSCodeCommand())
        {
            return true;
        }
        if (OperatingSystem.IsWindows())
        {

        }
        else if (OperatingSystem.IsMacOS())
        {
            return Directory.Exists("/Applications/Visual Studio Code.app");
        }
        else if (OperatingSystem.IsLinux())
        {

        }
        return false;
    }

    private bool CheckForVSCodeCommand()
    {
        string command = "code --version";
        ShellExecutor exe = ShellExecutor.getSingleInstance();
        ShellExecutorResult result = exe.ExecuteCommand(command, 2000);
        return result.ExitCode == 0;
    }
}