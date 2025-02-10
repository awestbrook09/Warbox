using ImGuiNET;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Interface;

public static class UIHelper
{
    public static void DisplayAlias(string aliasName)
    {
        if (aliasName != "")
        {
            var sanitisedName = aliasName.Replace("%", "%%");

            ImGui.SameLine();

            if (CFG.Current.System_WrapAliasDisplay)
            {
                ImGui.PushTextWrapPos();
                ImGui.TextColored(CFG.Current.ImGui_AliasName_Text, $"{sanitisedName}");
                ImGui.PopTextWrapPos();
            }
            else
            {
                ImGui.TextColored(CFG.Current.ImGui_AliasName_Text, $"{sanitisedName}");
            }
        }
    }

    public static void ShowHoverTooltip(string desc)
    {
        if (CFG.Current.System_Show_UI_Tooltips)
        {
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(450.0f);
                ImGui.TextUnformatted(desc);
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
        }
    }
    public static void WrappedText(string text)
    {
        var size = ImGui.GetWindowSize();

        ImGui.PushTextWrapPos(size.X);
        ImGui.TextUnformatted(text);
        ImGui.PopTextWrapPos();
    }

    public static void WrappedTextColored(Vector4 color, string text)
    {
        var size = ImGui.GetWindowSize();

        ImGui.PushTextWrapPos(size.X);
        ImGui.PushStyleColor(ImGuiCol.Text, color);
        ImGui.TextUnformatted(text);
        ImGui.PopStyleColor();
        ImGui.PopTextWrapPos();
    }

    public static void ShowMenuIcon(string iconStr)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));
        ImGui.TextUnformatted(iconStr);
        ImGui.PopStyleVar(1);
        ImGui.SameLine();
    }

    public static void ShowActiveStatus(bool isActive)
    {
        if (isActive)
        {
            ImGui.SameLine();
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));
            ImGui.TextUnformatted($"{ForkAwesome.CheckSquare}");
            ImGui.PopStyleVar(1);
        }
        else
        {
            ImGui.SameLine();
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));
            ImGui.TextUnformatted($"{ForkAwesome.Square}");
            ImGui.PopStyleVar(1);
        }
    }

    public static void DisplayHeaderText(string text, bool wrap = false)
    {
        if (wrap)
        {
            ImGui.PushTextWrapPos();
            ImGui.TextColored(CFG.Current.ImGui_Header_Text_Color, @$"{text}");
            ImGui.PopTextWrapPos();
        }
        else
        {
            ImGui.TextColored(CFG.Current.ImGui_Header_Text_Color, @$"{text}");
        }
    }

    public static void DisplayInformationText(string text, bool wrap = false)
    {
        if (wrap)
        {
            ImGui.PushTextWrapPos();
            ImGui.TextColored(CFG.Current.ImGui_Information_Text_Color, @$"{text}");
            ImGui.PopTextWrapPos();
        }
        else
        {
            ImGui.TextColored(CFG.Current.ImGui_Information_Text_Color, @$"{text}");
        }
    }
    public static void DisplayWarningText(string text, bool wrap = false)
    {
        if (wrap)
        {
            ImGui.PushTextWrapPos();
            ImGui.TextColored(CFG.Current.ImGui_Warning_Text_Color, @$"{text}");
            ImGui.PopTextWrapPos();
        }
        else
        {
            ImGui.TextColored(CFG.Current.ImGui_Warning_Text_Color, @$"{text}");
        }
    }
    public static void DisplayHighlightText(string text, bool wrap = false)
    {
        if (wrap)
        {
            ImGui.PushTextWrapPos();
            ImGui.TextColored(CFG.Current.ImGui_Highlight_Text_Color, @$"{text}");
            ImGui.PopTextWrapPos();
        }
        else
        {
            ImGui.TextColored(CFG.Current.ImGui_Highlight_Text_Color, @$"{text}");
        }
    }
    public static void DisplayActionText(string text, bool wrap = false)
    {
        if (wrap)
        {
            ImGui.PushTextWrapPos();
            ImGui.TextColored(CFG.Current.ImGui_Action_Text_Color, @$"{text}");
            ImGui.PopTextWrapPos();
        }
        else
        {
            ImGui.TextColored(CFG.Current.ImGui_Action_Text_Color, @$"{text}");
        }
    }

    public static void DisplayColoredText(string text, Vector4 color)
    {
        if (CFG.Current.System_WrapAliasDisplay)
        {
            ImGui.PushTextWrapPos();
            ImGui.TextColored(CFG.Current.ImGui_AliasName_Text, @$"{text}");
            ImGui.PopTextWrapPos();
        }
        else
        {
            ImGui.TextColored(CFG.Current.ImGui_AliasName_Text, @$"{text}");
        }
    }

    public static string GetKeybindHint(string hint)
    {
        if (hint == "")
            return "None";
        else
            return hint;
    }

    public static void RestoreImguiIfMissing()
    {
        var curImgui = $@"{AppContext.BaseDirectory}\imgui.ini";
        var defaultImgui = $@"{AppContext.BaseDirectory}\imgui.default";

        if (!File.Exists(curImgui) && File.Exists(defaultImgui))
        {
            var bytes = File.ReadAllBytes(defaultImgui);
            File.WriteAllBytes(curImgui, bytes);
        }
    }
}
