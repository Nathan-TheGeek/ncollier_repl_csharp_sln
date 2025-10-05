using System.Dynamic;

namespace GitRepoFinder.IdeDetection;

public interface IIdeDetector
{
    public string GetDescription();

    public string GetCommand(IdeDetectorArguments args);

    public string GetArguments(IdeDetectorArguments args);

    public bool IsInstalled();
}