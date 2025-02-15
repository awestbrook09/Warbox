using ImGuiNET;
using StudioCore.Editor;
using StudioCore.Utilities;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Threading.Tasks;
using System;
using StudioCore.Interface;

namespace StudioCore.Tools.Development;

public static class DebugWindow
{
    private static bool MenuOpenState;

    public static bool _showImGuiDemoWindow = false;
    public static bool _showImGuiMetricsWindow = false;
    public static bool _showImGuiDebugLogWindow = false;
    public static bool _showImGuiStackToolWindow = false;

    public static void ToggleMenuVisibility()
    {
        MenuOpenState = !MenuOpenState;
    }

    private static Task _loadingTask;

    private static SelectedDebugTab SelectedTab = SelectedDebugTab.DisplayTaskStatus;

    public enum SelectedDebugTab
    {
        // Information
        [Display(Name = "Task Status")] DisplayTaskStatus,

        // ImGui
        [Display(Name = "ImGui Demo")] ImGuiDemo,
        [Display(Name = "ImGui Metrics")] ImGuiMetrics,
        [Display(Name = "ImGui Debug Log")] ImGuiLog,
        [Display(Name = "ImGui Stack Tool")] ImGuiStackTool
    }

    public static void Display()
    {
        var scale = Warbox.GetUIScale();

        if (!MenuOpenState)
            return;

        ImGui.SetNextWindowSize(new Vector2(600.0f, 600.0f) * scale, ImGuiCond.FirstUseEver);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, CFG.Current.Imgui_Moveable_MainBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, CFG.Current.Imgui_Moveable_TitleBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, CFG.Current.Imgui_Moveable_TitleBg_Active);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, CFG.Current.Imgui_Moveable_ChildBg);
        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(5.0f, 5.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f * scale);

        if (ImGui.Begin("Debug##TestWindow", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            ImGui.Columns(2);

            ImGui.BeginChild("debugToolList");

            var arr = Enum.GetValues(typeof(SelectedDebugTab));
            for (int i = 0; i < arr.Length; i++)
            {
                var tab = (SelectedDebugTab)arr.GetValue(i);

                if (tab == SelectedDebugTab.DisplayTaskStatus)
                {
                    ImGui.Separator();
                    UIHelper.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "Information");
                    ImGui.Separator();
                }
                if (tab == SelectedDebugTab.ImGuiDemo)
                {
                    ImGui.Separator();
                    UIHelper.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "ImGui");
                    ImGui.Separator();
                }

                if (ImGui.Selectable(tab.GetDisplayName(), tab == SelectedTab))
                {
                    SelectedTab = tab;
                }
            }

            ImGui.EndChild();

            ImGui.NextColumn();

            ImGui.BeginChild("configurationTab");
            switch (SelectedTab)
            {
                // Information
                case SelectedDebugTab.DisplayTaskStatus:
                    DisplayTasks();
                    break;

                // ImGui
                case SelectedDebugTab.ImGuiDemo:
                    DisplayImGuiDemo();
                    break;
                case SelectedDebugTab.ImGuiMetrics:
                    DisplayImGuiMetrics();
                    break;
                case SelectedDebugTab.ImGuiLog:
                    DisplayImGuiDebugLog();
                    break;
                case SelectedDebugTab.ImGuiStackTool:
                    DisplayImGuiStackTool();
                    break;
            }
            ImGui.EndChild();

            ImGui.Columns(1);
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);
    }

    // Information
    private static void DisplayTasks()
    {
        ImGui.Text("Currently running tasks:");
        ImGui.Text("");

        if (TaskManager.GetLiveThreads().Count > 0)
        {
            foreach (var task in TaskManager.GetLiveThreads())
            {
                ImGui.Text(task);
            }
        }
    }

    // ImGui
    private static void DisplayImGuiDemo()
    {
        var buttonSize = new Vector2(ImGui.GetWindowWidth(), 32);

        if (ImGui.Button("Demo", buttonSize))
        {
            _showImGuiDemoWindow = !_showImGuiDemoWindow;
        }
    }
    private static void DisplayImGuiMetrics()
    {
        var buttonSize = new Vector2(ImGui.GetWindowWidth(), 32);

        if (ImGui.Button("Metrics", buttonSize))
        {
            _showImGuiMetricsWindow = !_showImGuiMetricsWindow;
        }
    }
    private static void DisplayImGuiDebugLog()
    {
        var buttonSize = new Vector2(ImGui.GetWindowWidth(), 32);

        if (ImGui.Button("Debug Log", buttonSize))
        {
            _showImGuiDebugLogWindow = !_showImGuiDebugLogWindow;
        }
    }
    private static void DisplayImGuiStackTool()
    {
        var buttonSize = new Vector2(ImGui.GetWindowWidth(), 32);

        if (ImGui.Button("Stack Tool", buttonSize))
        {
            _showImGuiStackToolWindow = !_showImGuiStackToolWindow;
        }
    }
}
