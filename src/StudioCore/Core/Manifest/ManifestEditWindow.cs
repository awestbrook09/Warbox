using ImGuiNET;
using StudioCore.Core.Project;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Core.Manifest;

public static class ManifestEditWindow
{
    private static string _name = "";
    private static string _modid = "";
    private static string _description = "";
    private static string _author = "";
    private static string _version = "";
    private static string _created_on = "";
    private static string _modifies_level = "";
    private static bool _modifies_levelBool = false;
    private static string _dependencies = "";

    private static bool MenuOpenState = false;

    public static void ToggleMenuVisibility()
    {
        if(ManifestHandler.ManifestDocument != null)
        {
            MenuOpenState = !MenuOpenState;

            var infoElement = ManifestHandler.ManifestDocument.Root.Element("info");

            _name = infoElement.Element("name").Value;
            _modid = infoElement.Element("modid").Value;
            _description = infoElement.Element("description").Value;
            _author = infoElement.Element("author").Value;
            _version = infoElement.Element("version").Value;
            _created_on = infoElement.Element("created_on").Value;
            _modifies_level = infoElement.Element("modifies_level").Value;
            _dependencies = infoElement.Element("dependencies").Value;

            if(_modifies_level == "true")
            {
                _modifies_levelBool = true;
            }
        }
    }

    public static void Display()
    {
        var scale = Warbox.GetUIScale();

        if (!MenuOpenState)
            return;

        if (ManifestHandler.ManifestDocument == null)
            return;

        ImGui.SetNextWindowSize(new Vector2(600.0f, 400.0f) * scale, ImGuiCond.FirstUseEver);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, CFG.Current.Imgui_Moveable_MainBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, CFG.Current.Imgui_Moveable_TitleBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, CFG.Current.Imgui_Moveable_TitleBg_Active);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, CFG.Current.Imgui_Moveable_ChildBg);
        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(5.0f, 5.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f * scale);

        var flags = ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoResize;

        if (ImGui.Begin("Mod Manifest", ref MenuOpenState, flags))
        {
            DisplayModManifestEdit();
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);
    }

    public static void DisplayModManifestEdit()
    {
        var width = ImGui.GetWindowWidth();
        var flags = ImGuiTableFlags.SizingFixedFit;

        var infoElement = ManifestHandler.ManifestDocument.Root.Element("info");

        if (ImGui.BeginTable($"ModManifestTable", 2, flags))
        {
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed, 150);
            ImGui.TableSetupColumn("Input", ImGuiTableColumnFlags.WidthFixed, 400);

            // Name
            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.AlignTextToFramePadding();

            ImGui.Text("Name");
            UIHelper.ShowHoverTooltip("The name of the mod.");

            ImGui.TableSetColumnIndex(1);

            ImGui.AlignTextToFramePadding();

            ImGui.SetNextItemWidth(400);
            if(ImGui.InputText("##input_name", ref _name, 255))
            {
                infoElement.Element("name").Value = _name;
            }

            // Mod ID
            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.AlignTextToFramePadding();

            ImGui.Text("Mod ID");
            UIHelper.ShowHoverTooltip("The name of the mod when appending to files for patching purposes.");

            ImGui.TableSetColumnIndex(1);

            ImGui.AlignTextToFramePadding();

            ImGui.SetNextItemWidth(400);
            if (ImGui.InputText("##input_modid", ref _modid, 255))
            {
                infoElement.Element("modid").Value = _modid;
            }

            // Description
            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.AlignTextToFramePadding();

            ImGui.Text("Description");
            UIHelper.ShowHoverTooltip("A description of what the mod does.");

            ImGui.TableSetColumnIndex(1);

            ImGui.AlignTextToFramePadding();

            ImGui.SetNextItemWidth(400);
            if (ImGui.InputText("##input_description", ref _description, 255))
            {
                infoElement.Element("description").Value = _description;
            }

            // Author
            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.AlignTextToFramePadding();

            ImGui.Text("Author");
            UIHelper.ShowHoverTooltip("The names of the author or authors of the mod.");

            ImGui.TableSetColumnIndex(1);

            ImGui.AlignTextToFramePadding();

            ImGui.SetNextItemWidth(400);
            if (ImGui.InputText("##input_author", ref _author, 255))
            {
                infoElement.Element("author").Value = _author;
            }

            // Version
            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.AlignTextToFramePadding();

            ImGui.Text("Version");
            UIHelper.ShowHoverTooltip("The version of the mod these files represent.");

            ImGui.TableSetColumnIndex(1);

            ImGui.AlignTextToFramePadding();

            ImGui.SetNextItemWidth(400);
            if (ImGui.InputText("##input_version", ref _version, 255))
            {
                infoElement.Element("version").Value = _version;
            }

            // Created On
            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.AlignTextToFramePadding();

            ImGui.Text("Created On");
            UIHelper.ShowHoverTooltip("The date on which this mod was created.");

            ImGui.TableSetColumnIndex(1);

            ImGui.AlignTextToFramePadding();

            ImGui.SetNextItemWidth(400);
            if (ImGui.InputText("##input_created_on", ref _created_on, 255))
            {
                infoElement.Element("created_on").Value = _created_on;
            }

            // Modifies Level
            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.AlignTextToFramePadding();

            ImGui.Text("Modifies Map");
            UIHelper.ShowHoverTooltip("Whether this mod affects any of the maps.");

            ImGui.TableSetColumnIndex(1);

            ImGui.AlignTextToFramePadding();

            ImGui.SetNextItemWidth(400);
            if (ImGui.Checkbox("##input_modifies_level", ref _modifies_levelBool))
            {
                if(_modifies_levelBool)
                {
                    infoElement.Element("modifies_level").Value = "true";
                }
                else
                {
                    infoElement.Element("modifies_level").Value = "false";
                }
            }

            // Dependencies
            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.AlignTextToFramePadding();

            ImGui.Text("Dependencies");
            UIHelper.ShowHoverTooltip("Any mod dependencies this mod has.");

            ImGui.TableSetColumnIndex(1);

            ImGui.AlignTextToFramePadding();

            ImGui.SetNextItemWidth(400);
            if (ImGui.InputText("##input_dependencies", ref _dependencies, 255))
            {
                infoElement.Element("dependencies").Value = _dependencies;
            }

            // Edit
            if (ImGui.Button("Commit Edits", new Vector2(400, 24)))
            {
                ManifestHandler.WriteManifest();

                ToggleMenuVisibility();
            }

            ImGui.EndTable();
        }
    }
}
