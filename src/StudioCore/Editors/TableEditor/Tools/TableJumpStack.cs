using ImGuiNET;
using StudioCore.Editor;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TableEditor.Tools;

public static class TableJumpStack
{
    public static List<string> EditorCommands = new();
    public static int CurrentJumpCommand = -1;

    public static void Display()
    {
        var width = ImGui.GetWindowWidth();
        var buttonSize = new Vector2(width, 24);
        var childSectionSize = new Vector2(width, 500);

        UIHelper.WrappedText("Displays the editor jumps that have been performed. You can use this to replay the jumps.");
        ImGui.Text("");

        if (ImGui.Button("Clear", buttonSize))
        {
            EditorCommands.Clear();
        }

        ImGui.BeginChild("jumpStackList", childSectionSize);

        int index = 0;
        foreach(var entry in EditorCommands)
        {
            // Remove the table and operation type since it doesn't matter for the user
            // Replace the rest of the /'s with arrows
            var displayName = entry
                .Replace("table/select/", "")
                .Replace("table/silent_select/", "")
                .Replace("table/index_select/", "")
                .Replace("table/silent_index_select/", "")
                .Replace("/", " -> ");

            if (ImGui.Selectable($"{displayName}##jumpCommand{index}", CurrentJumpCommand == index))
            {
                CurrentJumpCommand = index;
                EditorCommandQueue.AddCommand(entry);
            }

            index++;
        }

        ImGui.EndChild();
    }

    public static void AddRowJump(string fileName, int index)
    {
        EditorCommands.Add($"table/silent_index_select/{fileName}/{index}");
    }

    public static void AddFileJump(string fileName)
    {
        EditorCommands.Add($"table/silent_index_select/{fileName}/-1");
    }

    public static void AddJump(string[] initcmd)
    {
        var fileName = initcmd[1];
        var targetAttributeName = initcmd[2];
        var targetAttributeValue = initcmd[3];
        var targetIndex = initcmd[4];

        EditorCommands.Add($"table/silent_select/{fileName}/{targetAttributeName}/{targetAttributeValue}/{targetIndex}");
    }
}
