using ImGuiNET;
using NativeFileDialogSharp;
using StudioCore.Configuration;
using StudioCore.Core.Data;
using StudioCore.Editor;
using StudioCore.Editors.TableEditor.Tools;
using StudioCore.Editors.TextEditor;
using StudioCore.Interface;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace StudioCore.Editors.TableEditor.Views;

public class TableToolsView
{
    private TableEditorScreen Screen;

    private string SearchText = "";

    public TableToolsView(TableEditorScreen screen)
    {
        Screen = screen;
    }

    public void Display()
    {
        if (!Warbox.Project.IsValid())
            return;

        var width = ImGui.GetWindowWidth();

        if (ImGui.Begin("Tools##tableToolsView"))
        {
            if (ImGui.CollapsingHeader("Property Value Search"))
            {
                DisplayPropertyValueSearch();
            }

            if (ImGui.CollapsingHeader("GUID Generator"))
            {
                DisplayGuidGenerator();
            }

            if (ImGui.CollapsingHeader("Mass Edit"))
            {
                DisplayMassEdit();
            }

            ImGui.End();
        }
    }

    public void Shortcuts()
    {

    }

    private string propertyValueInput = "";

    private void DisplayPropertyValueSearch()
    {
        var width = ImGui.GetWindowWidth();
        var buttonSize = new Vector2(width, 24);
        var childSectionSize = new Vector2(width, 500);

        UIHelper.WrappedText("This tool will let you find all instances of a specified property value and quickly navigate to them.");
        ImGui.Text("");

        ImGui.Checkbox("Fuzzy Match", ref TablePropertyValueFinder.LooseMatch);
        UIHelper.ShowHoverTooltip("If enabled, will match if the property value contains the input value, rather than matching exactly.");

        ImGui.SetNextItemWidth(width);
        ImGui.InputText("##propertyValueInput", ref propertyValueInput, 255);
        UIHelper.ShowHoverTooltip("Input the property value you wish to search for.");

        if (ImGui.Button("Search", buttonSize))
        {
            TablePropertyValueFinder.FindPropertyValues(propertyValueInput);
        }

        ImGui.Separator();

        ImGui.BeginChild("propertyValueFinderResults", childSectionSize);

        for (int i = 0; i < TablePropertyValueFinder.PropertyValueFinderResults.Count; i++)
        {
            var result = TablePropertyValueFinder.PropertyValueFinderResults[i];
            var filename = result.File;
            var rowIndex = result.RowIndex;
            var elementName = result.Element.Name;
            var descendantName = result.Descendant.Name;
            var attributeName = result.Attribute.Name;
            var attributeValue = result.Attribute.Value;

            if (ImGui.Selectable($"{filename} [{rowIndex}] -> {elementName} -> {descendantName} -> {attributeName}##resultEntry{i}"))
            {
                EditorCommandQueue.AddCommand($"table/select/{filename}/{attributeName}/{attributeValue}/{rowIndex}");
            }
        }

        ImGui.EndChild();
    }

    private string guidOutput = "";

    public void DisplayGuidGenerator()
    {
        var width = ImGui.GetWindowWidth();
        var buttonSize = new Vector2(width, 24);

        ImGui.Text("This tool will generate a new GUID for usage in a table.");
        ImGui.Text("");

        ImGui.SetNextItemWidth(width);
        ImGui.InputText("##guidOutput", ref guidOutput, 255);
        UIHelper.ShowHoverTooltip("The GUID output.");

        if (ImGui.Button("Generate", buttonSize))
        {
            var guid = TableGuidTools.GenerateGuidV4();
            guidOutput = guid.ToString();
        }
    }

    public void DisplayMassEdit()
    {
        var width = ImGui.GetWindowWidth();
        var buttonSize = new Vector2(width, 24);

        ImGui.Separator();
        UIHelper.WrappedText("Selection Criteria");
        UIHelper.ShowHoverTooltip("Determine which map objects will be affected by the mass edit.");
        ImGui.Separator();

        TableMassEdit.ConfigureSelection();

        ImGui.Separator();
        UIHelper.WrappedText("Edit Commands");
        UIHelper.ShowHoverTooltip("Determine which property to affect and the value change to apply for this mass edit.");
        ImGui.Separator();

        TableMassEdit.ConfigureEdit();

        if (ImGui.Button("Apply", buttonSize))
        {
            TableMassEdit.ProcessMassEdit();
        }
    }
}

