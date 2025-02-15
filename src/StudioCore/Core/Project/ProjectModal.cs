using ImGuiNET;
using Octokit;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Numerics;

namespace StudioCore.Core.Project;

public static class ProjectModal
{
    public static Project NewProject = new();

    public static string NewProjectDirectory = "";

    public static bool DisplayProjectCreation = false;

    public static void Display()
    {
        if (ImGui.BeginPopupModal("Project Creation##projectCreationModal", ref DisplayProjectCreation, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize))
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

            ImGui.EndPopup();
        }
    }

    public static void DisplayProjectLoadOptions()
    {
        var scale = Warbox.GetUIScale();
        var width = ImGui.GetWindowWidth();
        var buttonSize = new Vector2(400 * 0.5f, 24) * Warbox.GetUIScale();

        if (CFG.Current.RecentProjects.Count > 0)
        {
            ImGui.Separator();
            UIHelper.WrappedText("Recent Projects");
            ImGui.Separator();

            ProjectHandler.DisplayRecentProjects();

            ImGui.Separator();
        }

        if (ImGui.Button("Load New Project", buttonSize))
        {
            ProjectHandler.OpenProjectLoadDialog();
        }
        ImGui.SameLine();
        if (CFG.Current.LastProjectFile != "")
        {
            if (ImGui.Button("Load Recent Project", buttonSize))
            {
                ProjectHandler.LoadProject(CFG.Current.LastProjectFile);
            }
        }
    }

    public static void DisplayNewProjectCreation()
    {
        var width = ImGui.GetWindowWidth();
        var buttonSize = new Vector2(400, 24) * Warbox.GetUIScale();

        // Project Name
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Project Name:      ");
        UIHelper.ShowHoverTooltip("The name of this project. Used when generating the mod.manifest and patched table files.");
        ImGui.SameLine();

        var pname = NewProject.Config != null ? NewProject.Config.ProjectName : "Blank";

        if (ImGui.InputText("##pname", ref pname, 255))
        {
            NewProject.Config.ProjectName = pname;
        }

        // Project Directory
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Project Directory:");
        UIHelper.ShowHoverTooltip("The directory that contains the data for this project.");
        ImGui.SameLine();

        var projectDirectory = NewProject.Config != null ? NewProject.Config.ProjectDirectory : "";
        if (ImGui.InputText("##projectDirectoryInput", ref projectDirectory, 255))
        {
            NewProject.Config.ProjectDirectory = projectDirectory;
        }

        ImGui.SameLine();

        if (ImGui.Button($@"{ForkAwesome.FileO}"))
        {
            if (PlatformUtils.Instance.OpenFolderDialog("Select project directory...", out var path))
            {
                NewProject.Config.ProjectDirectory = path;
            }
        }

        // Data Directory
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Game Directory:");
        UIHelper.ShowHoverTooltip("The directory that contains the game data.");
        ImGui.SameLine();

        var gname = NewProject.Config != null ? NewProject.Config.GameDirectory : "";
        if (ImGui.InputText("##gameDirectoryInput", ref gname, 255))
        {
            NewProject.Config.GameDirectory = gname;
        }

        ImGui.SameLine();

        if (ImGui.Button($@"{ForkAwesome.FileO}##dataDirectorySelect"))
        {
            if (PlatformUtils.Instance.OpenFolderDialog(
                    "Select game directory...",
                    out var path))
            {
                NewProject.Config.GameDirectory = path;
            }
        }

        ImGui.Separator();

        // Create
        if (ImGui.Button("Create", buttonSize))
        {
            bool validProject = CanCreateNewProject();

            if (validProject)
            {
                ProjectHandler.WriteProjectConfig(NewProject);
                ProjectHandler.LoadProject(NewProject.ProjectDirectory);
            }
        }
    }

    public static bool CanCreateNewProject()
    {
        var validated = true;

        if (NewProject.Config.GameDirectory == null || !Directory.Exists(NewProject.Config.GameDirectory))
        {
            PlatformUtils.Instance.MessageBox(
                "Your game directory path does not exist. Please select a valid directory.", "Error",
                MessageBoxButtons.OK);
            validated = false;
        }
        if (NewProject.Config.ProjectDirectory == null || !Directory.Exists(NewProject.Config.ProjectDirectory))
        {
            PlatformUtils.Instance.MessageBox(
                "Your project directory path does not exist. Please select a valid directory.", "Error",
                MessageBoxButtons.OK);
            validated = false;
        }

        if (validated && File.Exists($@"{NewProjectDirectory}\project.json"))
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

        if (validated && (NewProject.Config.ProjectName == null || NewProject.Config.ProjectName == ""))
        {
            PlatformUtils.Instance.MessageBox("You must specify a project name.", "Error",
                MessageBoxButtons.OK);
            validated = false;
        }

        return validated;
    }
}
