using ImGuiNET;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StudioCore.Editors.TableEditor.Tools;

public static class TableMassEdit
{
    public static List<string> SelectionInputs = new List<string>() { "" };

    public static SelectionConditionLogic PropertySelectionLogic;

    public static List<string> EditInputs = new List<string>() { "" };

    /// <summary>
    /// Handles the selection criteria section
    /// </summary>
    public static void ConfigureSelection()
    {
        var width = ImGui.GetWindowWidth();
        var buttonSize = new Vector2(width * 0.32f, 24);

        //--------------
        // Actions
        //--------------
        // Documentation
        if (ImGui.Button($"{ForkAwesome.QuestionCircle}##selectionHintButton"))
        {
            ImGui.OpenPopup("selectionInputHint");
        }
        UIHelper.ShowHoverTooltip("View documentation on selection commands.");

        ImGui.SameLine();

        // Add
        if (ImGui.Button($"{ForkAwesome.Plus}##selectionAdd"))
        {
            SelectionInputs.Add("");
        }
        UIHelper.ShowHoverTooltip("Add new selection input row.");

        ImGui.SameLine();

        // Remove
        if (SelectionInputs.Count < 2)
        {
            ImGui.BeginDisabled();

            if (ImGui.Button($"{ForkAwesome.Minus}##selectionRemoveDisabled"))
            {
                SelectionInputs.RemoveAt(SelectionInputs.Count - 1);
            }
            UIHelper.ShowHoverTooltip("Remove last added selection input row.");

            ImGui.EndDisabled();
        }
        else
        {
            if (ImGui.Button($"{ForkAwesome.Minus}##selectionRemove"))
            {
                SelectionInputs.RemoveAt(SelectionInputs.Count - 1);
                UIHelper.ShowHoverTooltip("Remove last added selection input row.");
            }
        }

        ImGui.SameLine();

        // Reset
        if (ImGui.Button("Reset##resetSelectionInput"))
        {
            SelectionInputs = new List<string>() { "" };
        }
        UIHelper.ShowHoverTooltip("Reset selection input rows.");

        ImGui.SameLine();

        // Conditional Logic
        ImGui.SetNextItemWidth(width * 0.3f);
        if (ImGui.BeginCombo($"##selectionCommandLogic", PropertySelectionLogic.GetDisplayName()))
        {
            foreach (var entry in Enum.GetValues(typeof(SelectionConditionLogic)))
            {
                var curEnum = (SelectionConditionLogic)entry;

                if (ImGui.Selectable($"{curEnum.GetDisplayName()}", PropertySelectionLogic == curEnum))
                {
                    PropertySelectionLogic = curEnum;
                }
            }

            ImGui.EndCombo();
        }
        UIHelper.ShowHoverTooltip("The logic with which to handle the selection inputs." +
            "\n\nAll must match means all the selection criteria must be true for the property to be included." +
            "\n\nOne must match means only one of the selection criteria must be true for the property to be included.");

        //--------------
        // Selection Inputs
        //--------------
        for (int i = 0; i < SelectionInputs.Count; i++)
        {
            var curCommand = SelectionInputs[i];
            var curText = curCommand;

            ImGui.SetNextItemWidth(width);
            if (ImGui.InputText($"##selectionInput{i}", ref curText, 255))
            {
                SelectionInputs[i] = curText;
            }
            UIHelper.ShowHoverTooltip("The selection command to process.");
        }

    }


    /// <summary>
    /// Handles the edit section
    /// </summary>
    public static void ConfigureEdit()
    {
        var width = ImGui.GetWindowWidth();
        var buttonSize = new Vector2(width * 0.32f, 24);

        //--------------
        // Actions Inputs
        //--------------
        // Documentation
        if (ImGui.Button($"{ForkAwesome.QuestionCircle}##editHintButton"))
        {
            ImGui.OpenPopup("editInputHint");
        }
        UIHelper.ShowHoverTooltip("View documentation on edit commands.");

        ImGui.SameLine();

        // Add
        if (ImGui.Button($"{ForkAwesome.Plus}##editAdd"))
        {
            EditInputs.Add("");
        }
        UIHelper.ShowHoverTooltip("Add edit input row.");

        ImGui.SameLine();

        // Remove
        if (EditInputs.Count < 2)
        {
            ImGui.BeginDisabled();

            if (ImGui.Button($"{ForkAwesome.Minus}##editRemoveDisabled"))
            {
                EditInputs.RemoveAt(EditInputs.Count - 1);
            }
            UIHelper.ShowHoverTooltip("Remove last added edit input row.");

            ImGui.EndDisabled();
        }
        else
        {
            if (ImGui.Button($"{ForkAwesome.Minus}##editRemove"))
            {
                EditInputs.RemoveAt(EditInputs.Count - 1);
            }
            UIHelper.ShowHoverTooltip("Remove last added edit input row.");
        }

        ImGui.SameLine();

        // Reset
        if (ImGui.Button("Reset##resetEditInputs"))
        {
            EditInputs = new List<string>() { "" };
        }
        UIHelper.ShowHoverTooltip("Reset edit input rows.");

        //--------------
        // Edit Inputs
        //--------------
        for (int i = 0; i < EditInputs.Count; i++)
        {
            var curCommand = EditInputs[i];
            var curText = curCommand;

            ImGui.SetNextItemWidth(width);
            if (ImGui.InputText($"##editInput{i}", ref curText, 255))
            {
                EditInputs[i] = curText;
            }
            UIHelper.ShowHoverTooltip("The edit command to process.");
        }
    }

    /// <summary>
    /// Handles the Mass Edit
    /// </summary>
    public static void ProcessMassEdit()
    {
        var fileView = Warbox.TableEditor.FileSelectionView;
        var curDocument = fileView.GetSelectedDocument();

        var actions = new List<EditorAction>();

        var actionList = ProcessEditCommands(curDocument);
        foreach (var actionEntry in actionList)
        {
            actions.Add(actionEntry);
        }
    }

    /// <summary>
    /// Handles the edit command process
    /// </summary>
    private static List<EditorAction> ProcessEditCommands(XDocument curDocument)
    {
        var editCommands = EditInputs;

        List<EditorAction> actions = new();

        for (int i = 0; i < editCommands.Count; i++)
        {
            var cmd = editCommands[i];

            var action = PropertyValueOperation(curDocument, cmd);
            if (action != null)
                actions.Add(action);
        }

        return actions;
    }

    /// <summary>
    /// Handles the property value operation edits
    /// TODO: adjust how this is done so we don't need to duplicate the operation logic so much
    /// </summary>
    private static EditorAction PropertyValueOperation(XDocument curDocument, string cmd)
    {
        var input = cmd.Replace("prop:", "");

        var segments = input.Split(" ");
        if (segments.Length >= 3)
        {
            var prop = segments[0];
            var compare = segments[1].Trim().ToLower();
            var newValue = segments[2].Trim().ToLower();

            var index = -1;

            if (prop.Contains("[") && prop.Contains("]"))
            {
                var match = new Regex(@"\[(.*?)\]").Match(prop);

                if (match.Success)
                {
                    var val = match.Value.Replace("[", "").Replace("]", "");

                    int.TryParse(val, out index);
                    prop = prop.Replace($"{match.Value}", "");
                }
            }
        }

        return null;
    }

}

public enum SelectionConditionLogic
{
    [Display(Name = "All must match")]
    AND = 0,
    [Display(Name = "One must match")]
    OR = 1
}

