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


public class TableEditorTab
{
    public TableEditorTab() { }

    public void Display()
    {
        if (ImGui.CollapsingHeader("Properties", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Use Display Names", ref CFG.Current.TableEditor_View_Properties_DisplayNames);
            UIHelper.ShowHoverTooltip("If enabled, display names will be shown for the fields, instead of their internal script name");
        }
    }
}
