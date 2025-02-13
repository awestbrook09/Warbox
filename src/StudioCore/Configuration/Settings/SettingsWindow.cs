using ImGuiNET;
using System;
using System.Numerics;
using System.ComponentModel.DataAnnotations;
using StudioCore.Utilities;
using StudioCore.Interface;

namespace StudioCore.Configuration.Settings;

public class SettingsWindow
{
    public bool MenuOpenState;

    private SystemTab SystemSettings;
    private InterfaceTab InterfaceSettings;

    private ProjectStatusTab ProjectStatusTab;
    private ProjectSettingsTab ProjectSettingsTab;

    private TableEditorTab TableEditorSettings;
    private TextEditorTab TextEditorSettings;

    private bool TabInitialized = false;

    public SettingsWindow()
    {
        SystemSettings = new SystemTab();
        InterfaceSettings = new InterfaceTab();

        ProjectStatusTab = new ProjectStatusTab();
        ProjectSettingsTab = new ProjectSettingsTab();

        TableEditorSettings = new TableEditorTab();
        TextEditorSettings = new TextEditorTab();
    }

    public void SaveSettings()
    {
        CFG.Save();
    }
    public void ToggleMenuVisibility()
    {
        MenuOpenState = !MenuOpenState;
    }

    public void Display()
    {
        var scale = Warbox.GetUIScale();

        if (!MenuOpenState)
            return;

        ImGui.SetNextWindowSize(new Vector2(1200.0f, 1000.0f) * scale, ImGuiCond.FirstUseEver);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, CFG.Current.Imgui_Moveable_MainBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, CFG.Current.Imgui_Moveable_TitleBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, CFG.Current.Imgui_Moveable_TitleBg_Active);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, CFG.Current.Imgui_Moveable_ChildBg);
        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(5.0f, 5.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f * scale);

        if (ImGui.Begin("Configuration##configurationWindow", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            ImGui.Columns(2);

            ImGui.BeginChild("configurationTabList");

            var arr = Enum.GetValues(typeof(SelectedSettingTab));
            for (int i = 0; i < arr.Length; i++)
            {
                var tab = (SelectedSettingTab)arr.GetValue(i);

                if (tab == SelectedSettingTab.ProjectStatus)
                {
                    ImGui.Separator();
                    UIHelper.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "Project");
                    ImGui.Separator();
                }
                if (tab == SelectedSettingTab.System)
                {
                    ImGui.Separator();
                    UIHelper.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "Settings");
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
                case SelectedSettingTab.System:
                    SystemSettings.Display();
                    break;
                case SelectedSettingTab.Interface:
                    InterfaceSettings.Display();
                    break;
                case SelectedSettingTab.Project:
                    ProjectSettingsTab.Display();
                    break;
                case SelectedSettingTab.ProjectStatus:
                    ProjectStatusTab.Display();
                    break;
                case SelectedSettingTab.TableEditor:
                    TableEditorSettings.Display();
                    break;
                case SelectedSettingTab.TextEditor:
                    TextEditorSettings.Display();
                    break;
            }
            ImGui.EndChild();

            ImGui.Columns(1);
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);
    }

    private SelectedSettingTab SelectedTab = SelectedSettingTab.System;

    public enum SelectedSettingTab
    {
        [Display(Name = "Status")] ProjectStatus,

        [Display(Name = "System")] System,
        [Display(Name = "Project")] Project,
        [Display(Name = "Interface")] Interface,

        [Display(Name = "Table Editor")] TableEditor,
        [Display(Name = "Text Editor")] TextEditor
    }
}
