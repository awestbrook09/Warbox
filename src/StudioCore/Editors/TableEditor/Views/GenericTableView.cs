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

    public ResourceDescriptor ViewStatus;
    public XDocument ViewDocument;

    public string Name = "";
    private string ImGuiName = "";

    private string AliasNameKey = "";

    public int CurrentRowIndex = -1;

    private bool selectRow = false;
    private bool focusRow = false;

    private bool NoPrimaryKey = false;
    private int childDepth = 0;

    public bool SetupAliasOverrides = false;
    public Dictionary<int, string> AliasOverrides = new Dictionary<int, string>();

    public GenericTableView(TableEditorScreen screen, string name, string aliasNameKey, bool noPrimaryKey, ResourceDescriptor viewStatus, XDocument viewDocument)
    {
        Screen = screen;

        Name = name;
        ImGuiName = name;
        AliasNameKey = aliasNameKey;

        NoPrimaryKey = noPrimaryKey;

        ViewStatus = viewStatus;
        ViewDocument = viewDocument;
    }

    public XElement GetNextRow()
    {
        var curRow = GetCurrentRow();

        if (curRow == null)
        {
            return null;
        }

        return ViewDocument.Elements().Elements().Elements().ElementAt(CurrentRowIndex).ElementsAfterSelf().FirstOrDefault();
    }

    public XElement GetPreviousRow()
    {
        var curRow = GetCurrentRow();

        if (curRow == null)
        {
            return null;
        }

        return ViewDocument.Elements().Elements().Elements().ElementAt(CurrentRowIndex).ElementsBeforeSelf().LastOrDefault();
    }

    public XElement GetCurrentRow()
    {
        return ViewDocument.Elements().Elements().Elements().ElementAt(CurrentRowIndex);
    }

    public XElement GetContainer()
    {
        return ViewDocument.Elements().Elements().FirstOrDefault();
    }

    public IEnumerable<XElement> GetContents()
    {
        return ViewDocument.Elements().Elements().Elements();
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
        UIHelper.ShowHoverTooltip($"Filters the list.\n\n{TextSearchFilters.SearchCommandsHint}");

        ImGui.Separator();

        ImGui.BeginChild($"{ImGuiName}Section");

        int index = 0;
        foreach(var element in GetContents())
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

            if (!TextSearchFilters.FilterTableRowEntry(entry, alias, CFG.Current.TableEditor_RowFilterText))
            {
                continue;
            }

            // Focus the newly selected row when set via command queue
            if (focusRow && index == CurrentRowIndex)
            {
                focusRow = false;
                CurrentRowIndex = index;
                ImGui.SetScrollHereY();
            }

            if (ImGui.Selectable($"Entry: {key}##{ImGuiName}selectEntry{index}", CurrentRowIndex == index))
            {
                CurrentRowIndex = index;
            }

            // Arrow Selection
            if (ImGui.IsItemHovered() && selectRow)
            {
                selectRow = false;
                CurrentRowIndex = index;
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
            if (CurrentRowIndex == index)
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
        CurrentRowIndex = index;
        focusRow = true;
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
        foreach(var element in GetContents())
        {
            if(index == CurrentRowIndex)
            {
                if (ImGui.BeginTable($"{ImGuiName}AttributeTable", 2, ImGuiTableFlags.SizingFixedFit))
                {
                    ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Inputs", ImGuiTableColumnFlags.WidthFixed);

                    childDepth = 0;
                    HandleElementEntry(element, "root", CurrentRowIndex);

                    ImGui.EndTable();
                }

                if (!TableMetaHandler.CheckMetaToggle("SuppressAdditionButtons", ViewStatus.Name))
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
        childDepth += 1;

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
            if (TextSearchFilters.FilterTableEntry(entry.Name.ToString(), CFG.Current.TableEditor_PropertyFilterText))
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

                if (ImGui.BeginPopupContextItem($"####{ImGuiName}_contextMenu_{imguiElementName}{childDepth}"))
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
            if (TextSearchFilters.FilterTableEntry(entry.Value, CFG.Current.TableEditor_PropertyFilterText))
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

                if (ImGui.BeginPopupContextItem($"####{ImGuiName}_contextMenu_{entry.Name}{imguiElementName}{childDepth}"))
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

                if (ImGui.InputText($"##{ImGuiName}_input_{entry.Name}{imguiElementName}{childDepth}", ref tValue, 255))
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
            if (TextSearchFilters.FilterTableEntry(attribute.Value, CFG.Current.TableEditor_PropertyFilterText))
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

                if (ImGui.BeginPopupContextItem($"####{ImGuiName}_contextMenu_{attribute.Name}{imguiElementName}{childDepth}"))
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

                    if (ImGui.Checkbox($"##{ImGuiName}_inputBool_{attribute.Name}{attributeIndex}{imguiElementName}{childDepth}", ref tBool))
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
                    if (ImGui.InputText($"##{ImGuiName}_input_{attribute.Name}{attributeIndex}{imguiElementName}{childDepth}", ref tValue, 255))
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
            if (TextSearchFilters.FilterTableEntry(attribute.Value, CFG.Current.TableEditor_PropertyFilterText))
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
                        childDepth,
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
        var action = new AddTableRow(this);
        Screen.EditorActionManager.ExecuteAction(action);
    }

    /// <summary>
    /// Remove the currently selected row
    /// </summary>
    public void RemoveRow()
    {
        var action = new RemoveTableRow(this);
        Screen.EditorActionManager.ExecuteAction(action);
    }

}
