using ImGuiNET;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Configuration.Keybinds;

public static class ViewportKeybindTab
{
    public static void Display()
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