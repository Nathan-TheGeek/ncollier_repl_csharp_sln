namespace GitRepoFinder.IdeDetection;

using Microsoft.Win32;
using GitRepoFinder.Platform;
using GitRepoFinder.Plugin.Interface.IdeDetection;

public class RiderDetection : IIdeDetector
{
    private static string __windowsRegistryPathCache;
    private static bool __windowsRegistryChecked = false; 
    private static readonly string[] MACOSX_CODE_PATHS =
    {
        "/Applications/Rider.app"
    };
    private static readonly string[] LINUX_CODE_PATHS = {
        "/snap/bin/code", "/usr/bin/code"
    };

    public string GetDescription()
    {
        return "Open in Rider";
    }

    public string GetCommand(IdeDetectorArguments args)
    {
        var command = GetExePathForPlatform();
        if (OperatingSystem.IsMacOS())
        {
            return $"open -na \"{command}\"";
        }
        else if (OperatingSystem.IsWindows())
        {
            command = command.Contains(" ") ? $"\"{command}\"" : command;
            return $"{command} \"{args.folderPath}s\"";
        }
        return "rider";
    }

    public string GetArguments(IdeDetectorArguments args)
    {
        if (OperatingSystem.IsMacOS())
        {
            return $" --args \"{args.folderPath}\"";
        }
        else if (OperatingSystem.IsWindows())
        {
            return ""; // due to the way cmd /C works parameters are passed as part of the command.
        }
        return $" \"{args.folderPath}\"";
    }


    public bool IsInstalled()
    {
        return GetExePathForPlatform() != null;
    }

    private string WindowsGetRiderExecutablePath()
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new Exception("Windows registry search support is only applicable on windows.");
        }
        if (!RiderDetection.__windowsRegistryChecked)
        {
            RiderDetection.__windowsRegistryChecked = true;
            string[] uninstallKeys = new string[]
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };
            
            RiderDetection.__windowsRegistryPathCache = null;
            foreach (string uninstallKey in uninstallKeys)
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(uninstallKey))
                {
                    if (key != null)
                    {
                        foreach (string subkeyName in key.GetSubKeyNames())
                        {
                            using (RegistryKey subkey = key.OpenSubKey(subkeyName))
                            {
                                string displayName = subkey?.GetValue("DisplayName") as string;

                                // Check if it's JetBrains Rider
                                if (!string.IsNullOrEmpty(displayName) &&
                                    displayName.Contains("JetBrains Rider", StringComparison.OrdinalIgnoreCase))
                                {
                                    string installLocation = subkey.GetValue("InstallLocation") as string;

                                    if (!string.IsNullOrEmpty(installLocation))
                                    {
                                        // Construct the full path to the executable
                                        string exePath = Path.Combine(installLocation, "bin", "rider64.exe");

                                        if (File.Exists(exePath))
                                        {
                                            RiderDetection.__windowsRegistryPathCache =  exePath;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return RiderDetection.__windowsRegistryPathCache;
    }

    private string? GetExePathForPlatform()
    {
        if (OperatingSystem.IsWindows())
        {
            return WindowsGetRiderExecutablePath();
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
}