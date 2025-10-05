using System.Dynamic;

namespace GitRepoFinder.Plugin.Interface.IdeDetection;

public interface IIdeDetector
{
    public string GetDescription();

    public string GetCommand(IdeDetectorArguments args);

    public string GetArguments(IdeDetectorArguments args);

    public bool IsInstalled();
}