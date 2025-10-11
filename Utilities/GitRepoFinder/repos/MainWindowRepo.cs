using Avalonia.Input;
using GitRepoFinder.Plugin.Interface.IdeDetection;
using GitRepoFinder.models;
using nac.Forms;
using nac.Forms.model;
using GitRepoFinder.Platform;
using System.IO.Pipelines;

namespace GitRepoFinder.repos;

public static class MainWindowRepo
{
    private static nac.Forms.Form myForm;
    private static models.MainWindowModel model;

    private static PluginRepo pluginRepo;

    private static ShellExecutor shellExecutor = ShellExecutor.getSingleInstance();

    public static async Task run()
    {
        pluginRepo = PluginRepo.GetSingleInstance();

        myForm = nac.Forms.Form.NewForm();

        model = new models.MainWindowModel();
        myForm.DataContext = model;

        myForm.Title = $"Git Repo Finder v{model.Version}";

        await buildAndDisplayUI();
    }

    private static async Task buildAndDisplayUI()
    {
        
        myForm.HorizontalGroup(h =>
            {
                h.Text("Git Repo Count: ")
                    .TextFor(nameof(model.repoCount))
                    .Text(" Displayed Count: ")
                    .TextFor(nameof(model.repoDisplayCount))
                    .Button("Edit Workspaces", async () =>
                    {
                        await repos.EditWorkspacesWindowRepo.run(myForm);
                        await RefreshGitRepos();
                    }, new Style
                    {
                        contextMenuItems = new nac.Forms.model.MenuItem[]
                        {
                            new nac.Forms.model.MenuItem
                            {
                                Header = "Edit Commands",
                                Action = async () =>
                                {
                                    await repos.EditCommandsWindowRepo.run(myForm);
                                    await repos.CommandsRepo.RefreshCommandListCache();
                                    await RefreshGitRepos();
                                }
                            }
                        }
                    });
            })
        .HorizontalGroup(h =>
        {
            h.Text("Loading git repos", style: new Style{width = 100})
                .LoadingTextAnimation(style: new Style { width = 30 });
        }, style:new Style{isHiddenModelName = nameof(model.doneGitRepoLoad)})
        .VerticalGroup(v =>
        {
            v
                .HorizontalGroup(h =>
                {
                    h.Button("Refresh", async () =>
                    {
                        await RefreshGitRepos();
                    }).Button("Filter", async () =>
                    {
                        FilterRepos();
                    }).TextBoxFor(nameof(model.filterText), onKeyPress: async (key) =>
                    {
                        // filter the repos if they hit enter in the textbox
                        if (key.Key == Key.Return && key.KeyModifiers == KeyModifiers.None)
                        {
                            FilterRepos();
                        }
                    });
                }, style: new Style { height = 30 })
                .Table<models.GitRepoInfo>(nameof(model.displayedGitRepos),
                    autoGenerateColumns: false,
                    columns: new[]
                    {
                        new nac.Forms.model.Column
                        {
                            Header = "",
                            template = row =>
                            {
                                var repo = row.DataContext as models.GitRepoInfo;

                                var commandMenuItems = generateMenuItemsForAllFolderComppands(repo);

                                row.Button("...", async () => { repos.os.OpenBrowser(repo.Path); },
                                    style: new Style
                                    {
                                        width = 30,
                                        contextMenuItems = commandMenuItems
                                    });
                            }
                        },
                        new nac.Forms.model.Column
                        {
                            Header = "Name",
                            modelBindingPropertyName = nameof(models.GitRepoInfo.Name)
                        },
                        new nac.Forms.model.Column
                        {
                            Header = "Path",
                            modelBindingPropertyName = nameof(models.GitRepoInfo.Path)
                        }
                    });
        }, style: new Style{isVisibleModelName = nameof(model.doneGitRepoLoad)})
        .Display(onDisplay: async (_f) =>
        {
            await repos.CommandsRepo.RefreshCommandListCache();
            await RefreshGitRepos();
        });
    }

    private static nac.Forms.model.MenuItem[] generateMenuItemsForAllFolderComppands(GitRepoInfo repo)
    {
        var items = new List<nac.Forms.model.MenuItem>();
        var cmdList = repos.CommandsRepo.GetAllCached();

        foreach (var c in cmdList)
        {
            var item = new nac.Forms.model.MenuItem();
            
            item.Header = c.Description;
            item.Action = async() =>
            {
                runCommand(repo, command: c);
            };
            items.Add(item);
        }

        return items.ToArray();
    }

    private async static void runCommand(GitRepoInfo repo, FolderCommandModel command)
    {
        try
        {
            string cmdArgs;
            string cmdStr;

            if (!command.IsAutoDetected)
            {
                cmdStr = command.ExePath;
                cmdArgs = repos.StringFormat.OskarFormat(command.Arguments, new
                {
                    folderpath = repo.Path
                });
            }
            else
            {
                var args = new IdeDetectorArguments();
                args.folderPath = repo.Path;

                cmdStr = command.IdeDetector.GetCommand(args);
                cmdArgs = command.IdeDetector.GetArguments(args);
            }

            repos.log.Info($"Running command: EXE[{cmdStr}] Arguments[{cmdArgs}]");

            ShellExecutorResult result = await shellExecutor.ExecuteCommand(cmdStr + " " + cmdArgs);

            if (result.ExitCode > 0)
            {
                throw new Exception($"An error occured attempting to run command EXE[{cmdStr}] Arguments[{cmdArgs}]. " + result.StandardError);
            }
        }
        catch (Exception ex)
        {
            repos.ErrorWindowRepo.run(parentForm: myForm, exception: ex);
        }
    }

    private static void FilterRepos()
    {
        if (string.IsNullOrWhiteSpace(model.filterText))
        {
            model.filterText = "";
        }
        
        model.displayedGitRepos.Clear();
        foreach (var r in model.gitRepos)
        {
            if (r.Name.IndexOf(model.filterText, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                model.displayedGitRepos.Add(r);
            }
        }

        model.repoDisplayCount = model.displayedGitRepos.Count;
    }

    private static async Task RefreshGitRepos()
    {
        model.doneGitRepoLoad = false;
        try
        {
            model.gitRepos.Clear();
            model.displayedGitRepos.Clear();

            var workspaces = await repos.WorkspacesRepo.getAll();
            
            var repoList = await repos.GitRepo.refreshGitRepos(workspaces);
            // sort it
            repoList = repoList.OrderBy(i => i.Name)
                .ToList();

            model.repoCount = repoList.Count;
            model.repoDisplayCount = repoList.Count;


            foreach (var r in repoList)
            {
                model.gitRepos.Add(r);
                model.displayedGitRepos.Add(r);
            }
        }
        catch (Exception ex)
        {
        }
        finally
        {
            model.doneGitRepoLoad = true;
        }
    }
    
    
    
    
    
}