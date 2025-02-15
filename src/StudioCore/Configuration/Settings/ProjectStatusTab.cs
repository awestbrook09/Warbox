using ImGuiNET;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace StudioCore.Configuration.Settings;

public static class ProjectStatusTab
{
    public static void Display()
    {
        var widthUnit = ImGui.GetWindowWidth();

        if (TaskManager.AnyActiveTasks())
        {
            ImGui.Text("Waiting for program tasks to finish...");
            UIHelper.ShowHoverTooltip("Warbox must finished all program tasks before it can load a project.");
        }
        else
        {
            ImGui.Text($"Project Name: {Warbox.Project.ProjectName}");
            ImGui.Text($"Game Data Directory: {Warbox.Project.GameDirectory}");
            ImGui.Text($"Project Data Directory: {Warbox.Project.ProjectDirectory}");
        }
    }
}
