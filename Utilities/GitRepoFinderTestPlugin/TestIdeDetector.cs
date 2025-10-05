using GitRepoFinder.Plugin.Interface.IdeDetection;

namespace GitRepoFinderTestPlugin;

public class TestIdeDetector : IIdeDetector
{
    public string GetArguments(IdeDetectorArguments args)
    {
        throw new NotImplementedException();
    }

    public string GetCommand(IdeDetectorArguments args)
    {
        throw new NotImplementedException();
    }

    public string GetDescription()
    {
        return  "Testing Plugin Loading Succeded.";
    }

    public bool IsInstalled()
    {
        #if DEBUG
                return true;
        #else
                return false;
        #endif
    }
}
