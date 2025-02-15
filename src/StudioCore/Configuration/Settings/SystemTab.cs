using ImGuiNET;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Settings;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Configuration.Settings;

public static class SystemTab
{
    public static void Display()
    {
        if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Check for new versions of Warbox during startup",
                ref CFG.Current.System_Check_Program_Update);
            UIHelper.ShowHoverTooltip("When enabled Warbox will automatically check for new versions upon program start.");

            ImGui.SliderFloat("Frame Rate", ref CFG.Current.System_Frame_Rate, 20.0f, 240.0f);
            UIHelper.ShowHoverTooltip("Adjusts the frame rate of the viewport.");

            // Round FPS to the nearest whole number
            CFG.Current.System_Frame_Rate = (float)Math.Round(CFG.Current.System_Frame_Rate);

            if (ImGui.Button("Reset"))
            {
                CFG.Current.System_Frame_Rate = CFG.Default.System_Frame_Rate;
                CFG.Current.System_UI_Scale = CFG.Default.System_UI_Scale;
                Warbox.FontRebuildRequest = true;
            }
            ImGui.SameLine();
            if (ImGui.Button("Reset AppData"))
            {
                DialogResult result = PlatformUtils.Instance.MessageBox(
                $"Do you want to delete your AppData files?",
                $"Warning", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    var configFolder = CFG.GetConfigFilePath();
                    var keybindsFolder = CFG.GetBindingsFilePath();

                    if (File.Exists(configFolder))
                    {
                        File.Delete(configFolder);
                    }
                    if (File.Exists(keybindsFolder))
                    {
                        File.Delete(keybindsFolder);
                    }

                    CFG.Save();
                }
            }
            UIHelper.ShowHoverTooltip("This will delete your Warbox folder in %appdata%/Local/, allowing it to be re-generated.");
        }

        if (ImGui.CollapsingHeader("Loggers"))
        {
            ImGui.Checkbox("Display Information Logger", ref CFG.Current.Interface_DisplayInfoLogger);
            UIHelper.ShowHoverTooltip("If enabled, the information logger will be visible in the menubar.");
        }

        if (ImGui.CollapsingHeader("Secret Tools"))
        {
            //ImGui.Checkbox("Display Randomiser Toosl", ref CFG.Current.DisplayRandomiserTools);
            //UIHelper.ShowHoverTooltip("If enabled, the randomiser tools will be available via the icon bar.");

            ImGui.Checkbox("Display Debug Toosl", ref CFG.Current.DisplayDebugTools);
            UIHelper.ShowHoverTooltip("If enabled, the debug tools will be available via the icon bar.");
        }
    }
}
