using Assimp;
using ImGuiNET;
using StudioCore.Core.Data;
using StudioCore.Editors.TextEditor.Framework;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace StudioCore.Editors.TextEditor.Views;

public class TextCellView
{
    private TextEditorScreen Screen;
    private TextEditorState EditorState;

    public TextCellView(TextEditorScreen screen)
    {
        Screen = screen;
        EditorState = screen.EditorState;
    }

    public void Display()
    {
        var height = ImGui.GetWindowHeight();

        var curStatus = EditorState.SelectedStatus;
        var curDocument = EditorState.SelectedDocument;
        var curText = EditorState.SelectedText;
        var curTextRow = EditorState.SelectedTextRow;

        if (ImGui.Begin("Entries##textCellView"))
        {
            if (curTextRow == null || curTextRow == null)
            {
                ImGui.Text("No text row selected.");
            }
            else
            {
                if (curTextRow != null)
                {
                    for (int i = 0; i < curTextRow.Cells.Count; i++)
                    {
                        var cell = curTextRow.Cells[i];

                        var size = new Vector2(-1, 24 * Warbox.GetUIScale());

                        if (i > 0)
                            size = new Vector2(-1, 100 * Warbox.GetUIScale());

                        if (ImGui.InputTextMultiline($"##textEntry{i}", ref cell, 2000, size))
                        {
                            curTextRow.Cells[i] = cell;
                            EditorState.SelectedStatus.Modified = true;
                        }
                    }
                }
            }

            ImGui.End();
        }
    }

    public void Shortcuts()
    {

    }
}
