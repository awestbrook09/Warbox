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
            // Save
            if (ImGui.MenuItem($"Save", KeyBindings.Current.CORE_Save.HintText))
            {
                Warbox.ProjectHandler.WriteProjectConfig(Warbox.ProjectHandler.CurrentProject);
                Save();
            }

            // Save PTF
            if (ImGui.MenuItem($"Save PTF", KeyBindings.Current.CORE_Save.HintText))
            {
                Warbox.ProjectHandler.WriteProjectConfig(Warbox.ProjectHandler.CurrentProject);
                SavePTF();
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
        var outputDir = $"{Warbox.ProjectDataRoot}\\Data";

        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        var status = FileSelectionView.GetSelectedDocumentStatus();
        var document = DataHandler.Tables[status];

        var writePath = status.Path;
        var fileDir = $"{outputDir}\\{writePath}";

        // If it is a project-specific file, use the status Path as it is a full path
        if (writePath.Contains(outputDir))
        {
            fileDir = $"{writePath}";
        }

        var fileOutputDir = Path.GetDirectoryName(fileDir);

        if (!Directory.Exists(fileOutputDir))
            Directory.CreateDirectory(fileOutputDir);

        document.Save(fileDir);

        TaskLogs.AddLog($"{fileDir} saved.");
    }

    public void SavePTF()
    {
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

        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_SavePTF))
        {
            Warbox.ProjectHandler.WriteProjectConfig(Warbox.ProjectHandler.CurrentProject);
            SavePTF();
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

                KeyValuePair<DataStatus, XDocument> targetEntry = new KeyValuePair<DataStatus, XDocument>();

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
