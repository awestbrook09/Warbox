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

public static class KeybindWindow
{
    private static KeyBind _currentKeyBind;
    public static bool MenuOpenState;

    public static void SaveSettings()
    {
        CFG.Save();
    }

    public static void ToggleMenuVisibility()
    {
        MenuOpenState = !MenuOpenState;
    }

    public static void Display()
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
                    CommonKeybindTab.Display();
                    break;
                case SelectedKeybindTab.Viewport:
                    ViewportKeybindTab.Display();
                    break;
            }
            ImGui.EndChild();

            ImGui.Columns(1);
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);
    }

    private static SelectedKeybindTab SelectedTab = SelectedKeybindTab.Common;

    public enum SelectedKeybindTab
    {
        [Display(Name = "Common")] Common,
        [Display(Name = "Viewport")] Viewport
    }
}
