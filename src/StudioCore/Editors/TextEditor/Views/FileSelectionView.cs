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

    private string SearchText = "";

    public bool FocusFileEntry = false;
    public bool SelectNextText = false;

    public DataStatus SelectedStatus;
    public XDocument SelectedDocument;
    public List<XElement> SelectedElements;

    public FileSelectionView(TextEditorScreen screen)
    {
        Screen = screen;
    }

    public void Display()
    {
        var languageOptions = DataHandler.GetLanguageOptions();
        var curLanguage = CFG.Current.TextEditor_CurrentLanguage;

        if (ImGui.Begin("Files##textFileView"))
        {
            var width = ImGui.GetWindowWidth();

            // Language
            ImGui.SetNextItemWidth(width);
            if (ImGui.BeginCombo("##languageSelection", curLanguage))
            {
                foreach(var entry in languageOptions)
                {
                    if(ImGui.Selectable($"{entry}"))
                    {
                        CFG.Current.TextEditor_CurrentLanguage = entry;
                        SelectedStatus = null;
                        SelectedDocument = null;
                        SelectedElements = null;
                    }
                }

                ImGui.EndCombo();
            }

            ImGui.SetNextItemWidth(width);
            ImGui.InputText($"##textFileViewSearch", ref SearchText, 255);
            UIHelper.ShowHoverTooltip("Filters the list.");

            ImGui.BeginChild("fileListSection");

            for (int i = 0; i < DataHandler.GetCurrentLocalization().Count; i++)
            {
                var entry = DataHandler.GetCurrentLocalization().ElementAt(i);
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
        // Focus the newly selected row when set via command queue
        if (FocusFileEntry && SelectedStatus == status)
        {
            FocusFileEntry = false;
            UpdateSelection(entry);
            ImGui.SetScrollHereY();
        }

        if (ImGui.Selectable($"{name}##fileEntry{name}{index}", SelectedStatus == status))
        {
            UpdateSelection(entry);
        }

        // Arrow Selection
        if (ImGui.IsItemHovered() && SelectNextText)
        {
            SelectNextText = false;
            UpdateSelection(entry);
        }
        if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
        {
            SelectNextText = true;
        }
    }

    public void UpdateSelection(KeyValuePair<DataStatus, XDocument> entry, bool focus = false)
    {
        SelectedStatus = entry.Key;
        SelectedDocument = entry.Value;
        SelectedElements = entry.Value.Elements().Elements().ToList();

        if(focus)
            FocusFileEntry = true;
    }

    public void Shortcuts()
    {
    }
}
