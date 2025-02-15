using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Core.Data;
using StudioCore.Editors.TextEditor.Actions;
using StudioCore.Editors.TextEditor.Framework;
using StudioCore.Interface;
using StudioCore.KCD;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using static Assimp.Metadata;

namespace StudioCore.Editors.TextEditor.Views;

public class TextRowView
{
    private TextEditorScreen Screen;

    public int TextEntryIndex = -1;

    private bool SelectNextTextRow = false;
    public bool FocusEntry = false;

    public List<XElement> SelectedCells = new List<XElement>();

    public TextRowView(TextEditorScreen screen)
    {
        Screen = screen;
    }

    public void Display()
    {
        if (!Warbox.Project.IsValid())
            return;

        var width = ImGui.GetWindowWidth();

        var curElements = Screen.FileSelectionView.SelectedElements;

        if (ImGui.Begin("Rows##textRowView"))
        {
            ImGui.SetNextItemWidth(width * 0.75f);
            ImGui.InputText($"##textRowViewSearch", ref CFG.Current.TextEditor_RowFilterText, 255);
            UIHelper.ShowHoverTooltip("Filters the list.");

            ImGui.BeginChild("rowListSection");

            if (curElements != null && curElements.Count > 0)
            {
                // Row
                for (int i = 0; i < curElements.Count; i++)
                {
                    var entry = curElements[i];
                    var cells = entry.Elements().ToList();

                    var id = cells[0].Value;
                    var text = cells[1].Value;
                    var fallback_text = cells[2].Value;

                    if (TextSearchFilters.FilterRowList(id, text, fallback_text, CFG.Current.TextEditor_RowFilterText))
                    {
                        SelectionRow(curElements, entry, id, text, fallback_text, i);
                    }
                }
            }

            ImGui.EndChild();
            ImGui.End();
        }
    }

    private void SelectionRow(List<XElement> elements, XElement entry, string id, string text, string fallback_text, int rowIndex)
    {
        // Focus the newly selected row when set via command queue
        if (FocusEntry && TextEntryIndex == rowIndex)
        {
            FocusEntry = false;
            UpdateSelection(entry, rowIndex);
            ImGui.SetScrollHereY();
        }

        if (ImGui.Selectable($"{id}##textRow{id}{rowIndex}", TextEntryIndex == rowIndex))
        {
            UpdateSelection(entry, rowIndex);
        }

        // Only display aliases for entries of reasonable length
        if (Screen.FileSelectionView.SelectedStatus.Name != "text_ui_dialog")
        {
            UIHelper.DisplayAlias(text);
        }

        // Arrow Selection
        if (ImGui.IsItemHovered() && SelectNextTextRow)
        {
            SelectNextTextRow = false;
            UpdateSelection(entry, rowIndex);
        }
        if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
        {
            SelectNextTextRow = true;
        }

        // Context
        if (TextEntryIndex == rowIndex)
        {
            if (ImGui.BeginPopupContextItem($"##textRowContext{rowIndex}"))
            {
                // Duplicate
                if (ImGui.Selectable("Duplicate"))
                {
                    var action = new AddTextRow(elements, rowIndex);
                    Screen.EditorActionManager.ExecuteAction(action);
                }

                // Remove
                if (ImGui.Selectable("Remove"))
                {
                    var action = new RemoveTextRow(elements, rowIndex);
                    Screen.EditorActionManager.ExecuteAction(action);
                }

                ImGui.EndPopup();
            }
        }
    }

    public void UpdateSelection(XElement entry, int rowIndex, bool focus = false)
    {
        TextEntryIndex = rowIndex;
        SelectedCells = entry.Elements().ToList();

        if (focus)
            FocusEntry = true;
    }

    public void Shortcuts()
    {
        var curElements = Screen.FileSelectionView.SelectedElements;

        // Duplicate
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DuplicateSelectedEntry))
        {
            var action = new AddTextRow(curElements, TextEntryIndex);
            Screen.EditorActionManager.ExecuteAction(action);
        }

        // Remove
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DeleteSelectedEntry))
        {
            var action = new RemoveTextRow(curElements, TextEntryIndex);
            Screen.EditorActionManager.ExecuteAction(action);
        }
    }
}
