using ImGuiNET;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Numerics;

namespace StudioCore.Core.Project;

public class ProjectModal
{
    public Project newProject;

    public string newProjectDirectory = "";

    public ProjectModal()
    {
        newProject = new Project();

        newProjectDirectory = "";
    }

    public void Display()
    {
        ImGui.BeginTabBar("ProjectModelTabs");

        if (ImGui.BeginTabItem("Create Project"))
        {
            DisplayNewProjectCreation();

            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("Load Project"))
        {
            DisplayProjectLoadOptions();

            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();
    }

    public void DisplayProjectLoadOptions()
    {
        var scale = Warbox.GetUIScale();
        var width = ImGui.GetWindowWidth();
        var buttonSize = new Vector2(400 * 0.5f, 24) * Warbox.GetUIScale();

        if (CFG.Current.RecentProjects.Count > 0)
        {
            ImGui.Separator();
            UIHelper.WrappedText("Recent Projects");
            ImGui.Separator();

            Warbox.ProjectHandler.DisplayRecentProjects();

            ImGui.Separator();
        }

        if (ImGui.Button("Load New Project", buttonSize))
        {
            Warbox.ProjectHandler.OpenProjectDialog();
        }
        ImGui.SameLine();
        if (CFG.Current.LastProjectFile != "")
        {
            if (ImGui.Button("Load Recent Project", buttonSize))
            {
                Warbox.ProjectHandler.LoadRecentProject();
            }
        }
    }

    public void DisplayNewProjectCreation()
    {
        var width = ImGui.GetWindowWidth();
        var buttonSize = new Vector2(400, 24) * Warbox.GetUIScale();

        // Project Name
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Project Name:      ");
        UIHelper.ShowHoverTooltip("The name of this project. Used when generating the mod.manifest and patched table files.");
        ImGui.SameLine();

        var pname = newProject.Config != null ? newProject.Config.ProjectName : "Blank";

        if (ImGui.InputText("##pname", ref pname, 255))
        {
            newProject.Config.ProjectName = pname;
        }

        // Project Directory
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Project Directory: ");
        UIHelper.ShowHoverTooltip("The directory that contains the data for this project.");
        ImGui.SameLine();
        ImGui.InputText("##pdir", ref newProjectDirectory, 255);
        ImGui.SameLine();
        if (ImGui.Button($@"{ForkAwesome.FileO}"))
        {
            if (PlatformUtils.Instance.OpenFolderDialog("Select project directory...", out var path))
            {
                newProjectDirectory = path;
            }
        }

        // Data Directory
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Data Directory:    ");
        UIHelper.ShowHoverTooltip("The directory that contains the game data.");
        ImGui.SameLine();

        var gname = newProject.Config != null ? newProject.Config.GameRoot : "";
        if (ImGui.InputText("##dataDirectoryInput", ref gname, 255))
        {
            newProject.Config.GameRoot = gname;
        }

        ImGui.SameLine();

        if (ImGui.Button($@"{ForkAwesome.FileO}##dataDirectorySelect"))
        {
            if (PlatformUtils.Instance.OpenFolderDialog(
                    "Select game directory...",
                    out var path))
            {
                newProject.Config.GameRoot = Path.GetDirectoryName(path);
            }
        }

        ImGui.Separator();

        // Create
        if (ImGui.Button("Create", buttonSize))
        {
            newProject.ProjectJsonPath = $@"{newProjectDirectory}\project.json";

            bool validProject = CanCreateNewProject();

            if (validProject)
            {
                Warbox.ProjectHandler.WriteProjectConfig(newProject);

                Warbox.ProjectHandler.CurrentProject = newProject;

                // Only proceed if load is successful
                if (Warbox.ProjectHandler.LoadProject(newProject.ProjectJsonPath))
                    Warbox.ProjectHandler.IsInitialLoad = false;
            }
        }
    }

    public bool CanCreateNewProject()
    {
        var validated = true;

        if (newProject.Config.GameRoot == null ||
            !Directory.Exists(newProject.Config.GameRoot))
        {
            PlatformUtils.Instance.MessageBox(
                "Your game directory path does not exist. Please select a valid directory.", "Error",
                MessageBoxButtons.OK);
            validated = false;
        }


        if (validated && (newProjectDirectory == null || !Directory.Exists(newProjectDirectory)))
        {
            PlatformUtils.Instance.MessageBox("Your selected project directory is not valid.", "Error",
                MessageBoxButtons.OK);
            validated = false;
        }

        if (validated && File.Exists($@"{newProjectDirectory}\project.json"))
        {
            DialogResult message = PlatformUtils.Instance.MessageBox(
                "Your selected project directory already contains a project.json. Would you like to replace it?",
                "Error",
                MessageBoxButtons.YesNo);
            if (message == DialogResult.No)
            {
                validated = false;
            }
        }

        if (validated && (newProject.Config.ProjectName == null || newProject.Config.ProjectName == ""))
        {
            PlatformUtils.Instance.MessageBox("You must specify a project name.", "Error",
                MessageBoxButtons.OK);
            validated = false;
        }

        return validated;
    }
}
