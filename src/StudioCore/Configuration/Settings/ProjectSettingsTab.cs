using ImGuiNET;
using StudioCore.Interface;
using StudioCore.Utilities;

namespace StudioCore.Configuration.Settings;

public static class ProjectSettingsTab
{
    public static void Display()
    {
        if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Enable Automatic Recent Project Loading", ref CFG.Current.Project_LoadPreviousProject);
            UIHelper.ShowHoverTooltip("The last loaded project will be automatically loaded when Warbox starts up if this is enabled.");
        }
    }
}
