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
        FileSelectionView = new(this);
        TableDataView = new(this);
        TableToolsView = new(this);
    }

    public void DrawEditorMenu()
    {
        // Save
        if (ImGui.BeginMenu("Save"))
        {
            if (ImGui.MenuItem($"Save Table", KeyBindings.Current.CORE_SaveTable.HintText))
            {
                Warbox.Project.UpdateProjectJSON();
                Save();
            }
            UIHelper.ShowHoverTooltip("Save the current table file in its entirety.");

            if (ImGui.MenuItem($"Save All Tables", KeyBindings.Current.CORE_SaveAllTables.HintText))
            {
                Warbox.Project.UpdateProjectJSON();
                SaveAll();
            }
            UIHelper.ShowHoverTooltip("Save all table files in their entirety.");

            if (ImGui.MenuItem($"Save Patched Table", KeyBindings.Current.CORE_SavePatchedTableFile.HintText))
            {
                Warbox.Project.UpdateProjectJSON();
                ExportPTF();
            }
            UIHelper.ShowHoverTooltip("Saves the current table file changes to its own patched table file for this project.");

            ImGui.EndMenu();
        }

        ImGui.Separator();

        // Package
        if (ImGui.BeginMenu("Package"))
        {
            if (ImGui.MenuItem($"Package Tables", KeyBindings.Current.CORE_PackagePatchedTables.HintText))
            {
                Warbox.Project.UpdateProjectJSON();
                PackageAll();
            }
            UIHelper.ShowHoverTooltip("Saves the all table file changes to their own patched table files for this project.");

            if (ImGui.MenuItem($"Package Patched Tables", KeyBindings.Current.CORE_PackagePatchedTables.HintText))
            {
                Warbox.Project.UpdateProjectJSON();
                PackagePTF();
            }
            UIHelper.ShowHoverTooltip("Saves the all table file changes to their own patched table files for this project.");


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

        ImGui.Separator();

        // Views
        if (ImGui.BeginMenu("Views"))
        {
            if (ImGui.MenuItem($"Tools"))
            {
                CFG.Current.TableEditor_View_Window_Tools = !CFG.Current.TableEditor_View_Window_Tools;
            }
            UIHelper.ShowActiveStatus(CFG.Current.TableEditor_View_Window_Tools);

            ImGui.EndMenu();
        }

        ImGui.Separator();

        // Toggles
        if (ImGui.BeginMenu("Toggles"))
        {
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
        TableSelection.ClearFileSelection();

        ResetActionManager();
    }

    public void Save()
    {
        TableDataHandler.Export();
    }

    public void SaveAll()
    {
        TableDataHandler.ExportAll();
    }

    public void PackageAll()
    {
        TableDataHandler.PackageAll();
    }

    public void ExportPTF()
    {
        TableDataHandler.ExportPTF();
    }

    public void PackagePTF()
    {
        TableDataHandler.PackagePTF();
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }

    public void Shortcuts()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_SaveTable))
        {
            Warbox.Project.UpdateProjectJSON();
            Save();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_SaveAllTables))
        {
            Warbox.Project.UpdateProjectJSON();
            SaveAll();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_SavePatchedTableFile))
        {
            Warbox.Project.UpdateProjectJSON();
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
            Warbox.Project.UpdateProjectJSON();
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
                for (int i = 0; i < TableDataHandler.Tables.Count; i++)
                {
                    targetEntry = TableDataHandler.Tables.ElementAt(i);
                    var name = targetEntry.Key.Name;

                    if (name == fileName)
                    {
                        TableSelection.SelectFile(targetEntry.Key, targetEntry.Value, i);
                        TableSelection.FocusFileSelection = true;
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
