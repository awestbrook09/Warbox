using ImGuiNET;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Configuration.Keybinds;


public static class CommonKeybindTab
{
    public static void Display()
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

            KeyBindings.Current.CORE_SaveTable = InputTracker.KeybindLine(6,
                KeyBindings.Current.CORE_SaveTable,
                KeyBindings.Default.CORE_SaveTable);

            KeyBindings.Current.CORE_SaveAllTables = InputTracker.KeybindLine(12,
                KeyBindings.Current.CORE_SaveAllTables,
                KeyBindings.Default.CORE_SaveAllTables);

            KeyBindings.Current.CORE_SavePatchedTableFile = InputTracker.KeybindLine(11,
                KeyBindings.Current.CORE_SavePatchedTableFile,
                KeyBindings.Default.CORE_SavePatchedTableFile);

            KeyBindings.Current.CORE_SaveAllPatchedTableFiles = InputTracker.KeybindLine(13,
                KeyBindings.Current.CORE_SaveAllPatchedTableFiles,
                KeyBindings.Default.CORE_SaveAllPatchedTableFiles);

            KeyBindings.Current.CORE_PackagePatchedTables = InputTracker.KeybindLine(10,
                KeyBindings.Current.CORE_PackagePatchedTables,
                KeyBindings.Default.CORE_PackagePatchedTables);

            KeyBindings.Current.CORE_SaveLocalizationFile = InputTracker.KeybindLine(14,
                KeyBindings.Current.CORE_SaveLocalizationFile,
                KeyBindings.Default.CORE_SaveLocalizationFile);

            KeyBindings.Current.CORE_SaveAllLocalizationFiles = InputTracker.KeybindLine(15,
                KeyBindings.Current.CORE_SaveAllLocalizationFiles,
                KeyBindings.Default.CORE_SaveAllLocalizationFiles);

            KeyBindings.Current.CORE_PackageLocalizationFiles = InputTracker.KeybindLine(16,
                KeyBindings.Current.CORE_PackageLocalizationFiles,
                KeyBindings.Default.CORE_PackageLocalizationFiles);
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
