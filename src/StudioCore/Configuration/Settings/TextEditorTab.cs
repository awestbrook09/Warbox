using ImGuiNET;
using StudioCore.Core.Data;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Configuration.Settings;

public class TextEditorTab
{
    public TextEditorTab() { }

    public void Display()
    {
        if (ImGui.CollapsingHeader("Languages", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Language: Chinese (Simplified)", ref CFG.Current.TextEditor_EnableLanguage_ChineseSimplified);
            UIHelper.ShowHoverTooltip("If enabled, this language will be loaded and available in the Text Editor.");

            ImGui.Checkbox("Language: Chinese (Traditional)", ref CFG.Current.TextEditor_EnableLanguage_ChineseTraditional);
            UIHelper.ShowHoverTooltip("If enabled, this language will be loaded and available in the Text Editor.");

            ImGui.Checkbox("Language: Czech", ref CFG.Current.TextEditor_EnableLanguage_Czech);
            UIHelper.ShowHoverTooltip("If enabled, this language will be loaded and available in the Text Editor.");

            ImGui.Checkbox("Language: French", ref CFG.Current.TextEditor_EnableLanguage_French);
            UIHelper.ShowHoverTooltip("If enabled, this language will be loaded and available in the Text Editor.");

            ImGui.Checkbox("Language: German", ref CFG.Current.TextEditor_EnableLanguage_German);
            UIHelper.ShowHoverTooltip("If enabled, this language will be loaded and available in the Text Editor.");

            ImGui.Checkbox("Language: Italian", ref CFG.Current.TextEditor_EnableLanguage_Italian);
            UIHelper.ShowHoverTooltip("If enabled, this language will be loaded and available in the Text Editor.");

            ImGui.Checkbox("Language: Japanese", ref CFG.Current.TextEditor_EnableLanguage_Japanese);
            UIHelper.ShowHoverTooltip("If enabled, this language will be loaded and available in the Text Editor.");

            ImGui.Checkbox("Language: Korean", ref CFG.Current.System_Font_Korean);
            UIHelper.ShowHoverTooltip("If enabled, this language will be loaded and available in the Text Editor.");

            ImGui.Checkbox("Language: Polish", ref CFG.Current.TextEditor_EnableLanguage_Polish);
            UIHelper.ShowHoverTooltip("If enabled, this language will be loaded and available in the Text Editor.");

            ImGui.Checkbox("Language: Portuguese", ref CFG.Current.TextEditor_EnableLanguage_Portuguese);
            UIHelper.ShowHoverTooltip("If enabled, this language will be loaded and available in the Text Editor.");

            ImGui.Checkbox("Language: Russian", ref CFG.Current.TextEditor_EnableLanguage_Russian);
            UIHelper.ShowHoverTooltip("If enabled, this language will be loaded and available in the Text Editor.");

            ImGui.Checkbox("Language: Spanish", ref CFG.Current.TextEditor_EnableLanguage_Spanish);
            UIHelper.ShowHoverTooltip("If enabled, this language will be loaded and available in the Text Editor.");

            ImGui.Checkbox("Language: Turkish", ref CFG.Current.TextEditor_EnableLanguage_Turkish);
            UIHelper.ShowHoverTooltip("If enabled, this language will be loaded and available in the Text Editor.");

            ImGui.Checkbox("Language: Ukrainian", ref CFG.Current.TextEditor_EnableLanguage_Ukrainian);
            UIHelper.ShowHoverTooltip("If enabled, this language will be loaded and available in the Text Editor.");

            if(ImGui.Button("Reload Languages"))
            {
                DataHandler.SetupLocalization();
                TaskLogs.AddLog("Reloaded the localization files.");
            }
        }
    }
}
