using ImGuiNET;
using StudioCore.Core.Data;
using StudioCore.Editor;
using StudioCore.Editors.TableEditor.Actions;
using StudioCore.Editors.TableEditor.Tools;
using StudioCore.Editors.TableEditor.Views;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Assimp.Metadata;

namespace StudioCore.Editors.TableEditor.Framework;

public static class TableMetaDecorators
{
    private static Dictionary<string, List<GuidSearchResult>> GuidResults = new();

    private static string EnumSearchText = "";

    public static void Reset()
    {
        GuidResults = new();
    }

    public static bool HasRowDecorators(XElement entry, XAttribute attribute)
    {
        var elementName = entry.Name.ToString();
        var attributeName = attribute.Name.ToString();
        var documentName = TableMeta.GetDocumentName(elementName);

        var metaDoc = TableMeta.GetMetaDocument(documentName);
        if (metaDoc != null)
        {
            List<XElement> elements = metaDoc.Descendants($"{attributeName}").ToList();

            foreach (var element in elements)
            {
                if (element.Attribute("Enum") != null)
                {
                    return true;
                }
                if (element.Attribute("FileEnum") != null)
                {
                    return true;
                }
                if (element.Attribute("ConditionalFileEnum") != null)
                {
                    return true;
                }
                if (element.Attribute("TextRef") != null)
                {
                    return true;
                }
                if (element.Attribute("GuidRef") != null)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static void HandleRowDecorators(XElement entry, XElement curElement, string imguiName, int childDepth, XAttribute attribute, string curImguiKey, GenericTableView curView)
    {
        if (curElement.Attribute("Enum") != null)
        {
            DisplayEnum(imguiName, childDepth, attribute, curElement, curImguiKey, curView);
        }
        if (curElement.Attribute("FileEnum") != null)
        {
            DisplayFileEnum(imguiName, childDepth, attribute, curElement, curImguiKey, curView);
        }
        if (curElement.Attribute("ConditionalFileEnum") != null)
        {
            DisplayConditionalFileEnum(imguiName, childDepth, entry, attribute, curElement, curImguiKey, curView);
        }
        if (curElement.Attribute("TextRef") != null)
        {
            DisplayTextRef(imguiName, childDepth, attribute, curElement, curImguiKey);
        }
        if (curElement.Attribute("GuidRef") != null)
        {
            DisplayGuidRef(imguiName, childDepth, entry, attribute, curElement, curImguiKey);
        }
    }

    /// <summary>
    /// Handle the enum reference meta text for a property that requires it.
    /// </summary>
    public static void DisplayEnum(string imguiName, int childDepth, XAttribute attribute, XElement metaAttribute, string curImguiKey, GenericTableView curView)
    {
        var enumParameters = metaAttribute.Attribute("Enum").Value.Split(",");
        var enumName = enumParameters[0];

        var imguiKey = $"{imguiName}_{childDepth}_{enumName}_{curImguiKey}";

        var targetMeta = TableMeta.GetMetaDocument(curView.ViewStatus.Name);
        var enumOptions = TableMeta.GetEnumOptions(targetMeta, enumName);

        var displayedName = "";

        foreach (var tElement in enumOptions)
        {
            var id = tElement.Attribute("ID").Value;
            var name = tElement.Attribute("Name").Value;

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

            if (ImGui.BeginPopupContextItem($"##enumContextMenu_{imguiKey}"))
            {
                // Enum Search
                ImGui.SetNextItemWidth(boxWidth);
                ImGui.InputText($"##enumSearch_{imguiKey}", ref EnumSearchText, 255);

                // Enum options
                if (ImGui.BeginListBox($"##enumListBox_{imguiKey}", boxSize))
                {
                    foreach (var tElement in enumOptions)
                    {
                        var id = tElement.Attribute("ID").Value;
                        var name = tElement.Attribute("Name").Value;

                        if (name.Contains(EnumSearchText) || EnumSearchText == "")
                        {
                            if (ImGui.Selectable($"{id}: {name}"))
                            {
                                var action = new ChangeAttributeValue(attribute, attribute.Value, id, curView);
                                curView.Screen.EditorActionManager.ExecuteAction(action);
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
    /// Handle the file enum reference meta text for a property that requires it, conditional on the header
    /// </summary>
    public static void DisplayConditionalFileEnum(string imguiName, int childDepth, XElement entry, XAttribute attribute, XElement metaAttribute, string curImguiKey, GenericTableView curView)
    {
        var conditionEntries = metaAttribute.Attribute("ConditionalFileEnum").Value.Split(";");

        foreach (var conditionEntry in conditionEntries)
        {
            var enumParameters = conditionEntry.Split(",");
            var headerName = enumParameters[0];

            // * is used for the default enum in a set of conditional enums
            if (entry.Name != headerName || entry.Name != "*")
                return;

            var fileName = enumParameters[1];
            var listKey = enumParameters[2];
            var enumId = enumParameters[3];
            var enumName = enumParameters[4];

            var imguiKey = $"{imguiName}_{childDepth}_{fileName}_{curImguiKey}";

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

                if (ImGui.BeginPopupContextItem($"##fileConditionalEnumContextMenu_{imguiKey}"))
                {
                    // Go to file -> entry
                    if (ImGui.Selectable($"Go to {fileName} -> {attribute.Value}##goToConditionalEnumFile_{imguiKey}"))
                    {
                        EditorCommandQueue.AddCommand($"table/select/{fileName}/{attribute.Name}/{attribute.Value}/-1");
                    }

                    // Enum Search
                    ImGui.SetNextItemWidth(boxWidth);
                    ImGui.InputText($"##fileConditionalEnumSearch_{imguiKey}", ref EnumSearchText, 255);

                    // Enum options
                    if (ImGui.BeginListBox($"##fileConditionalEnumListBox_{imguiKey}", boxSize))
                    {
                        foreach (var tElement in targetElements)
                        {
                            var id = tElement.Attribute(enumId).Value;
                            var name = tElement.Attribute(enumName).Value;

                            if (name.Contains(EnumSearchText) || EnumSearchText == "")
                            {
                                if (ImGui.Selectable($"{id}: {name}"))
                                {
                                    var action = new ChangeAttributeValue(attribute, attribute.Value, id, curView);
                                    curView.Screen.EditorActionManager.ExecuteAction(action);
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
    }


    /// <summary>
    /// Handle the file enum reference meta text for a property that requires it.
    /// </summary>
    public static void DisplayFileEnum(string imguiName, int childDepth, XAttribute attribute, XElement metaAttribute, string curImguiKey, GenericTableView curView)
    {
        var enumParameters = metaAttribute.Attribute("FileEnum").Value.Split(",");
        var fileName = enumParameters[0];
        var listKey = enumParameters[1];
        var enumId = enumParameters[2];
        var enumName = enumParameters[3];

        var imguiKey = $"{imguiName}_{childDepth}_{fileName}_{curImguiKey}";

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

            if (ImGui.BeginPopupContextItem($"##fileEnumContextMenu_{imguiKey}"))
            {
                // Go to file -> entry
                if (ImGui.Selectable($"Go to {fileName} -> {attribute.Value}##goToEnumFile_{imguiKey}"))
                {
                    EditorCommandQueue.AddCommand($"table/select/{fileName}/{attribute.Name}/{attribute.Value}/-1");
                }

                // Enum Search
                ImGui.SetNextItemWidth(boxWidth);
                ImGui.InputText($"##fileEnumSearch_{imguiKey}", ref EnumSearchText, 255);

                // Enum options
                if (ImGui.BeginListBox($"##fileEnumListBox_{imguiKey}", boxSize))
                {
                    foreach (var tElement in targetElements)
                    {
                        var id = tElement.Attribute(enumId).Value;
                        var name = tElement.Attribute(enumName).Value;

                        if (name.Contains(EnumSearchText) || EnumSearchText == "")
                        {
                            if (ImGui.Selectable($"{id}: {name}"))
                            {
                                var action = new ChangeAttributeValue(attribute, attribute.Value, id, curView);
                                curView.Screen.EditorActionManager.ExecuteAction(action);
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
    public static void DisplayTextRef(string imguiName, int childDepth, XAttribute attribute, XElement metaElement, string curImguiKey)
    {
        var localizationFile = metaElement.Attribute("TextRef").Value;
        var targetString = attribute.Value.ToString();

        var imguiKey = $"{imguiName}_{childDepth}_{localizationFile}_{curImguiKey}";

        var targetFile = DataHandler.GetCurrentLocalization().Where(e => e.Key.Name == localizationFile).FirstOrDefault();
        var targetDoc = targetFile.Value;

        var rows = targetDoc.Root.Elements("Row").ToList();

        var displayedName = "";

        foreach (var row in rows)
        {
            var cells = row.Elements("Cell").ToList();

            var ui_string = cells[0].Value;
            var reference_text = cells[1].Value;
            var localized_text = cells[2].Value;

            if (ui_string == targetString)
            {
                displayedName = localized_text;
            }
        }

        if (displayedName != "")
        {
            if (displayedName.Contains("%"))
            {
                displayedName = displayedName.Replace("%", "%%");
            }

            UIHelper.DisplayInformationText(displayedName, true);

            if (ImGui.BeginPopupContextItem($"##textRefContextMenu_{imguiKey}"))
            {
                // Go to file -> entry
                if (ImGui.Selectable($"Go to {localizationFile} -> {attribute.Value}##goToTextRef_{imguiKey}"))
                {
                    EditorCommandQueue.AddCommand($"text/select/{localizationFile}/{attribute.Value}/-1");
                }

                ImGui.EndPopup();
            }
        }
    }

    /// <summary>
    /// Handle the GUID reference meta text for a property that requires it.
    /// </summary>
    public static void DisplayGuidRef(string imguiName, int childDepth, XElement entry, XAttribute attribute, XElement metaElement, string curImguiKey)
    {
        var imguiKey = $"{imguiName}_{childDepth}_temp_{curImguiKey}";

        var targetGuid = attribute.Value;

        if (targetGuid == null || targetGuid == "")
            return;

        var guidParameters = metaElement.Attribute("GuidRef").Value.Split(",");
        var targetFileName = guidParameters[0];
        var targetProperty = guidParameters[1];

        if (targetFileName.Contains("__"))
        {
            targetFileName = targetFileName.Split("__")[0];
        }

        if (GuidResults.ContainsKey(imguiKey))
        {
            DisplayGuidRefEntry(GuidResults[imguiKey], targetGuid, guidParameters, imguiKey);
        }
        else
        {
            if (GuidResults.Count < 1)
            {
                GuidResults = new Dictionary<string, List<GuidSearchResult>>
                {
                    { imguiKey, new List<GuidSearchResult>() }
                };
            }
            else
            {
                GuidResults.Add(imguiKey, new List<GuidSearchResult>());
            }

            foreach (var view in Warbox.EditorHandler.TableEditor.TableDataView.GetTableViews())
            {
                var viewName = view.Key;
                var curView = view.Value;

                var baseName = viewName;

                if (baseName.Contains("__"))
                {
                    baseName = viewName.Split("__")[0];
                }

                if (baseName == targetFileName)
                {
                    var results = TableGuidTools.FindAttributebyNameAndValue(curView.ViewDocument, targetProperty, targetGuid);

                    foreach (var res in results)
                    {
                        var guidResult = new GuidSearchResult(curView.ViewStatus.Name, res.Item1, res.Item2, res.Item3, res.Item4);

                        GuidResults[imguiKey].Add(guidResult);
                    }
                }
            }
        }

    }

    private static void DisplayGuidRefEntry(List<GuidSearchResult> results, string targetValue, string[] guidParameters, string curImguiKey)
    {
        var displayedName = "";

        var targetFileName = guidParameters[0];
        var targetProperty = guidParameters[1];
        var localizationFile = guidParameters[2];
        var localizationProperty = guidParameters[3];

        // This is a fallback check if the row doesn't use the main loc property
        var fallbackNameProperty = "";

        if (guidParameters.Length >= 5)
            fallbackNameProperty = guidParameters[4];

        // If individual result, show directly.
        if (results.Count > 0)
        {
            // Only show first result
            var result = results[0];

            var locAttribute = result.Descendant.Attribute(localizationProperty);
            if (locAttribute != null)
            {
                var target_ui_string = locAttribute.Value;

                if (localizationFile != "null")
                {
                    var targetDoc = DataHandler.Localization[CFG.Current.TextEditor_CurrentLanguage].Where(e => e.Key.Name == localizationFile).FirstOrDefault();

                    if (targetDoc.Value != null)
                    {
                        var rows = targetDoc.Value.Root.Elements("Row").ToList();

                        foreach (var row in rows)
                        {
                            var cells = row.Elements("Cell").ToList();

                            var ui_string = cells[0].Value;
                            var reference_text = cells[1].Value;
                            var localized_text = cells[2].Value;

                            if (ui_string == target_ui_string)
                            {
                                displayedName = localized_text;
                            }
                        }
                    }
                }
            }

            // Failed to find loc name, use fallback property to get script name
            if (displayedName == "" && fallbackNameProperty != "")
            {
                var fallbackAttribute = result.Descendant.Attribute(fallbackNameProperty);
                if (fallbackAttribute != null)
                {
                    displayedName = fallbackAttribute.Value;
                }
            }

            if (displayedName != "")
            {
                UIHelper.DisplayInformationText(displayedName);

                if (ImGui.BeginPopupContextItem($"##guidRefContextMenu_{curImguiKey}"))
                {
                    // Go to file -> entry
                    if (ImGui.Selectable($"Go to {result.File} -> {targetValue}##goToEnumFile_{curImguiKey}"))
                    {
                        EditorCommandQueue.AddCommand($"table/select/{result.File}/{targetProperty}/{targetValue}/-1");
                    }

                    ImGui.EndPopup();
                }
            }
        }
    }

}
