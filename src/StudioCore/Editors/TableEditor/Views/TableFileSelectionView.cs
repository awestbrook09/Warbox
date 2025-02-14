using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Core.Data;
using StudioCore.Editors.TableEditor.Framework;
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

    private ResourceDescriptor SelectedStatus;
    private XDocument SelectedDocument;

    private bool SelectNextTable = false;

    public bool focusRow = false;

    public TableFileSelectionView(TableEditorScreen screen)
    {
        Screen = screen;
    }

    public void Display()
    {
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

        if (Warbox.ProjectHandler.CurrentProject == null)
            return;
        
        var modName = ManifestHandler.SanitizeModName(Warbox.ProjectHandler.CurrentProject.Config.ProjectName);

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
                    if (name.Contains($"__{modName}"))
                        continue;

                    if (TextSearchFilters.FilterFileList(name, CFG.Current.TableEditor_FileFilterText))
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
            displayName = TableMeta.GetFileTitle("Name", $"{name}");
        }

        // Focus the newly selected row when set via command queue
        if (focusRow && SelectedStatus == status)
        {
            focusRow = false;
            SelectedStatus = status;
            SelectedDocument = entry.Value;
            ImGui.SetScrollHereY();
        }

        if (ImGui.Selectable($"{displayName}##tableFileEntry{name}{index}", SelectedStatus == status))
        {
            SetSelection(entry);
            Screen.TableDataView.RefreshTableViews();
        }

        // Arrow Selection
        if (ImGui.IsItemHovered() && SelectNextTable)
        {
            SetSelection(entry);
            Screen.TableDataView.RefreshTableViews();
        }
        if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
        {
            SelectNextTable = true;
        }

        // Context
        if (SelectedStatus == status)
        {
            if (ImGui.BeginPopupContextItem($"##tableFileEntryContext{index}"))
            {

                ImGui.EndPopup();
            }
        }
    }

    public void SetSelection(KeyValuePair<ResourceDescriptor, XDocument> entry, bool FocusRow = false)
    {
        SelectedStatus = entry.Key;
        SelectedDocument = entry.Value;
        focusRow = FocusRow;
    }

    public string GetSelectedDocumentName()
    {
        return SelectedStatus == null ? "" : SelectedStatus.Name;
    }

    public ResourceDescriptor GetSelectedDocumentStatus()
    {
        return SelectedStatus;
    }

    public XDocument GetSelectedDocument()
    {
        return SelectedDocument;
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
            foreach (var category in TableDefinition.Categories)
            {
                for (int i = 0; i < DataHandler.Tables.Count; i++)
                {
                    var entry = DataHandler.Tables.ElementAt(i);
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
