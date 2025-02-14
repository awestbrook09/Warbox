using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Core.Data;
using StudioCore.Editor;
using StudioCore.Editors.TextEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid.Sdl2;
using Veldrid;
using System.Xml.Linq;
using CommunityToolkit.HighPerformance;
using StudioCore.Interface;
using System.Runtime.InteropServices;
using static Assimp.Metadata;
using StudioCore.Editors.TableEditor.Views;
using StudioCore.Editors.TableEditor.Framework;

namespace StudioCore.Editors.TableEditor;

public class TableEditorScreen : EditorScreen
{
    public string EditorName => "Tables";
    public string CommandEndpoint => "table";

    public TableFileSelectionView FileSelectionView;
    public TableDataView TableDataView;
    public TableToolsView TableToolsView;

    public ActionManager EditorActionManager = new();

    public TableEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        DataHandler.SetupTables();
        TableDefinition.Setup();

        FileSelectionView = new(this);
        TableDataView = new(this);
        TableToolsView = new(this);
    }

    public void DrawEditorMenu()
    {
        if (ImGui.BeginMenu("File"))
        {
            // Save Table File
            if (ImGui.MenuItem($"Save Table File", KeyBindings.Current.CORE_SaveTable.HintText))
            {
                Warbox.ProjectHandler.WriteProjectConfig(Warbox.ProjectHandler.CurrentProject);
                Save();
            }
            UIHelper.ShowHoverTooltip("Save the current table file in its entirety.");

            // Save All Table Files
            if (ImGui.MenuItem($"Save All Table Files", KeyBindings.Current.CORE_SaveAllTables.HintText))
            {
                Warbox.ProjectHandler.WriteProjectConfig(Warbox.ProjectHandler.CurrentProject);
                SaveAll();
            }
            UIHelper.ShowHoverTooltip("Save all table files in their entirety.");

            // Save Patched Table File
            if (ImGui.MenuItem($"Save Patched Table File", KeyBindings.Current.CORE_SavePatchedTableFile.HintText))
            {
                Warbox.ProjectHandler.WriteProjectConfig(Warbox.ProjectHandler.CurrentProject);
                ExportPTF();
            }
            UIHelper.ShowHoverTooltip("Saves the current table file changes to its own patched table file for this project.");

            // Save All Patched Table Files
            /*
            if (ImGui.MenuItem($"Save All Patched Table Files", KeyBindings.Current.CORE_SaveAllPatchedTableFiles.HintText))
            {
                Warbox.ProjectHandler.WriteProjectConfig(Warbox.ProjectHandler.CurrentProject);
                ExportAllPTF();
            }
            UIHelper.ShowHoverTooltip("Saves the all table file changes to their own patched table files for this project.");
            */

            // Package All Tables
            if (ImGui.MenuItem($"Package All Tables", KeyBindings.Current.CORE_PackagePatchedTables.HintText))
            {
                Warbox.ProjectHandler.WriteProjectConfig(Warbox.ProjectHandler.CurrentProject);
                PackageAll();
            }
            UIHelper.ShowHoverTooltip("Saves the all table file changes to their own patched table files for this project.");


            // Package Patched Tables
            if (ImGui.MenuItem($"Package Patched Tables", KeyBindings.Current.CORE_PackagePatchedTables.HintText))
            {
                Warbox.ProjectHandler.WriteProjectConfig(Warbox.ProjectHandler.CurrentProject);
                PackagePTF();
            }
            UIHelper.ShowHoverTooltip("Saves the all table file changes to their own patched table files for this project.");

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

        ImGui.Separator();

        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem($"Window: Tools"))
            {
                CFG.Current.TableEditor_View_Window_Tools = !CFG.Current.TableEditor_View_Window_Tools;
            }
            UIHelper.ShowActiveStatus(CFG.Current.TableEditor_View_Window_Tools);

            ImGui.Separator();

            if (ImGui.MenuItem($"Properties: Display Names"))
            {
                CFG.Current.TableEditor_View_Properties_DisplayNames = !CFG.Current.TableEditor_View_Properties_DisplayNames;
            }
            UIHelper.ShowActiveStatus(CFG.Current.TableEditor_View_Properties_DisplayNames);

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
        TableDataView.Shortcuts();

        EditorCommandQueue(initcmd);

        FileSelectionView.Display();
        TableDataView.Display();

        if (CFG.Current.TableEditor_View_Window_Tools)
        {
            TableToolsView.Display();
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
    }

    public void OnProjectChanged()
    {
        ResetActionManager();
    }

    public void Save()
    {
        TableSaveHandler.Export();
    }

    public void SaveAll()
    {
        TableSaveHandler.ExportAll();
    }

    public void PackageAll()
    {
        TableSaveHandler.PackageAll();
    }

    public void ExportPTF()
    {
        TableSaveHandler.ExportPTF();
    }

    public void PackagePTF()
    {
        TableSaveHandler.PackagePTF();
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }

    public void Shortcuts()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_SaveTable))
        {
            Warbox.ProjectHandler.WriteProjectConfig(Warbox.ProjectHandler.CurrentProject);
            Save();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_SaveAllTables))
        {
            Warbox.ProjectHandler.WriteProjectConfig(Warbox.ProjectHandler.CurrentProject);
            SaveAll();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_SavePatchedTableFile))
        {
            Warbox.ProjectHandler.WriteProjectConfig(Warbox.ProjectHandler.CurrentProject);
            ExportPTF();
        }

        /*
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_SaveAllPatchedTableFiles))
        {
            Warbox.ProjectHandler.WriteProjectConfig(Warbox.ProjectHandler.CurrentProject);
            ExportAllPTF();
        }
        */

        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_PackagePatchedTables))
        {
            Warbox.ProjectHandler.WriteProjectConfig(Warbox.ProjectHandler.CurrentProject);
            PackagePTF();
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
        if (initcmd != null && initcmd[0] == "select")
        {
            if (initcmd.Length > 4)
            {
                var fileName = initcmd[1];
                var targetAttributeName = initcmd[2];
                var targetAttributeValue = initcmd[3];
                var targetIndex = initcmd[4];

                KeyValuePair<ResourceDescriptor, XDocument> targetEntry = new KeyValuePair<ResourceDescriptor, XDocument>();

                // Set file selection
                for (int i = 0; i < DataHandler.Tables.Count; i++)
                {
                    targetEntry = DataHandler.Tables.ElementAt(i);
                    var name = targetEntry.Key.Name;

                    if (name == fileName)
                    {
                        FileSelectionView.SetSelection(targetEntry);
                        break;
                    }
                }

                // Set row selection
                if (targetEntry.Key != null)
                {
                    var database = targetEntry.Value.Elements();
                    var classEntry = database.Elements();
                    var entries = classEntry.Elements().ToList();

                    for (int i = 0; i < entries.Count; i++)
                    {
                        if ($"{i}" == targetIndex)
                        {
                            var entry = entries[i];
                            TraverseAndSearch(entry, targetAttributeName, targetAttributeValue, i);
                        }
                        // Select the first if no row index is given
                        else if(targetIndex == "-1")
                        {
                            var entry = entries[i];
                            TraverseAndSearch(entry, targetAttributeName, targetAttributeValue, i);
                        }
                    }
                }
            }
        }
    }

    private void TraverseAndSearch(XElement entry, string targetAttributeName, string targetAttributeValue, int index)
    {
        SearchAttributes(entry, targetAttributeName, targetAttributeValue, index);

        foreach (var child in entry.Elements())
        {
            TraverseAndSearch(child, targetAttributeName, targetAttributeValue, index);
        }
    }

    private void SearchAttributes(XElement entry, string targetAttributeName, string targetAttributeValue, int index)
    {
        foreach (var attribute in entry.Attributes())
        {
            if ($"{attribute.Name}" == targetAttributeName && attribute.Value == targetAttributeValue)
            {
                var curTableView = TableDataView.GetSelectedTableView();
                if (curTableView != null)
                {
                    curTableView.SetRowSelection(index);
                }
            }
        }
    }
}
