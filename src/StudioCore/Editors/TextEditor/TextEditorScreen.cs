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
        DataHandler.SetupLocalization();

        FileSelectionView = new(this);
        TextRowView = new(this);
        TextCellView = new(this);
    }

    public void DrawEditorMenu()
    {
        if (ImGui.BeginMenu("File"))
        {
            // Package
            if (ImGui.MenuItem($"Package", KeyBindings.Current.CORE_Package.HintText))
            {
                Warbox.ProjectHandler.WriteProjectConfig(Warbox.ProjectHandler.CurrentProject);
                Package();
            }

            // Save
            if (ImGui.MenuItem($"Save", KeyBindings.Current.CORE_Save.HintText))
            {
                Warbox.ProjectHandler.WriteProjectConfig(Warbox.ProjectHandler.CurrentProject);
                Save();
            }

            ImGui.EndMenu();
        }

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
        ResetActionManager();
    }

    public void Save()
    {
        var outputDir = $"{Warbox.ProjectDataRoot}\\Localization";

        if(!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        var status = FileSelectionView.SelectedStatus;
        var document = DataHandler.Localization[status];

        var writePath = status.Path;
        var fileDir = $"{outputDir}\\{writePath}";

        // If it is a project-specific file, use the status Path as it is a full path
        if (writePath.Contains(outputDir))
        {
            fileDir = $"{writePath}";
        }

        document.Save(fileDir);

        TaskLogs.AddLog($"{fileDir} saved.");
    }

    /// <summary>
    /// Creates the PAK file for the Localization folder based of the current loose files
    /// </summary>
    public void Package()
    {
        // Save first so the files are up to date.
        Save();

        var sourceDir = $"{Warbox.ProjectDataRoot}\\Localization\\";
        var pakName = "English_xml"; // Only support English for now.

        DataHandler.ZipXmlFiles(sourceDir, $"{sourceDir}\\{pakName}.pak");
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }

    public void Shortcuts()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_Save))
        {
            Warbox.ProjectHandler.WriteProjectConfig(Warbox.ProjectHandler.CurrentProject);
            Save();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_Package))
        {
            Warbox.ProjectHandler.WriteProjectConfig(Warbox.ProjectHandler.CurrentProject);
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
                var fileName = initcmd[1];
                var targetUiString = initcmd[2];

                var targetFile = DataHandler.Localization.Where(e => e.Key.Name == fileName).FirstOrDefault();

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
