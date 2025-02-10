using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Interface;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Assimp.Metadata;
using System.Xml.Linq;
using StudioCore.Core.Data;
using System.Numerics;
using System.Data.SqlTypes;
using StudioCore.Editors.TextEditor.Framework;

namespace StudioCore.Editors.TextEditor.Views;

public class FileSelectionView
{
    private TextEditorScreen Screen;
    private TextEditorState EditorState;

    private string SearchText = "";

    public FileSelectionView(TextEditorScreen screen)
    {
        Screen = screen;
        EditorState = screen.EditorState;
    }

    public void Display()
    {
        var width = ImGui.GetWindowWidth();

        if (ImGui.Begin("Files##textFileView"))
        {
            ImGui.SetNextItemWidth(width * 0.75f);
            ImGui.InputText($"##textFileViewSearch", ref SearchText, 255);
            UIHelper.ShowHoverTooltip("Filters the list.");

            ImGui.BeginChild("fileListSection");

            for (int i = 0; i < DataHandler.Localization.Count; i++)
            {
                var entry = DataHandler.Localization.ElementAt(i);
                var status = entry.Key;
                var name = entry.Key.Name;

                if (TextSearchFilters.FilterFileList(name, SearchText))
                {
                    SelectionRow(i, entry, status, name);
                }
            }

            ImGui.EndChild();

            ImGui.End();
        }
    }

    private void SelectionRow(int index, KeyValuePair<DataStatus, XDocument> entry, DataStatus status, string name)
    {
        if (ImGui.Selectable($"{name}##fileEntry{name}{index}", EditorState.SelectedStatus == status))
        {
            EditorState.UpdateSelection(entry);
        }

        // Arrow Selection
        if (ImGui.IsItemHovered() && EditorState.SelectNextText)
        {
            EditorState.SelectNextText = false;
            EditorState.UpdateSelection(entry);
        }
        if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
        {
            EditorState.SelectNextText = true;
        }

        // Context
        if (EditorState.SelectedStatus == status)
        {
            if (ImGui.BeginPopupContextItem($"##fileEntryContext{index}"))
            {
                CreateNewFile(index);
                ImGui.EndPopup();
            }
        }
    }

    public void Shortcuts()
    {
    }

    private string NewFileName = "";

    private void CreateNewFile(int index)
    {
        ImGui.SetNextItemWidth(250f);
        ImGui.InputText("##newFileName", ref NewFileName, 255);

        var isValidName = true;

        for (int i = 0; i < DataHandler.Localization.Count; i++)
        {
            var entry = DataHandler.Localization.ElementAt(i);
            var status = entry.Key;
            var name = entry.Key.Name;

            if (NewFileName == name || NewFileName == "")
            {
                isValidName = false;
            }
        }

        if (isValidName)
        {
            if (ImGui.Button("Create", new Vector2(250, 24)))
            {
                var xmlString = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Table>\r\n  <Row>\r\n    <Cell>blank</Cell>\r\n    <Cell></Cell>\r\n    <Cell></Cell>\r\n  </Row>\r\n</Table>";

                var newStatus = new DataStatus($"{NewFileName}", $"{NewFileName}.xml");
                newStatus.IsProjectData = true;

                XDocument newDoc = XDocument.Parse(xmlString);

                DataHandler.Localization.Add(newStatus, newDoc);
            }
        }
        else
        {
            ImGui.BeginDisabled();

            if (ImGui.Button("Create", new Vector2(250, 24)))
            {

            }

            ImGui.EndDisabled();
        }
    }
}
