using Assimp;
using ImGuiNET;
using Newtonsoft.Json.Linq;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Editors.TableEditor.Actions;
using StudioCore.Editors.TextEditor;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using static Assimp.Metadata;

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

    private int rowIndex = -1;

    private bool selectRow = false;
    private bool focusRow = false;

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

    public void SetRowSelection(string key, int index)
    {
        rowIndex = index;
        focusRow = true;
    }

    public void DisplayEntries()
    {
        var width = ImGui.GetWindowWidth();

        var currentDocument = EditorState.SelectedDocument;
        var elementList = EditorState.GetCurrentEntries();

        ImGui.SetNextItemWidth(width);
        ImGui.InputText($"##{ImGuiName}_KeySearchBar", ref SearchKeyText, 255);
        UIHelper.ShowHoverTooltip($"Filters the list.\n\n{TextSearchFilters.SearchCommandsHint}");

        ImGui.Separator();

        ImGui.BeginChild($"{ImGuiName}Section");

        for (int i = 0; i < elementList.Count; i++)
        {
            var entry = elementList[i];

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
            }

            if (!TextSearchFilters.FilterTableRowEntry(entry, alias, SearchKeyText))
            {
                continue;
            }

            // Focus the newly selected row when set via command queue
            if(focusRow && i == rowIndex)
            {
                focusRow = false;
                rowIndex = i;
                ImGui.SetScrollHereY();
            }

            if (ImGui.Selectable($"Entry: {key}##{ImGuiName}selectEntry{i}", rowIndex == i))
            {
                rowIndex = i;
            }

            // Arrow Selection
            if (ImGui.IsItemHovered() && selectRow)
            {
                selectRow = false;
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
            if (rowIndex == i)
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

            if (!TableMeta.CheckMetaToggle(EditorState, "SuppressAdditionButtons", EditorState.SelectedStatus.Name))
            {
                DisplayMissingElementOptions(entry);
            }
        }

        ImGui.EndChild();
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

    private void DisplayMissingElementOptions(XElement entry)
    {
        var currentDocument = EditorState.SelectedDocument;
        var allAttributes = TableMeta.GetAttributeList(EditorState, currentDocument, entry);
        var curAttributes = entry.Attributes();

        var missingAttributes = new List<XElement>();

        foreach (var aAttribute in allAttributes)
        {
            var isMissing = true;

            foreach (var cAttribute in curAttributes)
            {
                if(aAttribute.Name == cAttribute.Name)
                {
                    isMissing = false;
                    break;
                }
            }

            if(isMissing)
            {
                missingAttributes.Add(aAttribute);
            }
        }

        foreach(var attrEntry in missingAttributes)
        {
            ImGui.AlignTextToFramePadding();
            if(ImGui.Button($"{ForkAwesome.Plus}"))
            {

            }
            UIHelper.ShowHoverTooltip("Add this property as it is not currently present.");

            ImGui.SameLine();

            var displayName = attrEntry.Name.ToString();

            if (CFG.Current.TableEditor_View_Properties_DisplayNames)
            {
                displayName = TableMeta.GetAttributeNameValue(
                EditorState,
                "Name",
                $"{entry.Name}",
                $"{attrEntry.Name}");
            }

            ImGui.AlignTextToFramePadding();
            UIHelper.DisplayActionText($"{displayName}");
        }
    }

    private void HandleElementEntry(XDocument currentDocument, XElement entry, string imguiElementName)
    {
        DisplayHeaderRow(currentDocument, entry, imguiElementName);

        var attributes = entry.Attributes().ToList();

        for (int i = 0; i < attributes.Count; i++)
        {
            var attribute = attributes[i];

            DisplayAttributeRow(currentDocument, entry, attribute, i, imguiElementName);
            if(TableMeta.HasMetaData(EditorState, currentDocument, entry, attribute, i, imguiElementName))
            {
                DisplayMetaDataRow(currentDocument, entry, attribute, i, imguiElementName);
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

    private void DisplayHeaderRow(XDocument currentDocument, XElement entry, string imguiElementName)
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
                    EditorState,
                    "Name",
                    $"{entry.Name}");
                }

                var description = TableMeta.GetElementNameValue(
                    EditorState,
                    "Description",
                    $"{entry.Name}");

                ImGui.SetNextItemWidth(width * 0.25f);
                UIHelper.DisplayHeaderText(displayName);
                UIHelper.ShowHoverTooltip(description);

                if (ImGui.BeginPopupContextItem($"####{ImGuiName}_contextMenu_{imguiElementName}{childDepth}"))
                {
                    if(ImGui.Selectable("Copy Header Name"))
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

    private void DisplayAttributeRow(XDocument currentDocument, XElement entry, XAttribute attribute, int i, string imguiElementName)
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
                        EditorState,
                        "Name",
                        $"{entry.Name}",
                        $"{attribute.Name}");
                }

                var description = TableMeta.GetAttributeNameValue(
                    EditorState,
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
                if(TableMeta.IsBoolAttribute(EditorState, "IsBool", $"{entry.Name}", $"{attribute.Name}"))
                {
                    var tBool = false;

                    if (oldValue == "true")
                        tBool = true;

                    if (ImGui.Checkbox($"##{ImGuiName}_inputBool_{attribute.Name}{i}{imguiElementName}{childDepth}", ref tBool))
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
                    if (ImGui.InputText($"##{ImGuiName}_input_{attribute.Name}{i}{imguiElementName}{childDepth}", ref tValue, 255))
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

    private void DisplayMetaDataRow(XDocument currentDocument, XElement entry, XAttribute attribute, int i, string imguiElementName)
    {
        var width = ImGui.GetWindowWidth();

        if (attribute != null)
        {
            if (TextSearchFilters.FilterTableEntry(attribute.Value, SearchValueText))
            {
                var elementName = entry.Name.ToString();
                var attributeName = attribute.Name.ToString();
                var documentName = TableMeta.GetDocumentName(elementName);

                var metaDoc = TableMeta.GetMetaDocument(EditorState, documentName);
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
                        DisplayFileEnum(entry, attribute, element, i, imguiElementName);
                    }
                    if (element.Attribute("TextRef") != null)
                    {
                        DisplayTextRef(entry, attribute, element, i, imguiElementName);
                    }
                    if (element.Attribute("GuidRef") != null)
                    {
                        DisplayGuidRef(entry, attribute, element, i, imguiElementName);
                    }
                }
            }
        }
    }

    private string EnumSearchText = "";

    private void DisplayFileEnum(XElement entry, XAttribute attribute, XElement metaAttribute, int i, string imguiElementName)
    {
        var enumParameters = metaAttribute.Attribute("FileEnum").Value.Split(",");
        var fileName = enumParameters[0];
        var listKey = enumParameters[1];
        var enumId = enumParameters[2];
        var enumName = enumParameters[3];

        var targetFile = Warbox.DataHandler.Tables.Where(e => e.Key.Name == fileName).FirstOrDefault();
        var targetDoc = targetFile.Value;

        var targetElements = targetDoc.Descendants($"{listKey}").ToList();

        var displayedName = "";

        foreach(var tElement in targetElements)
        {
            var id = tElement.Attribute(enumId).Value;
            var name = tElement.Attribute(enumName).Value;

            if(attribute.Value == id)
            {
                displayedName = name;
            }
        }

        if(displayedName != "")
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
                                var action = new ChangeAttributeValue(attribute, attribute.Value, id);
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

    private void DisplayTextRef(XElement entry, XAttribute attribute, XElement metaAttribute, int i, string imguiElementName)
    {
        var localizationFile = metaAttribute.Attribute("TextRef").Value;
        var targetString = attribute.Value.ToString();

        var targetFile = Warbox.DataHandler.Localization.Where(e => e.Key.Name == localizationFile).FirstOrDefault();
        var targetDoc = targetFile.Value;

        var rows = targetDoc.Root.Elements("Row").ToList();

        var displayedName = "";

        foreach (var row in rows)
        {
            var cells = row.Elements("Cell").ToList();

            var defString = cells[0].Value;
            var textString1 = cells[1].Value;
            var textString2 = cells[2].Value;

            if(defString == targetString)
            {
                displayedName = textString1;
            }
        }

        if(displayedName != "")
        {
            UIHelper.DisplayInformationText(displayedName, true);
        }
    }

    private void DisplayGuidRef(XElement entry, XAttribute attribute, XElement metaAttribute, int i, string imguiElementName)
    {

    }
}
