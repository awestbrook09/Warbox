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

    public bool FocusFileEntry = false;
    public bool SelectNextText = false;

    public ResourceDescriptor SelectedStatus;
    public XDocument SelectedDocument;

    public FileSelectionView(TextEditorScreen screen)
    {
        Screen = screen;
    }

    public void Display()
    {
        if (!Warbox.Project.IsValid())
            return;

        var languageOptions = TextDataHandler.GetLanguageOptions();
        var curLanguage = CFG.Current.TextEditor_CurrentLanguage;
        var curLocalization = TextDataHandler.GetCurrentLocalization();

        if (curLocalization == null)
            return;

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
                    }
                }

                ImGui.EndCombo();
            }

            ImGui.SetNextItemWidth(width);
            ImGui.InputText($"##textFileViewSearch", ref CFG.Current.TextEditor_FileFilterText, 255);
            UIHelper.ShowHoverTooltip("Filters the list.");

            ImGui.BeginChild("fileListSection");

            for (int i = 0; i < curLocalization.Count; i++)
            {
                var entry = curLocalization.ElementAt(i);
                var status = entry.Key;
                var name = entry.Key.Name;

                if (TextSearchFilters.FilterFileList(name, CFG.Current.TextEditor_FileFilterText))
                {
                    SelectionRow(i, entry, status, name);
                }
            }

            ImGui.EndChild();

            ImGui.End();
        }
    }

    private void SelectionRow(int index, KeyValuePair<ResourceDescriptor, XDocument> entry, ResourceDescriptor status, string name)
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

    public void UpdateSelection(KeyValuePair<ResourceDescriptor, XDocument> entry, bool focus = false)
    {
        SelectedStatus = entry.Key;
        SelectedDocument = entry.Value;

        if(focus)
            FocusFileEntry = true;
    }

    public void Shortcuts()
    {
    }

    public XElement GetRowAtIndex(int index)
    {
        return SelectedDocument.Elements().Elements().ElementAt(index);
    }
    public XElement GetNextRow(int index)
    {
        var curRow = GetRowAtIndex(index);

        if (curRow == null)
        {
            return null;
        }

        return SelectedDocument.Elements().Elements().ElementAt(index).ElementsAfterSelf().FirstOrDefault();
    }

    public XElement GetPreviousRow(int index)
    {
        var curRow = GetRowAtIndex(index);

        if (curRow == null)
        {
            return null;
        }

        return SelectedDocument.Elements().Elements().ElementAt(index).ElementsBeforeSelf().LastOrDefault();
    }

    public XElement GetContainer()
    {
        return SelectedDocument.Elements().FirstOrDefault();
    }

    public IEnumerable<XElement> GetContents()
    {
        return SelectedDocument.Elements().Elements();
    }
}
