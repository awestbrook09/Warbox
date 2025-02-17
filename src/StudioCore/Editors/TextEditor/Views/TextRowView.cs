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

    public TextRowView(TextEditorScreen screen)
    {
        Screen = screen;
    }

    public void Display()
    {
        if (!Warbox.Project.IsValid())
            return;

        var width = ImGui.GetWindowWidth();

        if (ImGui.Begin("Rows##textRowView"))
        {
            ImGui.SetNextItemWidth(width * 0.75f);
            ImGui.InputText($"##textRowViewSearch", ref CFG.Current.TextEditor_RowFilterText, 255);
            UIHelper.ShowHoverTooltip("Filters the list.");

            ImGui.BeginChild("rowListSection");

            if (TextSelection.FileSelectionDocument != null)
            {
                var contents = TextSelection.GetRows();

                if (contents != null && contents.Count() > 0)
                {
                    int index = 0;
                    foreach (var entry in contents)
                    {
                        var cells = entry.Elements().ToList();

                        var id = cells[0].Value;
                        var text = cells[1].Value;
                        var fallback_text = cells[2].Value;

                        if (TextSearchFilters.FilterRowList(id, text, fallback_text, CFG.Current.TextEditor_RowFilterText))
                        {
                            SelectionRow(entry, id, text, fallback_text, index);
                        }

                        index++;
                    }
                }
            }

            ImGui.EndChild();
            ImGui.End();
        }
    }

    private void SelectionRow(XElement entry, string id, string text, string fallback_text, int rowIndex)
    {
        // Focus the newly selected row when set via command queue
        if (TextSelection.FocusRowSelection && TextSelection.RowSelectionIndex == rowIndex)
        {
            TextSelection.FocusRowSelection = false;
            TextSelection.SelectRow(entry, rowIndex);
            ImGui.SetScrollHereY();
        }

        if (ImGui.Selectable($"{id}##textRow{id}{rowIndex}", TextSelection.RowSelectionIndex == rowIndex))
        {
            TextSelection.SelectRow(entry, rowIndex);
        }

        // Only display aliases for entries of reasonable length
        if (TextSelection.FileSelectionDescriptor.Name != "text_ui_dialog")
        {
            UIHelper.DisplayAlias(text);
        }

        // Arrow Selection
        if (ImGui.IsItemHovered() && TextSelection.RowArrowSelect)
        {
            TextSelection.RowArrowSelect = false;
            TextSelection.SelectRow(entry, rowIndex);
        }
        if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
        {
            TextSelection.RowArrowSelect = true;
        }

        // Context
        if (TextSelection.RowSelectionIndex == rowIndex)
        {
            if (ImGui.BeginPopupContextItem($"##textRowContext{rowIndex}"))
            {
                // Duplicate
                if (ImGui.Selectable("Duplicate"))
                {
                    DuplicateRow();
                }

                // Remove
                if (ImGui.Selectable("Remove"))
                {
                    RemoveRow();
                }

                ImGui.EndPopup();
            }
        }
    }

    public void Shortcuts()
    {
        // Duplicate
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DuplicateSelectedEntry))
        {
            DuplicateRow();
        }

        // Remove
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DeleteSelectedEntry))
        {
            RemoveRow();
        }
    }

    public void DuplicateRow()
    {
        if (TextSelection.RowSelectionIndex == -1)
            return;

        var action = new AddTextRow();
        Screen.EditorActionManager.ExecuteAction(action);
    }

    public void RemoveRow()
    {
        if (TextSelection.RowSelectionIndex == -1)
            return;

        var rowList = TextSelection.GetRowList();
        var curIndex = TextSelection.RowSelectionIndex;

        if (rowList.Count > 0)
        {
            var action = new RemoveTextRow();
            Screen.EditorActionManager.ExecuteAction(action);

            if (curIndex > 0)
            {
                var prevEntry = rowList[curIndex - 1];

                TextSelection.SelectRow(prevEntry, curIndex - 1);
            }
        }
    }
}
