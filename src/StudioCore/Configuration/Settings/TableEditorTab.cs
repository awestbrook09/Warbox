using ImGuiNET;
using StudioCore.Interface;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Configuration.Settings;

public static class TableEditorTab
{
    public static void Display()
    {
        if (ImGui.CollapsingHeader("Rows", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Use Shared Row Filter Bar", ref CFG.Current.TableEditor_UseSharedRowFilterText);
            UIHelper.ShowHoverTooltip("If enabled, the row filter input will persist between each file. Otherwise, the input will be per file.");
        }

        if (ImGui.CollapsingHeader("Properties", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Use Shared Property Filter Bar", ref CFG.Current.TableEditor_UseSharedPropertyFilterText);
            UIHelper.ShowHoverTooltip("If enabled, the property filter input will persist between each file. Otherwise, the input will be per file.");

            ImGui.Checkbox("Use Display Names", ref CFG.Current.TableEditor_View_Properties_DisplayNames);
            UIHelper.ShowHoverTooltip("If enabled, display names will be shown for the fields, instead of their internal script name");
        }
    }
}
