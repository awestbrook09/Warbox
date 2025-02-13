using ImGuiNET;
using Octokit;
using StudioCore.Configuration;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using static StudioCore.Configuration.Settings.SettingsWindow;

namespace StudioCore.Configuration.Keybinds;
public class KeybindWindow
{
    private KeyBind _currentKeyBind;
    public bool MenuOpenState;

    private CommonKeybindTab CommonKeybinds;
    private ViewportKeybindTab ViewportKeybinds;

    public KeybindWindow()
    {
        CommonKeybinds = new CommonKeybindTab();
        ViewportKeybinds = new ViewportKeybindTab();
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

        ImGui.SetNextWindowSize(new Vector2(900.0f, 800.0f) * scale, ImGuiCond.FirstUseEver);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, CFG.Current.Imgui_Moveable_MainBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, CFG.Current.Imgui_Moveable_TitleBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, CFG.Current.Imgui_Moveable_TitleBg_Active);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, CFG.Current.Imgui_Moveable_ChildBg);
        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(5.0f, 5.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f * scale);

        if (ImGui.Begin("Keybinds##KeybindWindow", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            ImGui.Columns(2);

            ImGui.BeginChild("keybindTabList");

            var arr = Enum.GetValues(typeof(SelectedKeybindTab));
            for (int i = 0; i < arr.Length; i++)
            {
                var tab = (SelectedKeybindTab)arr.GetValue(i);

                if (ImGui.Selectable(tab.GetDisplayName(), tab == SelectedTab))
                {
                    SelectedTab = tab;
                }
            }
            ImGui.EndChild();

            ImGui.NextColumn();

            ImGui.BeginChild("keybindTab");
            switch (SelectedTab)
            {
                case SelectedKeybindTab.Common:
                    CommonKeybinds.Display();
                    break;
                case SelectedKeybindTab.Viewport:
                    ViewportKeybinds.Display();
                    break;
            }
            ImGui.EndChild();

            ImGui.Columns(1);
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);
    }

    private SelectedKeybindTab SelectedTab = SelectedKeybindTab.Common;

    public enum SelectedKeybindTab
    {
        [Display(Name = "Common")] Common,
        [Display(Name = "Viewport")] Viewport
    }
}

public class CommonKeybindTab
{
    public CommonKeybindTab() { }

    public void Display()
    {
        ImGui.Separator();
        UIHelper.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "Keybinds");
        ImGui.Separator();

        if (ImGui.CollapsingHeader("Core", ImGuiTreeNodeFlags.DefaultOpen))
        {
            KeyBindings.Current.CORE_CreateNewEntry = InputTracker.KeybindLine(0,
                KeyBindings.Current.CORE_CreateNewEntry,
                KeyBindings.Default.CORE_CreateNewEntry);

            KeyBindings.Current.CORE_DeleteSelectedEntry = InputTracker.KeybindLine(1,
                KeyBindings.Current.CORE_DeleteSelectedEntry,
                KeyBindings.Default.CORE_DeleteSelectedEntry);

            KeyBindings.Current.CORE_DuplicateSelectedEntry = InputTracker.KeybindLine(2,
                KeyBindings.Current.CORE_DuplicateSelectedEntry,
                KeyBindings.Default.CORE_DuplicateSelectedEntry);

            KeyBindings.Current.CORE_RedoAction = InputTracker.KeybindLine(3,
                KeyBindings.Current.CORE_RedoAction,
                KeyBindings.Default.CORE_RedoAction);

            KeyBindings.Current.CORE_UndoAction = InputTracker.KeybindLine(4,
                KeyBindings.Current.CORE_UndoAction,
                KeyBindings.Default.CORE_UndoAction);

            KeyBindings.Current.CORE_Save = InputTracker.KeybindLine(6,
                KeyBindings.Current.CORE_Save,
                KeyBindings.Default.CORE_Save);

            KeyBindings.Current.CORE_SaveAll = InputTracker.KeybindLine(12,
                KeyBindings.Current.CORE_SaveAll,
                KeyBindings.Default.CORE_SaveAll);

            KeyBindings.Current.CORE_SavePTF = InputTracker.KeybindLine(11,
                KeyBindings.Current.CORE_SavePTF,
                KeyBindings.Default.CORE_SavePTF);

            KeyBindings.Current.CORE_Package = InputTracker.KeybindLine(10,
                KeyBindings.Current.CORE_Package,
                KeyBindings.Default.CORE_Package);
        }

        if (ImGui.CollapsingHeader("Windows", ImGuiTreeNodeFlags.DefaultOpen))
        {
            KeyBindings.Current.CORE_ConfigurationWindow = InputTracker.KeybindLine(7,
                KeyBindings.Current.CORE_ConfigurationWindow,
                KeyBindings.Default.CORE_ConfigurationWindow);

            KeyBindings.Current.CORE_HelpWindow = InputTracker.KeybindLine(8,
                KeyBindings.Current.CORE_HelpWindow,
                KeyBindings.Default.CORE_HelpWindow);

            KeyBindings.Current.CORE_KeybindConfigWindow = InputTracker.KeybindLine(9,
                KeyBindings.Current.CORE_KeybindConfigWindow,
                KeyBindings.Default.CORE_KeybindConfigWindow);
        }
    }
}

public class ViewportKeybindTab
{
    public ViewportKeybindTab() { }

    public void Display()
    {
        ImGui.Separator();
        UIHelper.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "Keybinds");
        ImGui.Separator();

        if (ImGui.CollapsingHeader("Core", ImGuiTreeNodeFlags.DefaultOpen))
        {
            KeyBindings.Current.VIEWPORT_CameraForward = InputTracker.KeybindLine(0,
                KeyBindings.Current.VIEWPORT_CameraForward,
                KeyBindings.Default.VIEWPORT_CameraForward);

            KeyBindings.Current.VIEWPORT_CameraBack = InputTracker.KeybindLine(1,
                KeyBindings.Current.VIEWPORT_CameraBack,
                KeyBindings.Default.VIEWPORT_CameraBack);

            KeyBindings.Current.VIEWPORT_CameraUp = InputTracker.KeybindLine(2,
                KeyBindings.Current.VIEWPORT_CameraUp,
                KeyBindings.Default.VIEWPORT_CameraUp);

            KeyBindings.Current.VIEWPORT_CameraDown = InputTracker.KeybindLine(3,
                KeyBindings.Current.VIEWPORT_CameraDown,
                KeyBindings.Default.VIEWPORT_CameraDown);

            KeyBindings.Current.VIEWPORT_CameraLeft = InputTracker.KeybindLine(4,
                KeyBindings.Current.VIEWPORT_CameraLeft,
                KeyBindings.Default.VIEWPORT_CameraLeft);

            KeyBindings.Current.VIEWPORT_CameraRight = InputTracker.KeybindLine(5,
                KeyBindings.Current.VIEWPORT_CameraRight,
                KeyBindings.Default.VIEWPORT_CameraRight);

            KeyBindings.Current.VIEWPORT_CameraReset = InputTracker.KeybindLine(6,
                KeyBindings.Current.VIEWPORT_CameraReset,
                KeyBindings.Default.VIEWPORT_CameraReset);
        }

        if (ImGui.CollapsingHeader("Gizmos", ImGuiTreeNodeFlags.DefaultOpen))
        {
            KeyBindings.Current.VIEWPORT_GizmoRotationMode = InputTracker.KeybindLine(7,
                KeyBindings.Current.VIEWPORT_GizmoRotationMode,
                KeyBindings.Default.VIEWPORT_GizmoRotationMode);

            KeyBindings.Current.VIEWPORT_GizmoOriginMode = InputTracker.KeybindLine(8,
                KeyBindings.Current.VIEWPORT_GizmoOriginMode,
                KeyBindings.Default.VIEWPORT_GizmoOriginMode);

            KeyBindings.Current.VIEWPORT_GizmoSpaceMode = InputTracker.KeybindLine(9,
                KeyBindings.Current.VIEWPORT_GizmoSpaceMode,
                KeyBindings.Default.VIEWPORT_GizmoSpaceMode);

            KeyBindings.Current.VIEWPORT_GizmoTranslationMode = InputTracker.KeybindLine(10,
                KeyBindings.Current.VIEWPORT_GizmoTranslationMode,
                KeyBindings.Default.VIEWPORT_GizmoTranslationMode);
        }

        if (ImGui.CollapsingHeader("Grid", ImGuiTreeNodeFlags.DefaultOpen))
        {
            KeyBindings.Current.VIEWPORT_LowerGrid = InputTracker.KeybindLine(11,
                KeyBindings.Current.VIEWPORT_LowerGrid,
                KeyBindings.Default.VIEWPORT_LowerGrid);

            KeyBindings.Current.VIEWPORT_RaiseGrid = InputTracker.KeybindLine(12,
                KeyBindings.Current.VIEWPORT_RaiseGrid,
                KeyBindings.Default.VIEWPORT_RaiseGrid);

            KeyBindings.Current.VIEWPORT_SetGridToSelectionHeight = InputTracker.KeybindLine(13,
                KeyBindings.Current.VIEWPORT_SetGridToSelectionHeight,
                KeyBindings.Default.VIEWPORT_SetGridToSelectionHeight);
        }

        if (ImGui.CollapsingHeader("Selection", ImGuiTreeNodeFlags.DefaultOpen))
        {
            KeyBindings.Current.VIEWPORT_RenderOutline = InputTracker.KeybindLine(14,
                KeyBindings.Current.VIEWPORT_RenderOutline,
                KeyBindings.Default.VIEWPORT_RenderOutline);
        }
    }
}