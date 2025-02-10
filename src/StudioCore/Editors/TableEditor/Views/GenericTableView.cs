using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Core.Data;
using StudioCore.Editor;
using StudioCore.Editors.TableEditor.Actions;
using StudioCore.Editors.TableEditor.Framework;
using StudioCore.Editors.TextEditor.Framework;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Xml.Linq;

namespace StudioCore.Editors.TableEditor.Views;

public class GenericTableView
{
    private TableEditorScreen Screen;

    private DataStatus ViewStatus;
    private XDocument ViewDocument;

    private string SearchKeyText = "";
    private string SearchValueText = "";

    public string Name = "";
    private string ImGuiName = "";

    private string AliasNameKey = "";

    private int CurrentRowIndex = -1;

    private bool selectRow = false;
    private bool focusRow = false;

    private bool NoPrimaryKey = false;

    private int childDepth = 0;

    private bool SetupAliasOverrides = false;
    private Dictionary<int, string> AliasOverrides = new Dictionary<int, string>();

    private List<XElement> Contents = new();

    public GenericTableView(TableEditorScreen screen, string name, string aliasNameKey, bool noPrimaryKey, DataStatus viewStatus, XDocument viewDocument)
    {
        Screen = screen;

        Name = name;
        ImGuiName = name;
        AliasNameKey = aliasNameKey;

        NoPrimaryKey = noPrimaryKey;

        ViewStatus = viewStatus;
        ViewDocument = viewDocument;

        Contents = ViewDocument.Elements().Elements().Elements().ToList();
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
            ProcessAliasOverrides();
        }

        ImGui.SetNextItemWidth(width);
        ImGui.InputText($"##{ImGuiName}_KeySearchBar", ref SearchKeyText, 255);
        UIHelper.ShowHoverTooltip($"Filters the list.\n\n{TextSearchFilters.SearchCommandsHint}");

        ImGui.Separator();

        ImGui.BeginChild($"{ImGuiName}Section");

        for (int i = 0; i < Contents.Count; i++)
        {
            var entry = Contents[i];

            var key = $"{i}";
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

                if (AliasOverrides.ContainsKey(i))
                {
                    alias = AliasOverrides[i];
                }
            }

            if (!TextSearchFilters.FilterTableRowEntry(entry, alias, SearchKeyText))
            {
                continue;
            }

            // Focus the newly selected row when set via command queue
            if (focusRow && i == CurrentRowIndex)
            {
                focusRow = false;
                CurrentRowIndex = i;
                ImGui.SetScrollHereY();
            }

            if (ImGui.Selectable($"Entry: {key}##{ImGuiName}selectEntry{i}", CurrentRowIndex == i))
            {
                CurrentRowIndex = i;
            }

            // Arrow Selection
            if (ImGui.IsItemHovered() && selectRow)
            {
                selectRow = false;
                CurrentRowIndex = i;
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
            if (CurrentRowIndex == i)
            {
                if (ImGui.BeginPopupContextItem($"##{ImGuiName}EntryContext{i}"))
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
        }

        ImGui.EndChild();
    }

    /// <summary>
    /// Refreshes the current state of the view
    /// </summary>
    public void Refresh()
    {
        ProcessAliasOverrides();
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
        ImGui.InputText($"##{ImGuiName}_ValueSearchBar", ref SearchValueText, 255);
        UIHelper.ShowHoverTooltip("Filters the list.");

        ImGui.BeginChild($"{ImGuiName}PropertySection");

        for(int i = 0; i < Contents.Count; i++)
        {
            var element = Contents[i];

            if(i == CurrentRowIndex)
            {
                if (ImGui.BeginTable($"{ImGuiName}AttributeTable", 2, ImGuiTableFlags.SizingFixedFit))
                {
                    ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Inputs", ImGuiTableColumnFlags.WidthFixed);

                    childDepth = 0;
                    HandleElementEntry(element, "root", CurrentRowIndex);

                    ImGui.EndTable();
                }

                if (!TableMeta.CheckMetaToggle("SuppressAdditionButtons", ViewStatus.Name))
                {
                    DisplayMissingElementOptions(element);
                }
            }
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
        DisplayHeaderRow(entry, imguiElementName, rowIndex);

        var attributes = entry.Attributes().ToList();

        for (int i = 0; i < attributes.Count; i++)
        {
            var attribute = attributes[i];

            DisplayAttributeRow(entry, attribute, i, imguiElementName, rowIndex);

            if (TableMeta.HasMetaData(entry, attribute))
            {
                DisplayMetaDataRow(entry, attribute, i, imguiElementName, rowIndex);
            }
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
            if (TextSearchFilters.FilterTableEntry(entry.Name.ToString(), SearchValueText))
            {
                ImGui.TableNextRow();

                // Name Column
                ImGui.TableSetColumnIndex(0);
                ImGui.AlignTextToFramePadding();

                var displayName = entry.Name.ToString();

                if (CFG.Current.TableEditor_View_Properties_DisplayNames)
                {
                    displayName = TableMeta.GetElementNameValue(
                    "Name",
                    $"{entry.Name}");
                }

                var description = TableMeta.GetElementNameValue(
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
    /// Handle the display of the attribute rows
    /// </summary>
    private void DisplayAttributeRow(XElement entry, XAttribute attribute, int attributeIndex, string imguiElementName, int rowIndex)
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

                var displayName = attribute.Name.ToString();

                if (CFG.Current.TableEditor_View_Properties_DisplayNames)
                {
                    displayName = TableMeta.GetAttributeNameValue(
                        "Name",
                        $"{entry.Name}",
                        $"{attribute.Name}");
                }

                var description = TableMeta.GetAttributeNameValue(
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
                if (TableMeta.IsBoolAttribute("IsBool", $"{entry.Name}", $"{attribute.Name}"))
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
        var allAttributes = TableMeta.GetAttributeList(entry);
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
                displayName = TableMeta.GetAttributeNameValue(
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
            if (TextSearchFilters.FilterTableEntry(attribute.Value, SearchValueText))
            {
                var elementName = entry.Name.ToString();
                var attributeName = attribute.Name.ToString();
                var documentName = TableMeta.GetDocumentName(elementName);

                var metaDoc = TableMeta.GetMetaDocument(documentName);
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
                foreach (var element in elements)
                {
                    if (element.Attribute("FileEnum") != null)
                    {
                        DisplayFileEnum(attribute, element, imguiElementName);
                    }
                    if (element.Attribute("TextRef") != null)
                    {
                        DisplayTextRef(attribute, element);
                    }
                    if (element.Attribute("GuidRef") != null)
                    {
                        DisplayGuidRef(entry, attribute, element, attributeIndex, imguiElementName, rowIndex);
                    }
                }
            }
        }
    }

    private string EnumSearchText = "";

    /// <summary>
    /// Handle the file enum reference meta text for a property that requires it.
    /// </summary>
    private void DisplayFileEnum(XAttribute attribute, XElement metaAttribute, string imguiElementName)
    {
        var enumParameters = metaAttribute.Attribute("FileEnum").Value.Split(",");
        var fileName = enumParameters[0];
        var listKey = enumParameters[1];
        var enumId = enumParameters[2];
        var enumName = enumParameters[3];

        var targetFile = DataHandler.Tables.Where(e => e.Key.Name == fileName).FirstOrDefault();
        var targetDoc = targetFile.Value;

        var targetElements = targetDoc.Descendants($"{listKey}").ToList();

        var displayedName = "";

        foreach (var tElement in targetElements)
        {
            var id = tElement.Attribute(enumId).Value;
            var name = tElement.Attribute(enumName).Value;

            if (attribute.Value == id)
            {
                displayedName = name;
            }
        }

        if (displayedName != "")
        {
            var boxWidth = 250;
            var boxSize = new Vector2(boxWidth, 300);

            UIHelper.DisplayInformationText(displayedName);

            if (ImGui.BeginPopupContextItem($"##{ImGuiName}_enumContextMenu_{imguiElementName}{childDepth}"))
            {
                // Go to file -> entry
                if (ImGui.Selectable($"Go to {fileName} -> {attribute.Value}"))
                {
                    EditorCommandQueue.AddCommand($"table/select/{fileName}/{attribute.Name}/{attribute.Value}");
                }

                // Enum Search
                ImGui.SetNextItemWidth(boxWidth);
                ImGui.InputText($"##{ImGuiName}_enumSearch_{imguiElementName}{childDepth}", ref EnumSearchText, 255);

                // Enum options
                if (ImGui.BeginListBox($"##{ImGuiName}_enumListBox_{imguiElementName}{childDepth}", boxSize))
                {
                    foreach (var tElement in targetElements)
                    {
                        var id = tElement.Attribute(enumId).Value;
                        var name = tElement.Attribute(enumName).Value;

                        if (name.Contains(EnumSearchText) || EnumSearchText == "")
                        {
                            if (ImGui.Selectable($"{id}: {name}"))
                            {
                                var action = new ChangeAttributeValue(attribute, attribute.Value, id, this);
                                Screen.EditorActionManager.ExecuteAction(action);
                                break;
                            }
                        }
                    }

                    ImGui.EndListBox();
                }

                ImGui.EndPopup();
            }

        }
    }

    /// <summary>
    /// Handle the text reference meta text for a property that requires it.
    /// </summary>
    private void DisplayTextRef(XAttribute attribute, XElement metaElement)
    {
        var localizationFile = metaElement.Attribute("TextRef").Value;
        var targetString = attribute.Value.ToString();

        var targetFile = DataHandler.Localization.Where(e => e.Key.Name == localizationFile).FirstOrDefault();
        var targetDoc = targetFile.Value;

        var rows = targetDoc.Root.Elements("Row").ToList();

        var displayedName = "";

        foreach (var row in rows)
        {
            var cells = row.Elements("Cell").ToList();

            var defString = cells[0].Value;
            var textString1 = cells[1].Value;
            var textString2 = cells[2].Value;

            if (defString == targetString)
            {
                displayedName = textString1;
            }
        }

        if (displayedName != "")
        {
            if (displayedName.Contains("%"))
            {
                displayedName = displayedName.Replace("%", "%%");
            }

            UIHelper.DisplayInformationText(displayedName, true);
        }
    }

    /// <summary>
    /// Handle the GUID reference meta text for a property that requires it.
    /// </summary>
    private void DisplayGuidRef(XElement entry, XAttribute attribute, XElement metaAttribute, int attributeIndex, string imguiElementName, int rowIndex)
    {

    }

    /// <summary>
    /// Fill out the alias override dictionary for the current document
    /// </summary>
    private void ProcessAliasOverrides()
    {
        AliasOverrides = new();

        const string targetAttributeName = "UIName";

        var metaDocument = TableMeta.GetMetaDocument(ViewStatus.Name);

        if (metaDocument?.Root == null)
            return;

        var metaElementList = metaDocument.Root.Descendants("entries").Elements();
        var targetMetaEntry = metaElementList.FirstOrDefault(e => e.Name == targetAttributeName);

        if (targetMetaEntry == null)
            return;

        var textRef = targetMetaEntry.Attribute("TextRef")?.Value;
        if (string.IsNullOrEmpty(textRef))
            return;

        var targetFile = DataHandler.Localization.FirstOrDefault(e => e.Key.Name == textRef).Value;
        if (targetFile?.Root == null)
            return;

        // Preload rows for fast lookup
        var rowDictionary = targetFile.Root.Elements("Row")
            .Select(row => row.Elements("Cell").ToList())
            .Where(cells => cells.Count >= 3)
            .ToDictionary(cells => cells[0].Value, cells => cells[1].Value);

        for (int i = 0; i < Contents.Count; i++)
        {
            var elementEntry = Contents[i];

            var attribute = elementEntry.Attribute(targetAttributeName);
            if (attribute == null)
            {
                continue;
            }

            rowDictionary.TryGetValue(attribute.Value, out string displayedName);
            AliasOverrides[i] = displayedName ?? "";
        }
    }

    /// <summary>
    /// Duplicate the currently selected row
    /// </summary>
    public void DuplicateRow()
    {
        var action = new AddTableRow(Contents, CurrentRowIndex, this);
        Screen.EditorActionManager.ExecuteAction(action);
    }

    /// <summary>
    /// Remove the currently selected row
    /// </summary>
    public void RemoveRow()
    {
        var action = new RemoveTableRow(Contents, CurrentRowIndex, this);
        Screen.EditorActionManager.ExecuteAction(action);
    }

}
