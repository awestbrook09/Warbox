using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Editor;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using StudioCore.Utilities;
using StudioCore.Core.Project;
using StudioCore.Interface;
using System.IO;
using StudioCore.Core.Data;
using StudioCore.Editors.TextEditor.Views;
using StudioCore.Editors.TextEditor.Framework;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using static Assimp.Metadata;

namespace StudioCore.TextEditor;

public class TextEditorScreen : EditorScreen
{
    public string EditorName => "Localization";
    public string CommandEndpoint => "text";

    public FileSelectionView FileSelectionView;
    public TextRowView TextRowView;
    public TextCellView TextCellView;

    public ActionManager EditorActionManager = new();

    public TextEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        DataHandler.SetupTextEditor();

        FileSelectionView = new(this);
        TextRowView = new(this);
        TextCellView = new(this);
    }

    public void DrawEditorMenu()
    {
        // Save
        if (ImGui.BeginMenu("Save"))
        {
            if (ImGui.MenuItem($"Save Localization", KeyBindings.Current.CORE_SaveLocalizationFile.HintText))
            {
                Warbox.Project.UpdateProjectJSON();
                Save();
            }
            UIHelper.ShowHoverTooltip("Save the current localization file.");

            ImGui.EndMenu();
        }

        ImGui.Separator();

        // Package
        if (ImGui.BeginMenu("Package"))
        {
            if (ImGui.MenuItem($"Package Localization", KeyBindings.Current.CORE_PackageLocalizationFiles.HintText))
            {
                Warbox.Project.UpdateProjectJSON();
                Package();
            }
            UIHelper.ShowHoverTooltip("Package the localization files for the current language.");

            ImGui.EndMenu();
        }

        ImGui.Separator();

        // Edit
        if (ImGui.BeginMenu("Edit"))
        {
            if (ImGui.MenuItem($"Undo", KeyBindings.Current.CORE_UndoAction.HintText, false,
                    EditorActionManager.CanUndo()))
            {
                EditorActionManager.UndoAction();
            }

            if (ImGui.MenuItem("Undo All", "", false,
                    EditorActionManager.CanUndo()))
            {
                EditorActionManager.UndoAllAction();
            }

            if (ImGui.MenuItem("Redo", KeyBindings.Current.CORE_RedoAction.HintText, false,
                    EditorActionManager.CanRedo()))
            {
                EditorActionManager.RedoAction();
            }

            ImGui.EndMenu();
        }

        // Views
        /*
        ImGui.Separator();

        if (ImGui.BeginMenu("Views"))
        {

            ImGui.EndMenu();
        }

        ImGui.Separator();

        // Toggles
        if (ImGui.BeginMenu("Toggles"))
        {

            ImGui.EndMenu();
        }
        */
    }

    public void OnGUI(string[] initcmd)
    {
        var scale = Warbox.GetUIScale();

        // Docking setup
        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4) * scale);
        Vector2 wins = ImGui.GetWindowSize();
        Vector2 winp = ImGui.GetWindowPos();
        winp.Y += 20.0f * scale;
        wins.Y -= 20.0f * scale;
        ImGui.SetNextWindowPos(winp);
        ImGui.SetNextWindowSize(wins);

        var dsid = ImGui.GetID("DockSpace_TextEntries");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        Shortcuts();
        FileSelectionView.Shortcuts();
        TextRowView.Shortcuts();
        TextCellView.Shortcuts();

        EditorCommandQueue(initcmd);

        FileSelectionView.Display();
        TextRowView.Display();
        TextCellView.Display();

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
    }

    public void OnProjectChanged()
    {
        FileSelectionView.SelectedStatus = null;
        FileSelectionView.SelectedDocument = null;
        FileSelectionView.SelectedElements = null;

        TextRowView.TextEntryIndex = -1;
        TextRowView.SelectedCells = null;

        ResetActionManager();
    }

    public void Save()
    {
        var curLocalization = DataHandler.GetCurrentLocalization();

        if (curLocalization == null)
            return;

        var resDesc = FileSelectionView.SelectedStatus;
        var document = curLocalization[resDesc];

        var writeDir = $"{Warbox.Project.ProjectDirectory}\\Source\\Localization\\{CFG.Current.TextEditor_CurrentLanguage}\\";
        var writePath = $"{Warbox.Project.ProjectDirectory}\\Source\\Localization\\{CFG.Current.TextEditor_CurrentLanguage}\\{resDesc.Name}{resDesc.Extension}";

        if (!Directory.Exists(writeDir))
            Directory.CreateDirectory(writeDir);

        document.Save(writePath);

        TaskLogs.AddLog($"{writePath} saved.");
    }

    public void Package()
    {
        // Save first so the files are up to date.
        Save();

        var sourceDir = $"{Warbox.Project.ProjectDirectory}\\Source\\Localization\\{CFG.Current.TextEditor_CurrentLanguage}";
        var writeDir = $"{Warbox.Project.ProjectDirectory}\\Localization\\";

        foreach(var entry in DataHandler.Localization)
        {
            var fileName = entry.Key;


        }

        var pakName = "English_xml"; // Only support English for now.

        DataHandler.ZipXmlFiles(sourceDir, $"{sourceDir}\\{pakName}.pak");

        ManifestHandler.CreateManisfestIfMissing();
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }

    public void Shortcuts()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_SaveLocalizationFile))
        {
            Warbox.Project.UpdateProjectJSON();
            Save();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_PackageLocalizationFiles))
        {
            Warbox.Project.UpdateProjectJSON();
            Package();
        }

        if (EditorActionManager.CanUndo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_UndoAction))
        {
            EditorActionManager.UndoAction();
        }

        if (EditorActionManager.CanRedo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_RedoAction))
        {
            EditorActionManager.RedoAction();
        }
    }

    public void EditorCommandQueue(string[] initcmd)
    {
        // Parse select commands
        if (initcmd != null && initcmd[0] == "select")
        {
            if (initcmd.Length > 2)
            {
                var curLocalization = DataHandler.GetCurrentLocalization();

                if (curLocalization == null)
                    return;

                var fileName = initcmd[1];
                var targetUiString = initcmd[2];

                var targetFile = curLocalization.Where(e => e.Key.Name == fileName).FirstOrDefault();

                FileSelectionView.UpdateSelection(targetFile, true);

                // Set row selection
                if (FileSelectionView.SelectedDocument != null)
                {
                    var curElements = FileSelectionView.SelectedElements;

                    // Row
                    for (int i = 0; i < curElements.Count; i++)
                    {
                        var entry = curElements[i];
                        var cells = entry.Elements().ToList();

                        var id = cells[0].Value;
                        var text = cells[1].Value;
                        var fallback_text = cells[2].Value;

                        if(id == targetUiString)
                        {
                            TextRowView.UpdateSelection(entry, i, true);
                        }
                    }
                }
            }
        }
    }
}
