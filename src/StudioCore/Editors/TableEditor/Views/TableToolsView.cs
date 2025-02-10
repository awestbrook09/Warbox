using ImGuiNET;
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
        var width = ImGui.GetWindowWidth();

        if (ImGui.Begin("Tools##tableToolsView"))
        {
            if (ImGui.CollapsingHeader("GUID Finder"))
            {
                DisplayGuidFinder();
            }

            if (ImGui.CollapsingHeader("GUID Generator"))
            {
                DisplayGuidGenerator();
            }

            if (ImGui.CollapsingHeader("Property Search"))
            {
                DisplayPropertySearch();
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

    private string guidInput = "";

    private void DisplayGuidFinder()
    {
        var width = ImGui.GetWindowWidth();
        var buttonSize = new Vector2(width, 24);
        var childSectionSize = new Vector2(width, 500);

        ImGui.Text("This tool will let you find all instances of a specified GUID and quickly navigate to them.");
        ImGui.Text("");

        ImGui.SetNextItemWidth(width);
        ImGui.InputText("##guidInput", ref guidInput, 255);
        UIHelper.ShowHoverTooltip("Input the GUID you wish to search for.");

        if (ImGui.Button("Search", buttonSize))
        {
            TableGuidTools.FindGuids(guidInput);
        }

        ImGui.Separator();

        ImGui.BeginChild("guidFinderResults", childSectionSize);

        foreach (var res in TableGuidTools.GuidFinderResults)
        {
            var filename = res.File;
            var attributeName = res.Attribute.Name;
            var attributeValue = res.Attribute.Value;
            var index = res.Index;

            if (ImGui.Selectable($"{filename} -> {index}: {attributeName}"))
            {
                EditorCommandQueue.AddCommand($"table/select/{filename}/{attributeName}/{attributeValue}");
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

    public void DisplayPropertySearch()
    {

    }

    public void DisplayMassEdit()
    {

    }
}

