using Assimp;
using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Editors.TableEditor.Actions;
using StudioCore.Editors.TextEditor;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StudioCore.Editors.TableEditor;

public class GenericTableView
{
    private TableEditorScreen Screen;
    private TableEditorState EditorState;

    private string SearchKeyText = "";
    private string SearchValueText = "";

    public string Name = "";
    private string ImGuiName = "";

    private string AliasNameKey = "";
    private string RowNameKey = "";

    private string rowName = "";
    private int rowIndex = -1;

    private bool selectRow = false;

    private bool NoPrimaryKey = false;

    private int childDepth = 0;

    public GenericTableView(TableEditorScreen screen, string name, string aliasNameKey, string rowNameKey, bool noPrimaryKey)
    {
        Screen = screen;
        EditorState = screen.EditorState;

        Name = name;
        ImGuiName = name;
        AliasNameKey = aliasNameKey;
        RowNameKey = rowNameKey;

        NoPrimaryKey = noPrimaryKey;
    }

    public void DisplayEntries()
    {
        var width = ImGui.GetWindowWidth();

        var currentDocument = EditorState.SelectedDocument;
        var elementList = EditorState.GetCurrentEntries();

        ImGui.SetNextItemWidth(width);
        ImGui.InputText($"##{ImGuiName}_KeySearchBar", ref SearchKeyText, 255);
        UIHelper.ShowHoverTooltip("Filters the list.");

        ImGui.Separator();

        ImGui.BeginChild($"{ImGuiName}Section");

        for (int i = 0; i < elementList.Count + 1; i++)
        {
            if (i < elementList.Count)
            {
                var entry = elementList[i];

                var key = $"{i}";
                var alias = "";

                if (!NoPrimaryKey)
                {
                    key = entry.Attribute(RowNameKey).Value;
                    
                    XAttribute aliasAttribute = null;
                    if (AliasNameKey != "")
                    {
                        aliasAttribute = entry.Attribute(AliasNameKey);
                        if (aliasAttribute != null)
                        {
                            alias = aliasAttribute.Value;
                        }
                    }
                }

                if (!TextSearchFilters.FilterTableRowEntry(entry, alias, SearchKeyText))
                {
                    continue;
                }

                if (ImGui.Selectable($"Entry: {key}##{ImGuiName}selectEntry{i}", 
                    key == rowName && rowIndex == i))
                {
                    rowName = key;
                    rowIndex = i;
                }

                // Arrow Selection
                if (ImGui.IsItemHovered() && selectRow)
                {
                    selectRow = false;
                    rowName = key;
                    rowIndex = i;
                }
                if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                {
                    selectRow = true;
                }

                if (alias != "")
                {
                    UIHelper.DisplayAlias(alias);
                }

                // Context
                if ($"{key}" == rowName)
                {
                    if (ImGui.BeginPopupContextItem($"##{ImGuiName}EntryContext{i}"))
                    {
                        if(ImGui.Selectable("Duplicate"))
                        {
                            DuplicateRow();
                        }

                        if (ImGui.Selectable("Remove"))
                        {
                            RemoveRow();
                        }

                        ImGui.EndPopup();
                    }
                }
            }
        }

        ImGui.EndChild();
    }

    public void DisplayProperties()
    {
        var width = ImGui.GetWindowWidth();

        ImGui.SetNextItemWidth(width);
        ImGui.InputText($"##{ImGuiName}_ValueSearchBar", ref SearchValueText, 255);
        UIHelper.ShowHoverTooltip("Filters the list.");

        ImGui.BeginChild($"{ImGuiName}PropertySection");

        var currentDocument = EditorState.SelectedDocument;
        var elementList = EditorState.GetCurrentEntries();

        if (rowIndex != -1 && elementList.Count > rowIndex)
        {
            var entry = elementList.ElementAt(rowIndex);

            if (entry != null)
            {
                //-------------------
                // Names
                //-------------------
                if (ImGui.BeginTable($"{ImGuiName}AttributeTable", 2, ImGuiTableFlags.SizingFixedFit))
                {
                    ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Inputs", ImGuiTableColumnFlags.WidthFixed);

                    childDepth = 0;
                    HandleElementEntry(currentDocument, entry, "root");

                    ImGui.EndTable();
                }
            }

            TableMeta.DisplayAttributeAddSection(currentDocument, entry);
        }

        ImGui.EndChild();
    }

    private void HandleElementEntry(XDocument currentDocument, XElement entry, string elementName)
    {
        DisplayHeaderRow(currentDocument, entry, elementName);

        var attributes = entry.Attributes().ToList();

        for (int i = 0; i < attributes.Count; i++)
        {
            var attribute = attributes[i];

            DisplayAttributeRow(currentDocument, entry, attribute, i, elementName);
            if(TableMeta.HasMetaData(currentDocument, entry, attribute, i, elementName))
            {
                DisplayMetaDataRow(currentDocument, entry, attribute, i, elementName);
            }
        }

        childDepth += 1;

        foreach (var child in entry.Elements().ToList())
        {
            ImGui.Indent();
            HandleElementEntry(currentDocument, child, child.Name.ToString());
            ImGui.Unindent();
        }
    }

    private void DisplayHeaderRow(XDocument currentDocument, XElement entry, string elementName)
    {
        var width = ImGui.GetWindowWidth();

        if (entry != null)
        {
            if (TextSearchFilters.FilterTableEntry(entry.Name.ToString(), SearchValueText))
            {
                ImGui.TableNextRow();

                // Name Column
                ImGui.TableSetColumnIndex(0);
                ImGui.AlignTextToFramePadding();

                var displayName = TableMeta.GetHeaderName(
                    EditorState,
                    "Name",
                    $"{entry.Name}");

                var description = TableMeta.GetHeaderName(
                    EditorState,
                    "Description",
                    $"{entry.Name}");

                ImGui.SetNextItemWidth(width * 0.25f);
                UIHelper.DisplayHeaderText(displayName);
                UIHelper.ShowHoverTooltip(description);

                // Inputs Column
                ImGui.TableSetColumnIndex(1);
            }
        }
    }

    private void DisplayAttributeRow(XDocument currentDocument, XElement entry, XAttribute attribute, int i, string elementName)
    {
        var width = ImGui.GetWindowWidth();

        if (attribute != null)
        {
            if (TextSearchFilters.FilterTableEntry(attribute.Value, SearchValueText))
            {
                ImGui.TableNextRow();

                // Name Column
                ImGui.TableSetColumnIndex(0);
                ImGui.AlignTextToFramePadding();

                var displayName = TableMeta.GetAttributeName(
                    EditorState,
                    "Name",
                    $"{entry.Name}",
                    $"{attribute.Name}");

                var description = TableMeta.GetAttributeName(
                    EditorState,
                    "Description",
                    $"{entry.Name}",
                    $"{attribute.Name}");

                ImGui.SetNextItemWidth(width * 0.25f);
                ImGui.Text(displayName);
                UIHelper.ShowHoverTooltip(description);

                // Inputs Column
                ImGui.TableSetColumnIndex(1);

                var oldValue = attribute.Value;
                var tValue = attribute.Value;
                var isChanged = false;

                ImGui.AlignTextToFramePadding();
                ImGui.SetNextItemWidth(width * 0.5f);

                // Handling for bool type
                if(TableMeta.IsBoolAttribute(EditorState,"IsBool",$"{entry.Name}",$"{attribute.Name}"))
                {
                    var tBool = false;

                    if (oldValue == "true")
                        tBool = true;

                    if (ImGui.Checkbox($"##{ImGuiName}_inputBool_{attribute.Name}{i}{elementName}{childDepth}", ref tBool))
                    {
                        isChanged = true;
                    }
                    if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
                    {
                        if (isChanged)
                        {
                            if (tBool)
                            {
                                var action = new ChangeAttributeValue(attribute, oldValue, "true");
                                Screen.EditorActionManager.ExecuteAction(action);
                            }
                            else
                            {
                                var action = new ChangeAttributeValue(attribute, oldValue, "false");
                                Screen.EditorActionManager.ExecuteAction(action);
                            }
                        }
                    }
                }
                // Handling for string type
                else
                {
                    if (ImGui.InputText($"##{ImGuiName}_input_{attribute.Name}{i}{elementName}{childDepth}", ref tValue, 255))
                    {
                        isChanged = true;
                    }
                    if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
                    {
                        if (isChanged)
                        {
                            var action = new ChangeAttributeValue(attribute, oldValue, tValue);
                            Screen.EditorActionManager.ExecuteAction(action);
                        }
                    }
                }
            }
        }
    }

    private void DisplayMetaDataRow(XDocument currentDocument, XElement entry, XAttribute attribute, int i, string elementName)
    {
        var width = ImGui.GetWindowWidth();

        if (attribute != null)
        {
            if (TextSearchFilters.FilterTableEntry(attribute.Value, SearchValueText))
            {
                ImGui.TableNextRow();

                // Name Column
                ImGui.TableSetColumnIndex(0);
                ImGui.AlignTextToFramePadding();

                // Name
                UIHelper.DisplayMetaText("Test");

                // Inputs Column
                ImGui.TableSetColumnIndex(1);
                ImGui.AlignTextToFramePadding();
                ImGui.SetNextItemWidth(width * 0.5f);

                // Input
                UIHelper.DisplayMetaText("Test");
            }
        }
    }

    public void Shortcuts()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DuplicateSelectedEntry))
        {
            DuplicateRow();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DeleteSelectedEntry))
        {
            RemoveRow();
        }
    }

    public void DuplicateRow()
    {
        var elementList = EditorState.GetCurrentEntries();
        var action = new AddTableRow(elementList, rowIndex);
        Screen.EditorActionManager.ExecuteAction(action);
    }

    public void RemoveRow()
    {
        var elementList = EditorState.GetCurrentEntries();
        var action = new RemoveTableRow(elementList, rowIndex);
        Screen.EditorActionManager.ExecuteAction(action);
    }
}
