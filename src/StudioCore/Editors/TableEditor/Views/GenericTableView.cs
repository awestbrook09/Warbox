using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Core.Data;
using StudioCore.Editor;
using StudioCore.Editors.TableEditor.Actions;
using StudioCore.Editors.TableEditor.Framework;
using StudioCore.Editors.TableEditor.Tools;
using StudioCore.Editors.TextEditor.Framework;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace StudioCore.Editors.TableEditor.Views;

public class GenericTableView
{
    public TableEditorScreen Screen;

    public ResourceDescriptor TableDescriptor;
    public XDocument TableDocument;

    public string Name = "";
    private string ImGuiName = "";
    private string AliasNameKey = "";

    private bool NoPrimaryKey = false;
    private int TrackedDepth = 0;

    public bool SetupAliasOverrides = false;
    public Dictionary<int, string> AliasOverrides = new Dictionary<int, string>();

    // Row
    public int RowSelectionIndex = -1;

    private bool RowArrowSelect = false;
    private bool FocusRowSelection = false;

    public GenericTableView(TableEditorScreen screen, string name, string aliasNameKey, bool noPrimaryKey, ResourceDescriptor viewStatus, XDocument viewDocument)
    {
        Screen = screen;

        Name = name;
        ImGuiName = name;
        AliasNameKey = aliasNameKey;

        NoPrimaryKey = noPrimaryKey;

        TableDescriptor = viewStatus;
        TableDocument = viewDocument;
    }

    public void SelectRow(XElement entry, int index)
    {
        RowSelectionIndex = index;
    }

    public void ClearRowSelection()
    {
        RowSelectionIndex = -1;
    }

    public int GetLastRowIndex()
    {
        var count = TableDocument.Elements().Elements().Elements().Count();
        return count - 1;
    }

    public XElement GetRowAtIndex(int index)
    {
        return TableDocument.Elements().Elements().Elements().ElementAt(index);
    }

    public XElement GetRowContainer()
    {
        return TableDocument.Elements().Elements().FirstOrDefault();
    }

    public IEnumerable<XElement> GetRows()
    {
        return TableDocument.Elements().Elements().Elements();
    }

    public List<XElement> GetRowList()
    {
        return TableDocument.Elements().Elements().Elements().ToList();
    }

    /// <summary>
    /// Handles the top-level display of the row selection window
    /// </summary>
    public void DisplayEntries()
    {
        var width = ImGui.GetWindowWidth();

        if (!SetupAliasOverrides)
        {
            SetupAliasOverrides = true;
            TableRowDecorators.ProcessAliasOverrides(this);
        }

        ImGui.SetNextItemWidth(width);
        ImGui.InputText($"##{ImGuiName}_KeySearchBar", ref CFG.Current.TableEditor_RowFilterText, 255);
        UIHelper.ShowHoverTooltip($"Filters the list.\n\n{TableSearchFilters.SearchCommandsHint}");

        ImGui.Separator();

        ImGui.BeginChild($"{ImGuiName}Section");

        int index = 0;
        foreach(var element in GetRows())
        {
            var entry = element;

            var key = $"{index}";
            var alias = "";

            if (!NoPrimaryKey)
            {
                XAttribute aliasAttribute = null;
                if (AliasNameKey != "")
                {
                    aliasAttribute = entry.Attribute(AliasNameKey);
                    if (aliasAttribute != null)
                    {
                        alias = aliasAttribute.Value;
                    }
                }

                if (AliasOverrides.ContainsKey(index))
                {
                    alias = AliasOverrides[index];
                }
            }

            if (!TableSearchFilters.FilterTableRowEntry(entry, alias, CFG.Current.TableEditor_RowFilterText))
            {
                index++;
                continue;
            }

            // Focus the newly selected row when set via command queue
            if (FocusRowSelection && index == RowSelectionIndex)
            {
                FocusRowSelection = false;
                SelectRow(entry, index);
                ImGui.SetScrollHereY();
            }

            if (ImGui.Selectable($"Entry: {key}##{ImGuiName}selectEntry{index}", RowSelectionIndex == index))
            {
                SelectRow(entry, index);
            }

            // Arrow Selection
            if (ImGui.IsItemHovered() && RowArrowSelect)
            {
                RowArrowSelect = false;
                SelectRow(entry, index);
            }
            if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
            {
                RowArrowSelect = true;
            }

            if (alias != "")
            {
                UIHelper.DisplayAlias(alias);
            }

            // Context
            if (RowSelectionIndex == index)
            {
                if (ImGui.BeginPopupContextItem($"##{ImGuiName}EntryContext{index}"))
                {
                    if (ImGui.Selectable("Duplicate"))
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

            index++;
        }

        ImGui.EndChild();
    }

    /// <summary>
    /// Refreshes the current state of the view
    /// </summary>
    public void Refresh()
    {
        TableRowDecorators.ProcessAliasOverrides(this);
        TableMetaDecorators.Reset();
        
        //TableDifferenceEngine.Refresh();
    }

    /// <summary>
    /// Sets the current row selection
    /// </summary>
    public void SetRowSelection(int index)
    {
        RowSelectionIndex = index;
        FocusRowSelection = true;
    }

    /// <summary>
    /// Handles the top-level display of the properties window
    /// </summary>
    public void DisplayProperties()
    {
        var width = ImGui.GetWindowWidth();

        ImGui.SetNextItemWidth(width);
        ImGui.InputText($"##{ImGuiName}_ValueSearchBar", ref CFG.Current.TableEditor_PropertyFilterText, 255);
        UIHelper.ShowHoverTooltip("Filters the list.");

        ImGui.BeginChild($"{ImGuiName}PropertySection");

        int index = 0;
        foreach(var element in GetRows())
        {
            if(index == RowSelectionIndex)
            {
                if (ImGui.BeginTable($"{ImGuiName}AttributeTable", 2, ImGuiTableFlags.SizingFixedFit))
                {
                    ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Inputs", ImGuiTableColumnFlags.WidthFixed);

                    TrackedDepth = 0;
                    HandleElementEntry(element, "root", RowSelectionIndex);

                    ImGui.EndTable();
                }

                if (!TableMetaHandler.CheckMetaToggle("SuppressAdditionButtons", TableDescriptor.Name))
                {
                    DisplayMissingElementOptions(element);
                }
            }

            index++;
        }

        ImGui.EndChild();
    }

    /// <summary>
    /// Handle the shortcuts for this view
    /// </summary>
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

    /// <summary>
    /// Handle the overall display of the selected entry in the properties window
    /// </summary>
    private void HandleElementEntry(XElement entry, string imguiElementName, int rowIndex)
    {
        var attributes = entry.Attributes().ToList();

        // Attribute data
        if (attributes.Count > 0)
        {
            DisplayHeaderRow(entry, imguiElementName, rowIndex);

            for (int i = 0; i < attributes.Count; i++)
            {
                var attribute = attributes[i];

                DisplayAttributeRow(entry, attribute, i, imguiElementName, rowIndex);

                if (TableMetaDecorators.HasRowDecorators(entry, attribute))
                {
                    DisplayMetaDataRow(entry, attribute, i, imguiElementName, rowIndex);
                }
            }
        }
        // Element data - Show header only if it contains children
        else if (entry.Value != "" && entry.Descendants().Count() > 0)
        {
            DisplayHeaderRow(entry, imguiElementName, rowIndex);
        }
        // Element data - Show data if the element is a child element
        else if(entry.Value != "" && entry.Descendants().Count() == 0)
        {
            DisplayElementRow(entry, imguiElementName, rowIndex);
        }
        TrackedDepth += 1;

        foreach (var child in entry.Elements().ToList())
        {
            ImGui.Indent();
            HandleElementEntry(child, child.Name.ToString(), rowIndex);
            ImGui.Unindent();
        }
    }

    /// <summary>
    /// Handle the display of the header rows
    /// </summary>
    private void DisplayHeaderRow(XElement entry, string imguiElementName, int rowIndex)
    {
        var width = ImGui.GetWindowWidth();

        if (entry != null)
        {
            if (TableSearchFilters.FilterTableEntry(entry.Name.ToString(), CFG.Current.TableEditor_PropertyFilterText))
            {
                ImGui.TableNextRow();

                // Name Column
                ImGui.TableSetColumnIndex(0);
                ImGui.AlignTextToFramePadding();

                var displayName = entry.Name.ToString();

                if (CFG.Current.TableEditor_View_Properties_DisplayNames)
                {
                    displayName = TableMetaHandler.GetElementNameValue(
                    "Name",
                    $"{entry.Name}");
                }

                var description = TableMetaHandler.GetElementNameValue(
                    "Description",
                    $"{entry.Name}");

                ImGui.SetNextItemWidth(width * 0.25f);
                UIHelper.DisplayHeaderText(displayName);
                UIHelper.ShowHoverTooltip(description);

                if (ImGui.BeginPopupContextItem($"####{ImGuiName}_contextMenu_{imguiElementName}{TrackedDepth}"))
                {
                    if (ImGui.Selectable("Copy Header Name"))
                    {
                        PlatformUtils.Instance.SetClipboardText(entry.Name.ToString());
                    }

                    ImGui.EndPopup();
                }

                // Inputs Column
                ImGui.TableSetColumnIndex(1);
            }
        }
    }

    /// <summary>
    /// Handle the display of the element rows
    /// </summary>
    private void DisplayElementRow(XElement entry, string imguiElementName, int rowIndex)
    {
        var width = ImGui.GetWindowWidth();

        if (entry != null)
        {
            if (TableSearchFilters.FilterTableEntry(entry.Value, CFG.Current.TableEditor_PropertyFilterText))
            {
                ImGui.TableNextRow();

                // Name Column
                ImGui.TableSetColumnIndex(0);
                ImGui.AlignTextToFramePadding();

                var displayName = entry.Name.ToString();

                if (CFG.Current.TableEditor_View_Properties_DisplayNames)
                {
                    displayName = TableMetaHandler.GetAttributeNameValue(
                        "Name",
                        $"{entry.Name}",
                        $"{entry.Name}");
                }

                var description = TableMetaHandler.GetAttributeNameValue(
                    "Description",
                    $"{entry.Name}",
                    $"{entry.Name}");

                ImGui.SetNextItemWidth(width * 0.25f);
                UIHelper.DisplayHeaderText(displayName);
                UIHelper.ShowHoverTooltip(description);

                if (ImGui.BeginPopupContextItem($"####{ImGuiName}_contextMenu_{entry.Name}{imguiElementName}{TrackedDepth}"))
                {
                    if (ImGui.Selectable("Copy Property Name"))
                    {
                        PlatformUtils.Instance.SetClipboardText(entry.Name.ToString());
                    }

                    ImGui.EndPopup();
                }

                // Inputs Column
                ImGui.TableSetColumnIndex(1);

                var oldValue = entry.Value;
                var tValue = entry.Value;
                var isChanged = false;

                ImGui.AlignTextToFramePadding();
                ImGui.SetNextItemWidth(width * 0.5f);

                if (ImGui.InputText($"##{ImGuiName}_input_{entry.Name}{imguiElementName}{TrackedDepth}", ref tValue, 255))
                {
                    isChanged = true;
                }
                if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
                {
                    if (isChanged)
                    {
                        var action = new ChangeElementValue(entry, oldValue, tValue, this);
                        Screen.EditorActionManager.ExecuteAction(action);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Handle the display of the attribute rows
    /// </summary>
    private void DisplayAttributeRow(XElement entry, XAttribute attribute, int attributeIndex, string imguiElementName, int rowIndex)
    {
        var width = ImGui.GetWindowWidth();

        if (attribute != null)
        {
            if (TableSearchFilters.FilterTableEntry(attribute.Value, CFG.Current.TableEditor_PropertyFilterText))
            {
                ImGui.TableNextRow();

                // Name Column
                ImGui.TableSetColumnIndex(0);
                ImGui.AlignTextToFramePadding();

                var displayName = attribute.Name.ToString();

                if (CFG.Current.TableEditor_View_Properties_DisplayNames)
                {
                    displayName = TableMetaHandler.GetAttributeNameValue(
                        "Name",
                        $"{entry.Name}",
                        $"{attribute.Name}");
                }

                var description = TableMetaHandler.GetAttributeNameValue(
                    "Description",
                    $"{entry.Name}",
                    $"{attribute.Name}");

                ImGui.SetNextItemWidth(width * 0.25f);
                ImGui.Text(displayName);
                UIHelper.ShowHoverTooltip(description);

                if (ImGui.BeginPopupContextItem($"####{ImGuiName}_contextMenu_{attribute.Name}{imguiElementName}{TrackedDepth}"))
                {
                    if (ImGui.Selectable("Copy Property Name"))
                    {
                        PlatformUtils.Instance.SetClipboardText(attribute.Name.ToString());
                    }

                    ImGui.EndPopup();
                }

                // Inputs Column
                ImGui.TableSetColumnIndex(1);

                var oldValue = attribute.Value;
                var tValue = attribute.Value;
                var isChanged = false;

                ImGui.AlignTextToFramePadding();
                ImGui.SetNextItemWidth(width * 0.5f);

                // Handling for bool type
                if (TableMetaHandler.IsBoolAttribute("IsBool", $"{entry.Name}", $"{attribute.Name}"))
                {
                    var tBool = false;

                    if (oldValue == "true")
                        tBool = true;

                    if (ImGui.Checkbox($"##{ImGuiName}_inputBool_{attribute.Name}{attributeIndex}{imguiElementName}{TrackedDepth}", ref tBool))
                    {
                        isChanged = true;
                    }
                    if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
                    {
                        if (isChanged)
                        {
                            if (tBool)
                            {
                                var action = new ChangeAttributeValue(attribute, oldValue, "true", this);
                                Screen.EditorActionManager.ExecuteAction(action);
                            }
                            else
                            {
                                var action = new ChangeAttributeValue(attribute, oldValue, "false", this);
                                Screen.EditorActionManager.ExecuteAction(action);
                            }
                        }
                    }
                }
                // Handling for string type
                else
                {
                    if (ImGui.InputText($"##{ImGuiName}_input_{attribute.Name}{attributeIndex}{imguiElementName}{TrackedDepth}", ref tValue, 255))
                    {
                        isChanged = true;
                    }
                    if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
                    {
                        if (isChanged)
                        {
                            var action = new ChangeAttributeValue(attribute, oldValue, tValue, this);
                            Screen.EditorActionManager.ExecuteAction(action);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Handle the display of missing property addition buttons
    /// </summary>
    private void DisplayMissingElementOptions(XElement entry)
    {
        var allAttributes = TableMetaHandler.GetAttributeList(entry);
        var curAttributes = entry.Attributes();

        var missingAttributes = new List<XElement>();

        foreach (var aAttribute in allAttributes)
        {
            var isMissing = true;

            foreach (var cAttribute in curAttributes)
            {
                if (aAttribute.Name == cAttribute.Name)
                {
                    isMissing = false;
                    break;
                }
            }

            if (isMissing)
            {
                missingAttributes.Add(aAttribute);
            }
        }

        foreach (var attrEntry in missingAttributes)
        {
            ImGui.AlignTextToFramePadding();
            if (ImGui.Button($"{ForkAwesome.Plus}"))
            {

            }
            UIHelper.ShowHoverTooltip("Add this property as it is not currently present.");

            ImGui.SameLine();

            var displayName = attrEntry.Name.ToString();

            if (CFG.Current.TableEditor_View_Properties_DisplayNames)
            {
                displayName = TableMetaHandler.GetAttributeNameValue(
                "Name",
                $"{entry.Name}",
                $"{attrEntry.Name}");
            }

            ImGui.AlignTextToFramePadding();
            UIHelper.DisplayActionText($"{displayName}");
        }
    }


    /// <summary>
    /// Handle the display of the meta text rows
    /// </summary>
    private void DisplayMetaDataRow(XElement entry, XAttribute attribute, int attributeIndex, string imguiElementName, int rowIndex)
    {
        var width = ImGui.GetWindowWidth();

        if (attribute != null)
        {
            if (TableSearchFilters.FilterTableEntry(attribute.Value, CFG.Current.TableEditor_PropertyFilterText))
            {
                var elementName = entry.Name.ToString();
                var attributeName = attribute.Name.ToString();
                var documentName = TableMetaHandler.GetDocumentName(elementName);

                var metaDoc = TableMetaHandler.GetMetaDocument(documentName);
                List<XElement> elements = metaDoc.Descendants($"{attributeName}").ToList();

                ImGui.TableNextRow();

                // Name Column
                ImGui.TableSetColumnIndex(0);
                ImGui.AlignTextToFramePadding();

                // Inputs Column
                ImGui.TableSetColumnIndex(1);
                ImGui.AlignTextToFramePadding();
                ImGui.SetNextItemWidth(width * 0.5f);

                // Input
                for(int i = 0; i < elements.Count; i++)
                {
                    var curElement = elements[i];
                    var curImguiKey = $"{imguiElementName}{rowIndex}{attributeIndex}{i}";

                    TableMetaDecorators.HandleRowDecorators(
                        entry, curElement,
                        ImGuiName,
                        TrackedDepth,
                        attribute,
                        curImguiKey,
                        this);
                }
            }
        }
    }


    /// <summary>
    /// Duplicate the currently selected row
    /// </summary>
    public void DuplicateRow()
    {
        if (RowSelectionIndex == -1)
            return;

        var action = new AddTableRow(this);
        Screen.EditorActionManager.ExecuteAction(action);
    }

    /// <summary>
    /// Remove the currently selected row
    /// </summary>
    public void RemoveRow()
    {
        if (RowSelectionIndex == -1)
            return;

        var rowList = GetRowList();
        var curIndex = RowSelectionIndex;

        if (rowList.Count > 0)
        {
            var action = new RemoveTableRow(this);
            Screen.EditorActionManager.ExecuteAction(action);

            if (curIndex > 0)
            {
                var prevEntry = rowList[curIndex - 1];

                SelectRow(prevEntry, curIndex - 1);
            }
        }
    }

}
