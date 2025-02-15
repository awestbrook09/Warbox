using Assimp;
using ImGuiNET;
using Silk.NET.Core;
using StudioCore.Core.Data;
using StudioCore.Editors.TableEditor.Actions;
using StudioCore.Editors.TextEditor.Actions;
using StudioCore.Editors.TextEditor.Framework;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace StudioCore.Editors.TextEditor.Views;

public class TextCellView
{
    private TextEditorScreen Screen;

    public TextCellView(TextEditorScreen screen)
    {
        Screen = screen;
    }

    public void Display()
    {
        if (!Warbox.Project.IsValid())
            return;

        var width = ImGui.GetWindowWidth();
        var idInput = new Vector2(width * 0.75f, 24 * Warbox.GetUIScale());
        var textInput = new Vector2(width * 0.75f, 300 * Warbox.GetUIScale());

        var curCells = Screen.TextRowView.SelectedCells;

        if (ImGui.Begin("Entries##textCellView"))
        {
            if(curCells.Count > 0)
            {
                if (ImGui.BeginTable($"CellEntries", 2, ImGuiTableFlags.SizingFixedFit))
                {
                    var id = curCells[0];
                    var reference_text = curCells[1];
                    var localized_text = curCells[2];

                    ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Inputs", ImGuiTableColumnFlags.WidthFixed);

                    // ID 
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.AlignTextToFramePadding();

                    ImGui.Text("UI String");

                    ImGui.TableSetColumnIndex(1);

                    HandleCellEntry(id, "id", idInput);

                    // Text
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.AlignTextToFramePadding();

                    ImGui.Text("Reference Text");

                    ImGui.TableSetColumnIndex(1);

                    HandleCellEntry(reference_text, "reference_text", textInput);

                    // Fallback Text
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.AlignTextToFramePadding();

                    ImGui.Text("Localized Text");

                    ImGui.TableSetColumnIndex(1);

                    HandleCellEntry(localized_text, "localized_text", textInput);

                    ImGui.TableSetColumnIndex(0);

                    ImGui.EndTable();
                }
            }

            ImGui.End();
        }
    }

    private void HandleCellEntry(XElement cell, string imguiId, Vector2 inputSize)
    {
        var isChanged = false;
        var cellText = cell.Value;
        var tNewText = cellText;

        ImGui.AlignTextToFramePadding();
        if (ImGui.InputTextMultiline($"##textEntry_{imguiId}", ref tNewText, 2000, inputSize))
        {
            isChanged = true;
        }

        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (isChanged)
            {
                var action = new ChangeTextValue(cell, cellText, tNewText);
                Screen.EditorActionManager.ExecuteAction(action);
            }
        }
    }

    public void Shortcuts()
    {

    }
}
