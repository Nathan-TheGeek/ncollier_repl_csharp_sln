using System.Reflection;
using System.Runtime.Loader;

namespace GitRepoFinder.repos;

public class PluginRepo
{
    private const string pluginPath = "plugins/";
    private static PluginRepo __singleInstance;

    private List<Assembly> assemblies;

    private AssemblyLoadContext loadContext;

    private PluginRepo()
    {
        this.loadContext = new GitRepoFinderPluginLoadContext();
        this.assemblies = new List<Assembly>();
        if (Directory.Exists(pluginPath))
        {
            var files = Directory.GetFiles(pluginPath, "*.dll");
            foreach (var file in files)
            {
                string fullPath = Path.Combine(AppContext.BaseDirectory, file);
                try
                {
                    Assembly loadedAssembly = loadContext.LoadFromAssemblyPath(fullPath);
                    assemblies.Add(loadedAssembly);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed loading plugin assembly {fullPath}. \n" + e.ToString());
                }
            }
        }
        else
        {
            Directory.CreateDirectory(pluginPath);
        }
    }

    public Assembly[] GetAssembilies()
    {
        return this.assemblies.ToArray();
    }


    public static PluginRepo GetSingleInstance()
    {
        if (__singleInstance == null)
        {
            __singleInstance = new PluginRepo();
        }
        return __singleInstance;
    }
    
    private class GitRepoFinderPluginLoadContext : AssemblyLoadContext
    {
        public GitRepoFinderPluginLoadContext() : base(isCollectible: true) { }
    }
}