using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Veldrid;

namespace StudioCore;

[JsonSourceGenerationOptions(WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata, IncludeFields = true)]
[JsonSerializable(typeof(KeyBindings.Bindings))]
[JsonSerializable(typeof(KeyBind))]
internal partial class KeybindingsSerializerContext : JsonSerializerContext
{
}

public enum KeybindCategory
{
    Core,
    Window,
    Viewport
}

public class KeyBind
{
    public bool Alt_Pressed;
    public bool Ctrl_Pressed;
    public Key PrimaryKey;
    public bool Shift_Pressed;
    public bool FixedKey;

    public string PresentationName;
    public string Description;

    [JsonConstructor]
    public KeyBind()
    {
        PresentationName = "";
        Description = "";
    }

    public KeyBind(string name, string description, Key primaryKey = Key.Unknown, bool ctrlKey = false, bool altKey = false, bool shiftKey = false, bool fixedKey = false)
    {
        PresentationName = name;
        Description = description;

        PrimaryKey = primaryKey;
        Ctrl_Pressed = ctrlKey;
        Alt_Pressed = altKey;
        Shift_Pressed = shiftKey;
        FixedKey = fixedKey;
    }

    [JsonIgnore]
    public string HintText
    {
        get
        {
            if (PrimaryKey == Key.Unknown)
            {
                return "";
            }

            var str = "";
            if (Ctrl_Pressed)
            {
                str += "Ctrl+";
            }

            if (Alt_Pressed)
            {
                str += "Alt+";
            }

            if (Shift_Pressed)
            {
                str += "Shift+";
            }

            str += PrimaryKey.ToString();
            return str;
        }
    }
}

public class KeyBindings
{
    public static Bindings Current { get; set; }
    public static Bindings Default { get; set; } = new();

    public static void ResetKeyBinds()
    {
        Current = new Bindings();
    }

    public class Bindings
    {
        //-----------------------------
        // Core
        //-----------------------------
        // Core
        public KeyBind CORE_CreateNewEntry = new(
            "Create New Entry", 
            "Creates a new default entry based on the current selection context.", 
            Key.Insert);

        public KeyBind CORE_DeleteSelectedEntry = new(
            "Delete Selected Entry", 
            "Deletes the selected entry or entries based on the current selection context.", 
            Key.Delete);

        public KeyBind CORE_DuplicateSelectedEntry = new(
            "Duplicate",
            "Duplicates the selected entry or entries based on the current selection context.",
            Key.D, 
            true);

        public KeyBind CORE_RedoAction = new(
            "Redo", 
            "Re-executes a previously un-done action.", 
            Key.Y, 
            true);

        public KeyBind CORE_UndoAction = new(
            "Undo",
            "Undoes a previously executed action.",
            Key.Z,
            true);

        // Table Editor
        public KeyBind CORE_SaveTable = new(
            "Save Table File", 
            "Save the current table file in its entirety.", 
            Key.S, 
            true);

        public KeyBind CORE_SaveAllTables = new(
            "Save All Table Files",
            "Save all table files in their entirety.",
            Key.L,
            true);

        public KeyBind CORE_SavePatchedTableFile = new(
            "Save Patched Table File",
            "Saves the current table file changes to its own patched table file for this project.",
            Key.S,
            false,
            true);

        public KeyBind CORE_SaveAllPatchedTableFiles = new(
            "Save All Patched Table Files",
            "Saves the all table file changes to their own patched table files for this project.",
            Key.L,
            false,
            true);

        public KeyBind CORE_PackagePatchedTables = new(
            "Package Patched Tables",
            "Package all the patched tables files linked to this mod as a PAK file.",
            Key.P,
            true);

        // Text Editor
        public KeyBind CORE_SaveLocalizationFile = new(
            "Save Localization File",
            "Save the current localization file in its entirety.",
            Key.S,
            true);

        public KeyBind CORE_SaveAllLocalizationFiles = new(
            "Save All Localization Files",
            "Save all localization files in their entirety.",
            Key.L,
            true);

        public KeyBind CORE_PackageLocalizationFiles = new(
            "Package Localization Files",
            "Package all the localization files as a PAK file for the current language.",
            Key.P,
            true);

        // Windows
        public KeyBind CORE_ConfigurationWindow = new(
            "Configuration Window", 
            "Toggles the visibility of the Configuration window.",
            Key.F2);

        public KeyBind CORE_HelpWindow = new(
            "Help Window",
            "Toggles the visibility of the Help window.",
            Key.F3);

        public KeyBind CORE_KeybindConfigWindow = new(
            "Keybinds Window",
            "Toggles the visibility of the Keybinds window.",
            Key.F6);

        //-----------------------------
        // Viewport
        //-----------------------------
        // Core
        public KeyBind VIEWPORT_CameraForward = new(
            "Move Forward",
            "Moves the camera forward.",
            Key.W);

        public KeyBind VIEWPORT_CameraBack = new(
            "Move Back",
            "Moves the camera backwards.", 
            Key.S);

        public KeyBind VIEWPORT_CameraUp = new(
            "Move Up",
            "Moves the camera upwards.",
            Key.E);

        public KeyBind VIEWPORT_CameraDown = new(
            "Move Down",
            "Moves the camera downwards.", 
            Key.Q);

        public KeyBind VIEWPORT_CameraLeft = new(
            "Move Left",
            "Moves the camera leftwards.", 
            Key.A);

        public KeyBind VIEWPORT_CameraRight = new(
            "Move Right",
            "Moves the camera rightwards.",
            Key.D);

        public KeyBind VIEWPORT_CameraReset = new(
            "Reset Position", 
            "Resets the camera's position to (0,0,0)", 
            Key.R);

        // Gizmos
        public KeyBind VIEWPORT_GizmoRotationMode = new(
            "Cycle Gizmo Rotation Mode", 
            "Cycles through the gizmo rotation modes.", 
            Key.E);

        public KeyBind VIEWPORT_GizmoOriginMode = new(
            "Cycle Gizmo Origin Mode",
            "Cycles through the gizmo origin modes.",
            Key.Home);

        public KeyBind VIEWPORT_GizmoSpaceMode = new(
            "Cycle Gizmo Space Mode",
            "Cycles through the gizmo space modes.",
            Key.Unknown);

        public KeyBind VIEWPORT_GizmoTranslationMode = new(
            "Cycle Gizmo Translation Mode",
            "Cycles through the gizmo translation modes.", 
            Key.W);

        // Grid
        public KeyBind VIEWPORT_LowerGrid = new(
            "Lower Grid",
            "Lowers the viewport grid height by the specified unit increment.", 
            Key.Q, 
            true);

        public KeyBind VIEWPORT_RaiseGrid = new(
            "Raise Grid",
            "Raises the viewport grid height by the specified unit increment.",
            Key.E, 
            true);

        public KeyBind VIEWPORT_SetGridToSelectionHeight = new(
            "Move Grid to Selection Height",
            "Set the viewport grid height to the height of the current selection.",
            Key.K, 
            true);

        // Selection
        public KeyBind VIEWPORT_RenderOutline = new(
            "Toggle Selection Outline", 
            "Toggles the appearance of the selection outline.",
            Key.Unknown);

        //-----------------------------
        // Misc
        //-----------------------------
#pragma warning disable IDE0051
        // JsonExtensionData stores info in config file not present in class in order to retain settings between versions.
        [JsonExtensionData] internal IDictionary<string, JsonElement> AdditionalData { get; set; }
#pragma warning restore IDE0051
    }
}
