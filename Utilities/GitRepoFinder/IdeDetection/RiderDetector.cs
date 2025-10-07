namespace GitRepoFinder.IdeDetection;

using GitRepoFinder.Platform;
using GitRepoFinder.Plugin.Interface.IdeDetection;

public class RiderDetection : IIdeDetector
{
    private const string RiderAppPath = "/Applications/Rider.app";

    public string GetDescription()
    {
        return "Open in Rider";
    }

    public string GetCommand(IdeDetectorArguments args)
    {
        if (OperatingSystem.IsMacOS())
        {
            return $"open -na \"{RiderAppPath}\"";
        }
        return "rider";
    }

    public string GetArguments(IdeDetectorArguments args)
    {
        if (OperatingSystem.IsMacOS())
        {
            return $" --args \"{args.folderPath}\"";
        }
        return $" \"{args.folderPath}\"";
    }


    public bool IsInstalled()
    {
        if (OperatingSystem.IsWindows())
        {

        }
        else if (OperatingSystem.IsMacOS())
        {
            return Directory.Exists(RiderAppPath);
        }
        else if (OperatingSystem.IsLinux())
        {

        }
        return false;
    }
}