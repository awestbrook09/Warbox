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
    private TextEditorState EditorState;

    private string SearchText = "";

    public TextRowView(TextEditorScreen screen)
    {
        Screen = screen;
        EditorState = screen.EditorState;
    }

    public void Display()
    {
        var width = ImGui.GetWindowWidth();

        var curStatus = EditorState.SelectedStatus;
        var curDocument = EditorState.SelectedDocument;
        var curText = EditorState.SelectedText;

        if (ImGui.Begin("Rows##textRowView"))
        {
            if (curText == null || curText == null)
            {
                ImGui.Text("No text file selected.");
            }
            else
            {
                ImGui.SetNextItemWidth(width * 0.75f);
                ImGui.InputText($"##textRowViewSearch", ref SearchText, 255);
                UIHelper.ShowHoverTooltip("Filters the list.");

                ImGui.BeginChild("rowListSection");

                if (curStatus != null && curDocument != null && curText != null)
                {
                    for (int i = 0; i < curText.Rows.Count; i++)
                    {
                        var entry = curText.Rows[i];
                        var firstCell = entry.Cells.FirstOrDefault();

                        if (firstCell != null)
                        {
                            var name = firstCell;

                            var text1 = "";
                            if (entry.Cells.Count >= 2)
                                text1 = entry.Cells[1];

                            var text2 = "";
                            if (entry.Cells.Count >= 3)
                                text2 = entry.Cells[2];

                            if (TextSearchFilters.FilterRowList(name, text1, text2, SearchText))
                            {
                                SelectionRow(i, entry, name, text1);
                            }
                        }
                    }
                }

                ImGui.EndChild();
            }

            ImGui.End();
        }
    }

    private void SelectionRow(int index, KCDText.Row entry, string name, string text1)
    {
        var selectedIndex = EditorState.SelectedTextRowIndex;
        var curTextRow = EditorState.SelectedTextRow;

        if (ImGui.Selectable($"{name}##textRow{name}{index}", selectedIndex == index))
        {
            EditorState.SelectedTextRow = entry;
            EditorState.SelectedTextRowIndex = index;
        }

        // Only display aliases for entries of reasonable length
        if (EditorState.SelectedStatus.Name != "text_ui_dialog")
        {
            if (text1 != "" && text1.Length < 100)
            {
                UIHelper.DisplayAlias(text1);
            }
        }

        // Arrow Selection
        if (ImGui.IsItemHovered() && EditorState.SelectNextTextRow)
        {
            EditorState.SelectNextTextRow = false;
            EditorState.SelectedTextRowIndex = index;
        }
        if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
        {
            EditorState.SelectNextTextRow = true;
        }

        // Context
        if (selectedIndex == index)
        {
            if (ImGui.BeginPopupContextItem($"##textRowContext{index}"))
            {
                // Duplicate
                if (ImGui.Selectable("Duplicate"))
                {
                    var action = new AddTextRow(EditorState.SelectedText, index);
                    Screen.EditorActionManager.ExecuteAction(action);
                }

                // Remove
                if (ImGui.Selectable("Remove"))
                {
                    var action = new RemoveTextRow(EditorState.SelectedText, index);
                    Screen.EditorActionManager.ExecuteAction(action);
                }

                ImGui.EndPopup();
            }
        }
    }

    public void Shortcuts()
    {
        var selectedText = EditorState.SelectedText;
        var selectedIndex = EditorState.SelectedTextRowIndex;

        // Duplicate
        if (EditorState.SelectedText != null && InputTracker.GetKeyDown(KeyBindings.Current.CORE_DuplicateSelectedEntry))
        {
            var action = new AddTextRow(selectedText, selectedIndex);
            Screen.EditorActionManager.ExecuteAction(action);
        }

        // Remove
        if (EditorState.SelectedText != null && InputTracker.GetKeyDown(KeyBindings.Current.CORE_DeleteSelectedEntry))
        {
            var action = new RemoveTextRow(selectedText, selectedIndex);
            Screen.EditorActionManager.ExecuteAction(action);
        }
    }
}
