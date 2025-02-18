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

public static class ProjectCreationWindow
{
    public static Project NewProject = new();

    public static string NewProjectName = "";
    public static string NewProject_ProjectDirectory = "";
    public static string NewProject_GameDirectory = "";

    private static bool MenuOpenState = false;

    public static void ToggleMenuVisibility()
    {
        MenuOpenState = !MenuOpenState;
    }

    public static void Display()
    {
        var scale = Warbox.GetUIScale();

        if (!MenuOpenState)
            return;

        ImGui.SetNextWindowSize(new Vector2(800.0f, 600.0f) * scale, ImGuiCond.FirstUseEver);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, CFG.Current.Imgui_Moveable_MainBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, CFG.Current.Imgui_Moveable_TitleBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, CFG.Current.Imgui_Moveable_TitleBg_Active);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, CFG.Current.Imgui_Moveable_ChildBg);
        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(5.0f, 5.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f * scale);

        var flags = ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoResize;

        if (ImGui.Begin("Project Creation", ref MenuOpenState, flags))
        {
            DisplayNewProjectCreation();
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);
    }

    public static void DisplayNewProjectCreation()
    {
        var width = ImGui.GetWindowWidth();

        var flags = ImGuiTableFlags.SizingFixedFit;

        if (ImGui.BeginTable($"ProjectCreationTable", 3, flags))
        {
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed, 130);
            ImGui.TableSetupColumn("Button", ImGuiTableColumnFlags.WidthFixed, 20);
            ImGui.TableSetupColumn("Inputs", ImGuiTableColumnFlags.WidthStretch, 400);

            // Project Name
            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.AlignTextToFramePadding();

            ImGui.Text("Project Name");
            UIHelper.ShowHoverTooltip("The name of this project.\n" +
                "Used when generating the mod.manifest and patched table files.", 
                600.0f);

            ImGui.TableSetColumnIndex(1);

            ImGui.TableSetColumnIndex(2);
            ImGui.AlignTextToFramePadding();

            ImGui.SetNextItemWidth(400);
            ImGui.InputText("##newProjectName", ref NewProjectName, 255);

            // Game Directory
            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.AlignTextToFramePadding();

            ImGui.Text("Game Directory:");
            UIHelper.ShowHoverTooltip("The directory that contains the game data.\n" +
                "Example: G:\\SteamLibrary\\steamapps\\common\\KingdomComeDeliverance2",
                600.0f);

            ImGui.TableSetColumnIndex(1);

            if (ImGui.Button($@"{ForkAwesome.FileO}##gameRootDirSelect"))
            {
                if (PlatformUtils.Instance.OpenFolderDialog("Select game directory...", out var path))
                {
                    NewProject_GameDirectory = path;
                }
            }

            ImGui.TableSetColumnIndex(2);
            ImGui.AlignTextToFramePadding();

            ImGui.SetNextItemWidth(400);
            ImGui.InputText("##gameDirectoryInput", ref NewProject_GameDirectory, 255);

            // Project Directory
            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.AlignTextToFramePadding();

            ImGui.Text("Project Directory:");
            UIHelper.ShowHoverTooltip("The directory that contains the data for this project.\n" +
                "Example: G:\\SteamLibrary\\steamapps\\common\\KingdomComeDeliverance2\\Mods\\MyMod",
                600.0f);

            ImGui.TableSetColumnIndex(1);

            if (ImGui.Button($@"{ForkAwesome.FileO}##projectDirSelect"))
            {
                if (PlatformUtils.Instance.OpenFolderDialog("Select project directory...", out var path))
                {
                    NewProject_ProjectDirectory = path;
                }
            }

            ImGui.TableSetColumnIndex(2);
            ImGui.AlignTextToFramePadding();

            ImGui.SetNextItemWidth(400);
            ImGui.InputText("##projectDirectoryInput", ref NewProject_ProjectDirectory, 255);

            // Create
            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.AlignTextToFramePadding();

            ImGui.TableSetColumnIndex(1);

            ImGui.TableSetColumnIndex(2);
            ImGui.AlignTextToFramePadding();

            if (ImGui.Button("Create", new Vector2(400, 24)))
            {
                NewProject.Config.ProjectName = NewProjectName;
                NewProject.Config.GameDirectory = NewProject_GameDirectory;
                NewProject.Config.ProjectDirectory = NewProject_ProjectDirectory;

                bool validProject = CanCreateNewProject();

                if (validProject)
                {
                    ProjectHandler.WriteProjectConfig(NewProject);

                    Warbox.Project = NewProject;

                    ProjectHandler.LoadProject($"{NewProject.Config.ProjectDirectory}/project.json");

                    ToggleMenuVisibility();
                }
            }

            ImGui.EndTable();
        }
    }

    public static bool CanCreateNewProject()
    {
        var validated = true;

        if (NewProject_GameDirectory == null || !Directory.Exists(NewProject_GameDirectory))
        {
            PlatformUtils.Instance.MessageBox(
                "Your game directory path does not exist. Please select a valid directory.", "Error",
                MessageBoxButtons.OK);
            validated = false;
        }
        if (NewProject_ProjectDirectory == null || !Directory.Exists(NewProject_ProjectDirectory))
        {
            PlatformUtils.Instance.MessageBox(
                "Your project directory path does not exist. Please select a valid directory.", "Error",
                MessageBoxButtons.OK);
            validated = false;
        }

        if (validated && File.Exists($@"{NewProject_ProjectDirectory}\project.json"))
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

        if (validated && (NewProjectName == null || NewProjectName == ""))
        {
            PlatformUtils.Instance.MessageBox("You must specify a project name.", "Error",
                MessageBoxButtons.OK);
            validated = false;
        }

        return validated;
    }
}
