using ImGuiNET;
using StudioCore.Core.Data;
using StudioCore.Editors.TableEditor.Framework;
using StudioCore.Editors.TextEditor.Framework;
using StudioCore.Platform;
using StudioCore.UserProject;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Timers;

namespace StudioCore.Core.Project;

public static class ProjectHandler
{
    public static bool ShowProjectLoadSelection = true;
    public static bool RecentProjectLoad = false;

    public static bool FailedToLoadRecentProject = false;

    /// <summary>
    /// Run after Warbox setup, loads previous project if possible.
    /// </summary>
    public static void LoadProjectOnStart()
    {
        // Ignore this if it tried and failed, to allow user to create new project / load existing project
        if (FailedToLoadRecentProject)
            return;

        if (CFG.Current.Project_LoadPreviousProject)
        {
            try
            {
                LoadProject(CFG.Current.LastProjectFile);
            }
            catch (Exception ex)
            {
                FailedToLoadRecentProject = true;
                TaskLogs.AddLog("Failed to load recent project.");
            }
        }
    }

    /// <summary>
    /// Reset project to base state
    /// </summary>
    public static void ClearProject()
    {
        Warbox.Project = new Project();
        Warbox.ProjectChanged = true;

        TableDataHandler.Reset();
        TextDataHandler.Reset();
    }

    /// <summary>
    /// Load existing project
    /// </summary>
    public static void OpenProjectLoadDialog()
    {
        var success = PlatformUtils.Instance.OpenFileDialog("Choose the project json file", new[] { "json" }, out var projectPath);

        if (success)
        {
            if (projectPath != null)
            {
                if (projectPath.Contains("project.json"))
                {
                    LoadProject(projectPath);
                }
            }
        }
    }

    /// <summary>
    /// Display list of recently opened projects
    /// </summary>
    public static void DisplayRecentProjects()
    {
        var id = 0;

        foreach (CFG.RecentProject p in CFG.Current.RecentProjects.ToArray())
        {
            RecentProjectEntry(p, id);

            id++;
        }
    }

    /// <summary>
    /// Recently opened project display in list
    /// </summary>
    public static void RecentProjectEntry(CFG.RecentProject project, int id)
    {
        // Just remove invalid recent projects immediately
        if (!File.Exists(project.ProjectFile))
        {
            RemoveRecentProject(project);
        }

        if (ImGui.MenuItem($@"Project: {project.Name}##project{id}"))
        {
            if (File.Exists(project.ProjectFile))
            {
                var path = project.ProjectFile;

                if (LoadProject(path))
                {
                    TaskLogs.AddLog($"Loaded existing project: {project.Name}");
                }
                else
                {
                    TaskLogs.AddLog($"Failed to load existing project: {project.Name}, removed project from recent project list.");

                    RemoveRecentProject(project);
                }
            }
            else
            {
                DialogResult result = PlatformUtils.Instance.MessageBox(
                    $"Project file at \"{project.ProjectFile}\" does not exist.\n\n" +
                    $"Remove project from list of recent projects?",
                    $"Project.json cannot be found", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    RemoveRecentProject(project);
                }
            }
        }

        if (ImGui.BeginPopupContextItem())
        {
            if (ImGui.Selectable("Remove from list"))
            {
                RemoveRecentProject(project);
                CFG.Save();
            }

            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// Removes a project from the recent project list
    /// </summary>
    private static void RemoveRecentProject(CFG.RecentProject project)
    {
        if(CFG.Current.RecentProjects.Contains(project))
        {
            CFG.Current.RecentProjects.Remove(project);
        }
    }

    public static bool LoadProject(string path)
    {
        if (path == "")
        {
            PlatformUtils.Instance.MessageBox(
                $"Path parameter was empty: {path}",
                "Project Load Error", MessageBoxButtons.OK);
            return false;
        }

        Warbox.Project.Config = ReadProjectConfig(path);

        if (Warbox.Project.Config == null)
        {
            PlatformUtils.Instance.MessageBox(
                "Failed to load last project. Project will not be loaded after restart.",
                "Project Load Error", MessageBoxButtons.OK);
            return false;
        }

        Warbox.Project.Setup();
        Warbox.ProjectChanged = true;

        // Add to recent project list
        CFG.RecentProject recent = new()
        {
            Name = Warbox.Project.Config.ProjectName,
            ProjectFile = $"{Warbox.Project.ProjectDirectory}/project.json"
        };

        if (Warbox.Project.Config.ProjectName != "")
        {
            CFG.AddMostRecentProject(recent);
        }

        return true;
    }
    public static ProjectConfiguration ReadProjectConfig(string path)
    {
        var config = new ProjectConfiguration();

        if (File.Exists(path))
        {
            using (var stream = File.OpenRead(path))
            {
                config = JsonSerializer.Deserialize(stream, ProjectConfigurationSerializationContext.Default.ProjectConfiguration);
            }

            CFG.Current.LastProjectFile = path;
        }
        else
        {
            // Invalidate this if the file doesn't exist
            CFG.Current.LastProjectFile = "";
        }

        return config;
    }

    public static void WriteProjectConfig(Project targetProject)
    {
        if (targetProject == null)
            return;

        var config = targetProject.Config;
        var writePath = $"{targetProject.Config.ProjectDirectory}/project.json";

        if (writePath != "")
        {
            string jsonString = JsonSerializer.Serialize(config, typeof(ProjectConfiguration), ProjectConfigurationSerializationContext.Default);

            try
            {
                var fs = new FileStream(writePath, FileMode.Create);
                var data = Encoding.ASCII.GetBytes(jsonString);
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Dispose();
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"{ex}");
            }
        }
    }
}
