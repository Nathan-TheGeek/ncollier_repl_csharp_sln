using GitRepoFinder.Plugin.Interface.IdeDetection;

namespace GitRepoFinder.models;

public class FolderCommandModel : nac.ViewModelBase.ViewModelBase
{
    public string Description
    {
        get { return GetValue(() => Description); }
        set { SetValue(() => Description, value); }
    }

    public string ExePath
    {
        get { return GetValue(() => ExePath); }
        set { SetValue(() => ExePath, value); }
    }

    public string Arguments
    {
        get { return GetValue(() => Arguments); }
        set { SetValue(() => Arguments, value); }
    }

    public bool IsAutoDetected
    {
        get { return GetValue(() => IsAutoDetected); }
        set { SetValue(() => IsAutoDetected, value); }
    }

    public IIdeDetector IdeDetector
    {
        get { return GetValue(() => IdeDetector); }
        set { SetValue(() => IdeDetector, value); }
    }
}