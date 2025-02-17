using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Core.Data;
using StudioCore.Editors.TableEditor.Framework;
using StudioCore.Editors.TableEditor.Tools;
using StudioCore.Editors.TextEditor.Framework;
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

namespace StudioCore.Editors.TableEditor.Views;

public class TableFileSelectionView
{
    private TableEditorScreen Screen;

    public TableFileSelectionView(TableEditorScreen screen)
    {
        Screen = screen;
    }

    public void Display()
    {
        if (!Warbox.Project.IsValid())
            return;

        SetupCategoryLists();

        var width = ImGui.GetWindowWidth();

        if (ImGui.Begin("Files##tableFileView"))
        {
            ImGui.SetNextItemWidth(width * 0.75f);
            ImGui.InputText($"##tableFileSearchBar", ref CFG.Current.TableEditor_FileFilterText, 255);
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

                    // Ignore PTF files that are exported for this mod
                    if (name.Contains($"__{Warbox.Project.ProjectID}"))
                        continue;

                    if (TableSearchFilters.FilterFileList(name, CFG.Current.TableEditor_FileFilterText))
                    {
                        SelectionRow(i, selectionRow);
                    }
                }
            }
        }
    }

    private void SelectionRow(int index, KeyValuePair<ResourceDescriptor, XDocument> entry)
    {
        var status = entry.Key;
        var name = entry.Key.Name;

        var displayName = name;

        // Typically the name of the file is one of the headers, so just do this
        if (CFG.Current.TableEditor_View_Properties_DisplayNames)
        {
            displayName = TableMetaHandler.GetFileTitle("Name", $"{name}");
        }

        // Focus the newly selected row when set via command queue
        if (TableSelection.FocusFileSelection && TableSelection.FileSelectionDescriptor == status)
        {
            TableSelection.SelectFile(entry.Key, entry.Value, index);
            ImGui.SetScrollHereY();
        }

        if (ImGui.Selectable($"{displayName}##tableFileEntry{name}{index}", TableSelection.FileSelectionDescriptor == status))
        {
            TableSelection.SelectFile(entry.Key, entry.Value, index);
            Screen.TableDataView.RefreshTableViews();
        }

        // Modified tag
        /*
        var curTableView = Screen.TableDataView.GetSpecificTableView(name);
        if(curTableView != null)
        {
            if(TableDifferenceEngine.TableDifferenceCache.ContainsKey(curTableView))
            {
                var isModified = TableDifferenceEngine.TableDifferenceCache[curTableView];

                if(isModified)
                {
                    UIHelper.DisplayAlias("MODIFIED");
                }
            }
        }
        */

        // Arrow Selection
        if (ImGui.IsItemHovered() && TableSelection.FileArrowSelect)
        {
            TableSelection.SelectFile(entry.Key, entry.Value, index);
            Screen.TableDataView.RefreshTableViews();
        }
        if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
        {
            TableSelection.FileArrowSelect = true;
        }

        // Context
        if (TableSelection.FileSelectionDescriptor == status)
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

    private SortedDictionary<string, List<KeyValuePair<ResourceDescriptor, XDocument>>> CategoryLists = new();

    private void SetupCategoryLists()
    {
        if (CategoryLists.Count < 1)
        {
            // Build categories
            foreach (var category in TableMetaHandler.TableCategories)
            {
                for (int i = 0; i < TableDataHandler.Tables.Count; i++)
                {
                    var entry = TableDataHandler.Tables.ElementAt(i);
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
                            CategoryLists.Add(category, new List<KeyValuePair<ResourceDescriptor, XDocument>>()
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
            var tableEntry = TableMetaHandler.TableMetaDefinition.Where(e => e.Attribute("Name").Value == name).FirstOrDefault();

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
