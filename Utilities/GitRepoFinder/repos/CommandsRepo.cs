using System.Reflection;
using GitRepoFinder.IdeDetection;
using GitRepoFinder.models;

namespace GitRepoFinder.repos;

public static class CommandsRepo
{
    private static List<IIdeDetector> cachedIdeDetecors;

    private static List<models.FolderCommandModel> commandListCache;

    public static Task<List<models.FolderCommandModel>> getAll()
    {
        return Task.Run(() =>
        {
            var settings = repos.settingsFile.read();
            return mergeWithAutodetected(settings.commands);
        });
    }

    public static Task saveAll(IEnumerable<models.FolderCommandModel> commands)
    {
        commands = commands.Where(d => d.IsAutoDetected == false).ToList();
        return Task.Run(() =>
        {
            var settings = repos.settingsFile.read();

            settings.commands.Clear();
            settings.commands.AddRange(commands);

            repos.settingsFile.write(settings);
        });
    }

    public static Task Add(models.FolderCommandModel command)
    {
        return Task.Run(() =>
        {
            var settings = repos.settingsFile.read();
            settings.commands.Add(command);
            repos.settingsFile.write(settings);

        });
    }

    public static Task Remove(models.FolderCommandModel command)
    {
        return Task.Run(() =>
        {
            var settings = repos.settingsFile.read();
            settings.commands.Remove(command);
            repos.settingsFile.write(settings);

        });
    }

    public static async Task RefreshCommandListCache()
    {
        commandListCache = null;
        commandListCache = await getAll();
        RefreshCachedIdeDetectors();
    }

    private static void RefreshCachedIdeDetectors()
    {
        cachedIdeDetecors = new List<IIdeDetector>();
        var pluginTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsClass &&
                        !t.IsAbstract &&
                        typeof(IdeDetection.IIdeDetector).IsAssignableFrom(t));

        foreach (var plugin in pluginTypes)
        {
            IIdeDetector det = (IIdeDetector)Activator.CreateInstance(plugin);
            if (det != null && det.IsInstalled())
            {
                cachedIdeDetecors.Add(det);
            }
        }
    }

    public static List<models.FolderCommandModel> GetAllCached()
    {
        return commandListCache;
    }

    private static List<models.FolderCommandModel> mergeWithAutodetected(List<models.FolderCommandModel> currentList)
    {
        if (cachedIdeDetecors == null)
        {
            RefreshCachedIdeDetectors();
        }
        currentList = currentList.Where(d => d.IsAutoDetected == false).ToList();
        foreach (IIdeDetector detector in cachedIdeDetecors) {
            var model = new FolderCommandModel();
            model.Description = detector.GetDescription();
            model.IsAutoDetected = true;
            model.IdeDetector = detector;
            currentList.Add(model);
        }

        return currentList;
    }
}