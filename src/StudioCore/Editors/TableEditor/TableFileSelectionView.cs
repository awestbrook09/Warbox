using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Core.Data;
using StudioCore.Editors.TextEditor;
using StudioCore.Interface;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace StudioCore.Editors.TableEditor;

public class TableFileSelectionView
{
    private TableEditorScreen Screen;
    private TableEditorState EditorState;

    private string SearchText = "";

    public TableFileSelectionView(TableEditorScreen screen)
    {
        Screen = screen;
        EditorState = screen.EditorState;
    }

    public void Display()
    {
        SetupCategoryLists();

        var width = ImGui.GetWindowWidth();

        if (ImGui.Begin("Files##tableFileView"))
        {
            ImGui.SetNextItemWidth(width * 0.75f);
            ImGui.InputText($"##tableFileSearchBar", ref SearchText, 255);
            UIHelper.ShowHoverTooltip("Filters the list.");

            ImGui.BeginChild("tableListSection");

            DisplayCategories();

            ImGui.EndChild();

            ImGui.End();
        }
    }

    private void DisplayCategories()
    {
        ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.DefaultOpen;

        foreach (var entry in CategoryLists)
        {
            var category = entry.Key;
            var entries = entry.Value;

            if (ImGui.CollapsingHeader(category, flags))
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    var selectionRow = entries[i];
                    var name = selectionRow.Key.Name;

                    if (TextSearchFilters.FilterFileList(name, SearchText))
                    {
                        SelectionRow(i, selectionRow);
                    }
                }
            }
        }
    }

    private void SelectionRow(int index, KeyValuePair<DataStatus, XDocument> entry)
    {
        var status = entry.Key;
        var name = entry.Key.Name;

        var displayName = name;

        // Typically the name of the file is one of the headers, so just do this
        if (CFG.Current.TableEditor_View_Properties_DisplayNames)
        {
            displayName = TableMeta.GetFileTitle(EditorState, "Name", $"{name}");
        }

        if (ImGui.Selectable($"{displayName}##tableFileEntry{name}{index}", EditorState.SelectedStatus == status))
        {
            EditorState.InvalidateState();
            EditorState.UpdateSelection(entry);
        }

        // Arrow Selection
        if (ImGui.IsItemHovered() && EditorState.SelectNextTable)
        {
            EditorState.SelectNextTable = false;
            EditorState.InvalidateState();
            EditorState.UpdateSelection(entry);
        }
        if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
        {
            EditorState.SelectNextTable = true;
        }

        // Context
        if (EditorState.SelectedStatus == status)
        {
            if (ImGui.BeginPopupContextItem($"##tableFileEntryContext{index}"))
            {

                ImGui.EndPopup();
            }
        }
    }

    public void Shortcuts()
    {
    }

    private SortedDictionary<string, List<KeyValuePair<DataStatus, XDocument>>> CategoryLists = new();

    private void SetupCategoryLists()
    {
        if (CategoryLists.Count < 1)
        {
            // Build categories
            foreach (var category in TableDefinition.Categories)
            {
                for (int i = 0; i < Warbox.DataHandler.Tables.Count; i++)
                {
                    var entry = Warbox.DataHandler.Tables.ElementAt(i);
                    var status = entry.Key;
                    var name = entry.Key.Name;

                    if (IsPartOfCategory(name, category))
                    {
                        if (CategoryLists.ContainsKey(category))
                        {
                            CategoryLists[category].Add(entry);
                        }
                        else
                        {
                            CategoryLists.Add(category, new List<KeyValuePair<DataStatus, XDocument>>()
                        {
                            entry
                        });
                        }
                    }
                }
            }
        }
    }

    private Dictionary<string, string> CategoryEvaluations = new();

    private bool IsPartOfCategory(string name, string category)
    {
        if (name.Contains("__"))
        {
            name = name.Split("__")[0];
        }

        if (CategoryEvaluations.ContainsKey(name))
        {
            if (CategoryEvaluations[name] == category)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            var tableEntry = TableDefinition.Definitions.Where(e => e.Attribute("Name").Value == name).FirstOrDefault();

            if (tableEntry != null && tableEntry.Attribute("Category") != null)
            {
                if (tableEntry.Attribute("Category").Value == category)
                {
                    CategoryEvaluations.Add(name, category);

                    return true;
                }
            }
        }

        return false;
    }


}
