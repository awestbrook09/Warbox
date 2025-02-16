using ImGuiNET;
using Silk.NET.SDL;
using StudioCore.Configuration;
using StudioCore.Configuration.Keybinds;
using StudioCore.Configuration.Settings;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Editors.TableEditor;
using StudioCore.Graphics;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.TextEditor;
using StudioCore.Tools;
using StudioCore.Tools.Development;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.Sdl2;
using Renderer = StudioCore.Scene.Renderer;
using Thread = System.Threading.Thread;

namespace StudioCore;

public class Warbox
{
    public static Project Project;
    public static bool ProjectChanged = false;
    public static bool ProjectInitialized = false;

    public List<EditorScreen> EditorList;
    public EditorScreen FocusedEditor;

    public static TableEditorScreen TableEditor;
    public static TextEditorScreen TextEditor;

    private static double _desiredFrameLengthSeconds = 1.0 / 20.0f;
    private static readonly bool _limitFrameRate = true;

    private static bool _initialLoadComplete;
    private static bool _firstframe = true;
    public static bool FirstFrame = true;

    public static bool LowRequirementsMode;

    public static IGraphicsContext _context;

    public static bool FontRebuildRequest;

    public static string _programTitle;

    private readonly string _version;

    public static EventHandler UIScaleChanged;

    public unsafe Warbox(IGraphicsContext context, string version)
    {
        Project = new Project();

        _version = version;
        _programTitle = $"Version {_version}";

        UIHelper.RestoreImguiIfMissing();

        UIScaleChanged += (_, _) =>
        {
            FontRebuildRequest = true;
        };

        // Hack to make sure dialogs work before the main window is created
        PlatformUtils.InitializeWindows(null);
        CFG.AttemptLoadOrDefault();
        UI.SetupThemes();
        UI.SetTheme(true);

        Environment.SetEnvironmentVariable("PATH",
            Environment.GetEnvironmentVariable("PATH") + Path.PathSeparator + "bin");

        _context = context;
        _context.Initialize();
        _context.Window.Title = _programTitle;

        PlatformUtils.InitializeWindows(context.Window.SdlWindowHandle);

        // Handlers
        TextEditor = new TextEditorScreen(_context.Window, _context.Device);
        TableEditor = new TableEditorScreen(_context.Window, _context.Device);

        EditorList = [
            TableEditor,
            TextEditor
        ];

        FocusedEditor = TableEditor;

        ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
        SetupFonts();
        _context.ImguiRenderer.OnSetupDone();

        ImGuiStylePtr style = ImGui.GetStyle();
        style.TabBorderSize = 0;
    }

    private unsafe void SetupFonts()
    {
        string engFont = @"Assets\Fonts\RobotoMono-Light.ttf";
        string otherFont = @"Assets\Fonts\NotoSansCJKtc-Light.otf";

        if (!string.IsNullOrWhiteSpace(CFG.Current.System_English_Font) && File.Exists(CFG.Current.System_English_Font))
            engFont = CFG.Current.System_English_Font;
        if (!string.IsNullOrWhiteSpace(CFG.Current.System_Other_Font) && File.Exists(CFG.Current.System_Other_Font))
            otherFont = CFG.Current.System_Other_Font;

        ImFontAtlasPtr fonts = ImGui.GetIO().Fonts;
        var fileEn = Path.Combine(AppContext.BaseDirectory, engFont);
        var fontEn = File.ReadAllBytes(fileEn);
        var fontEnNative = ImGui.MemAlloc((uint)fontEn.Length);
        Marshal.Copy(fontEn, 0, fontEnNative, fontEn.Length);

        var fileOther = Path.Combine(AppContext.BaseDirectory, otherFont);
        var fontOther = File.ReadAllBytes(fileOther);
        var fontOtherNative = ImGui.MemAlloc((uint)fontOther.Length);
        Marshal.Copy(fontOther, 0, fontOtherNative, fontOther.Length);

        var fileIcon = Path.Combine(AppContext.BaseDirectory, @"Assets\Fonts\forkawesome-webfont.ttf");
        var fontIcon = File.ReadAllBytes(fileIcon);
        var fontIconNative = ImGui.MemAlloc((uint)fontIcon.Length);
        Marshal.Copy(fontIcon, 0, fontIconNative, fontIcon.Length);

        fonts.Clear();

        var scale = GetUIScale();

        // English fonts
        {
            ImFontConfig* ptr = ImGuiNative.ImFontConfig_ImFontConfig();
            ImFontConfigPtr cfg = new(ptr);
            cfg.GlyphMinAdvanceX = 5.0f;
            cfg.OversampleH = 5;
            cfg.OversampleV = 5;
            fonts.AddFontFromMemoryTTF(fontEnNative, fontIcon.Length, (float)Math.Round(CFG.Current.Interface_FontSize * scale), cfg,
                fonts.GetGlyphRangesDefault());
        }

        // Other language fonts
        {
            ImFontConfig* ptr = ImGuiNative.ImFontConfig_ImFontConfig();
            ImFontConfigPtr cfg = new(ptr);
            cfg.MergeMode = true;
            cfg.GlyphMinAdvanceX = 7.0f;
            cfg.OversampleH = 5;
            cfg.OversampleV = 5;

            ImFontGlyphRangesBuilderPtr glyphRanges =
                new(ImGuiNative.ImFontGlyphRangesBuilder_ImFontGlyphRangesBuilder());
            glyphRanges.AddRanges(fonts.GetGlyphRangesJapanese());
            Array.ForEach(FontUtils.SpecialCharsJP, c => glyphRanges.AddChar(c));

            if (CFG.Current.System_Font_Chinese)
            {
                glyphRanges.AddRanges(fonts.GetGlyphRangesChineseFull());
            }

            if (CFG.Current.System_Font_Korean)
            {
                glyphRanges.AddRanges(fonts.GetGlyphRangesKorean());
            }

            if (CFG.Current.System_Font_Thai)
            {
                glyphRanges.AddRanges(fonts.GetGlyphRangesThai());
            }

            if (CFG.Current.System_Font_Vietnamese)
            {
                glyphRanges.AddRanges(fonts.GetGlyphRangesVietnamese());
            }

            if (CFG.Current.System_Font_Cyrillic)
            {
                glyphRanges.AddRanges(fonts.GetGlyphRangesCyrillic());
            }

            glyphRanges.BuildRanges(out ImVector glyphRange);
            fonts.AddFontFromMemoryTTF(fontOtherNative, fontOther.Length, (float)Math.Round((CFG.Current.Interface_FontSize + 2) * scale), cfg, glyphRange.Data);
            glyphRanges.Destroy();
        }

        // Icon fonts
        {
            ushort[] ranges = { ForkAwesome.IconMin, ForkAwesome.IconMax, 0 };
            ImFontConfig* ptr = ImGuiNative.ImFontConfig_ImFontConfig();
            ImFontConfigPtr cfg = new(ptr);
            cfg.MergeMode = true;
            cfg.GlyphMinAdvanceX = 12.0f;
            cfg.OversampleH = 5;
            cfg.OversampleV = 5;
            ImFontGlyphRangesBuilder b = new();

            fixed (ushort* r = ranges)
            {
                ImFontPtr f = fonts.AddFontFromMemoryTTF(fontIconNative, fontIcon.Length, (float)Math.Round((CFG.Current.Interface_FontSize + 2) * scale), cfg,
                    (IntPtr)r);
            }
        }

        _context.ImguiRenderer.RecreateFontDeviceTexture();
    }

    public void SetupCSharpDefaults()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
    }

    public void Run()
    {
        SetupCSharpDefaults();

        long previousFrameTicks = 0;
        Stopwatch sw = new();
        sw.Start();
        Tracy.Startup();
        while (_context.Window.Exists)
        {
            Tracy.TracyCFrameMark();

            // Limit frame rate when window isn't focused unless we are profiling
            var focused = Tracy.EnableTracy ? true : _context.Window.Focused;
            if (!focused)
            {
                _desiredFrameLengthSeconds = 1.0 / 20.0f;
            }
            else
            {
                _desiredFrameLengthSeconds = 1.0 / CFG.Current.System_Frame_Rate;
            }

            var currentFrameTicks = sw.ElapsedTicks;
            var deltaSeconds = (currentFrameTicks - previousFrameTicks) / (double)Stopwatch.Frequency;

            Tracy.___tracy_c_zone_context ctx = Tracy.TracyCZoneNC(1, "Sleep", 0xFF0000FF);
            while (_limitFrameRate && deltaSeconds < _desiredFrameLengthSeconds)
            {
                currentFrameTicks = sw.ElapsedTicks;
                deltaSeconds = (currentFrameTicks - previousFrameTicks) / (double)Stopwatch.Frequency;
                Thread.Sleep(focused ? 0 : 1);
            }

            Tracy.TracyCZoneEnd(ctx);

            previousFrameTicks = currentFrameTicks;

            ctx = Tracy.TracyCZoneNC(1, "Update", 0xFF00FF00);
            InputSnapshot snapshot = null;
            Sdl2Events.ProcessEvents();
            snapshot = _context.Window.PumpEvents();
            InputTracker.UpdateFrameInput(snapshot, _context.Window);
            Update((float)deltaSeconds);
            Tracy.TracyCZoneEnd(ctx);

            if (!_context.Window.Exists)
            {
                break;
            }

            if (true) //_window.Focused)
            {
                ctx = Tracy.TracyCZoneNC(1, "Draw", 0xFFFF0000);

                _context.Draw(EditorList, FocusedEditor);

                Tracy.TracyCZoneEnd(ctx);
            }
            else
            {
                // Flush the background queues
                Renderer.Frame(null, true);
            }
        }

        //DestroyAllObjects();
        Tracy.Shutdown();
        _context.Dispose();
        CFG.Save();
    }

    public void ApplyStyle()
    {
        var scale = GetUIScale();
        ImGuiStylePtr style = ImGui.GetStyle();

        // Colors
        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, CFG.Current.ImGui_MainBg);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, CFG.Current.ImGui_ChildBg);
        ImGui.PushStyleColor(ImGuiCol.PopupBg, CFG.Current.ImGui_PopupBg);
        ImGui.PushStyleColor(ImGuiCol.Border, CFG.Current.ImGui_Border);
        ImGui.PushStyleColor(ImGuiCol.FrameBg, CFG.Current.ImGui_Input_Background);
        ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, CFG.Current.ImGui_Input_Background_Hover);
        ImGui.PushStyleColor(ImGuiCol.FrameBgActive, CFG.Current.ImGui_Input_Background_Active);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, CFG.Current.ImGui_TitleBarBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, CFG.Current.ImGui_TitleBarBg_Active);
        ImGui.PushStyleColor(ImGuiCol.MenuBarBg, CFG.Current.ImGui_MenuBarBg);
        ImGui.PushStyleColor(ImGuiCol.ScrollbarBg, CFG.Current.ImGui_ScrollbarBg);
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrab, CFG.Current.ImGui_ScrollbarGrab);
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabHovered, CFG.Current.ImGui_ScrollbarGrab_Hover);
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabActive, CFG.Current.ImGui_ScrollbarGrab_Active);
        ImGui.PushStyleColor(ImGuiCol.CheckMark, CFG.Current.ImGui_Input_CheckMark);
        ImGui.PushStyleColor(ImGuiCol.SliderGrab, CFG.Current.ImGui_SliderGrab);
        ImGui.PushStyleColor(ImGuiCol.SliderGrabActive, CFG.Current.ImGui_SliderGrab_Active);
        ImGui.PushStyleColor(ImGuiCol.Button, CFG.Current.ImGui_Button);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, CFG.Current.ImGui_Button_Hovered);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, CFG.Current.ImGui_ButtonActive);
        ImGui.PushStyleColor(ImGuiCol.Header, CFG.Current.ImGui_Selection);
        ImGui.PushStyleColor(ImGuiCol.HeaderHovered, CFG.Current.ImGui_Selection_Hover);
        ImGui.PushStyleColor(ImGuiCol.HeaderActive, CFG.Current.ImGui_Selection_Active);
        ImGui.PushStyleColor(ImGuiCol.Tab, CFG.Current.ImGui_Tab);
        ImGui.PushStyleColor(ImGuiCol.TabHovered, CFG.Current.ImGui_Tab_Hover);
        ImGui.PushStyleColor(ImGuiCol.TabActive, CFG.Current.ImGui_Tab_Active);
        ImGui.PushStyleColor(ImGuiCol.TabUnfocused, CFG.Current.ImGui_UnfocusedTab);
        ImGui.PushStyleColor(ImGuiCol.TabUnfocusedActive, CFG.Current.ImGui_UnfocusedTab_Active);

        // Sizes
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.TabRounding, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarRounding, 0.0f);

        ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarSize, 16.0f * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, new Vector2(100f, 100f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, style.FramePadding * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, style.CellPadding * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, style.IndentSpacing * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, style.ItemSpacing * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, style.ItemInnerSpacing * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 1);
        ImGui.PushStyleVar(ImGuiStyleVar.PopupBorderSize, 1);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1);
    }

    public void UnapplyStyle()
    {
        ImGui.PopStyleColor(29);
        ImGui.PopStyleVar(14);
    }

    //Unhappy with this being here
    [DllImport("user32.dll", EntryPoint = "ShowWindow")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool _user32_ShowWindow(IntPtr hWnd, int nCmdShow);

    /// <summary>
    /// Called from Program.cs - Try to shutdown things gracefully on a crash
    /// </summary>
    public void CrashShutdown()
    {
        Tracy.Shutdown();
        _context.Dispose();
    }

    /// <summary>
    /// Called from Program.cs - Saves modded files to a recovery directory in the mod folder on crash
    /// </summary>
    public void AttemptSaveOnCrash()
    {
        if (!_initialLoadComplete)
        {
            // Program crashed on initial load, clear recent project to let the user launch the program next time without issue.
            try
            {
                CFG.Current.LastProjectFile = "";
                CFG.Save();
            }
            catch (Exception e)
            {
                PlatformUtils.Instance.MessageBox($"Unable to save config during crash recovery.\n" +
                                                  $"If you continue to crash on startup, delete config in AppData\\Local\\Warbox\n\n" +
                                                  $"{e.Message} {e.StackTrace}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
    }

    private unsafe void Update(float deltaseconds)
    {
        Tracy.___tracy_c_zone_context ctx = Tracy.TracyCZoneN(1, "Imgui");

        UpdateDpi();
        var scale = GetUIScale();

        if (Project.ProjectName != "")
        {
            _context.Window.Title = $"{Project.ProjectName} - {_programTitle}";
        }
        else
        {
            _context.Window.Title = $"No Project - {_programTitle}";
        }

        if (FontRebuildRequest)
        {
            _context.ImguiRenderer.Update(deltaseconds, InputTracker.FrameSnapshot, SetupFonts);
            FontRebuildRequest = false;
        }
        else
        {
            _context.ImguiRenderer.Update(deltaseconds, InputTracker.FrameSnapshot, null);
        }

        Tracy.TracyCZoneEnd(ctx);
        TaskManager.ThrowTaskExceptions();

        var commandsplit = EditorCommandQueue.GetNextCommand();
        if (commandsplit != null && commandsplit[0] == "windowFocus")
        {
            //this is a hack, cannot grab focus except for when un-minimising
            _user32_ShowWindow(_context.Window.Handle, 6);
            _user32_ShowWindow(_context.Window.Handle, 9);
        }

        ctx = Tracy.TracyCZoneN(1, "Style");
        ApplyStyle();
        ImGuiViewportPtr vp = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(vp.Pos);
        ImGui.SetNextWindowSize(vp.Size);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));
        ImGuiWindowFlags flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse |
                                 ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove;
        flags |= ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.MenuBar;
        flags |= ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus;
        flags |= ImGuiWindowFlags.NoBackground;

        ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
        if (ImGui.Begin("DockSpace_W", flags))
        {
        }

        var dsid = ImGui.GetID("DockSpace");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.NoSplit);
        ImGui.PopStyleVar(1);
        ImGui.End();
        ImGui.PopStyleColor(1);
        Tracy.TracyCZoneEnd(ctx);

        ctx = Tracy.TracyCZoneN(1, "Menu");
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0.0f);

        if (ImGui.BeginMainMenuBar())
        {
            ImGui.Separator();

            // Settings
            if (ImGui.BeginMenu("Settings"))
            {
                if (ImGui.MenuItem($"Configuration##SettingsWindow"))
                {
                    SettingsWindow.ToggleMenuVisibility();
                }
                UIHelper.ShowHoverTooltip($"Configuration\n{KeyBindings.Current.CORE_ConfigurationWindow.HintText}");

                if (ImGui.MenuItem($"Keybinds##KeybindWindow"))
                {
                    KeybindWindow.ToggleMenuVisibility();
                }
                UIHelper.ShowHoverTooltip($"Keybinds\n{KeyBindings.Current.CORE_KeybindConfigWindow.HintText}");

                if (CFG.Current.DisplayDebugTools)
                {
                    if (ImGui.MenuItem($"Debugging##DebugWindow"))
                    {
                        DebugWindow.ToggleMenuVisibility();
                    }
                    UIHelper.ShowHoverTooltip($"Debug Tools");
                }

                ImGui.EndMenu();
            }

            ImGui.Separator();

            // Project
            if (ImGui.BeginMenu("Project"))
            {
                // New Project
                DisplayTaskStatus();
                if (ImGui.MenuItem("Create New Project", "", false, MayChangeProject()))
                {
                    ProjectHandler.ClearProject();
                    ProjectCreationWindow.ToggleMenuVisibility();
                }
                UIHelper.ShowHoverTooltip("Create a new project.");

                // Close Project
                DisplayTaskStatus();
                if (ImGui.MenuItem("Close Current Project", "", false, MayChangeProject()))
                {
                    ProjectHandler.ClearProject();
                }
                UIHelper.ShowHoverTooltip("Close the currently loaded project.");

                // Open Project
                DisplayTaskStatus();
                if (ImGui.BeginMenu("Load Existing Project", MayChangeProject()))
                {
                    if (ImGui.MenuItem("Select Project"))
                    {
                        ProjectHandler.OpenProjectLoadDialog();
                    }
                    UIHelper.ShowHoverTooltip("Load an existing project by selecting its project.json.");

                    if (CFG.Current.RecentProjects.Count > 0)
                    {
                        ProjectHandler.DisplayRecentProjects();
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenu();
            }

            ImGui.Separator();

            FocusedEditor.DrawEditorMenu();

            TaskLogs.Display();

            ImGui.EndMainMenuBar();
        }

        ImGui.PopStyleVar();
        Tracy.TracyCZoneEnd(ctx);

        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 7.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(14.0f, 8.0f) * scale);

        // ImGui Debug windows
        if (DebugWindow._showImGuiDemoWindow)
        {
            ImGui.ShowDemoWindow(ref DebugWindow._showImGuiDemoWindow);
        }

        if (DebugWindow._showImGuiMetricsWindow)
        {
            ImGui.ShowMetricsWindow(ref DebugWindow._showImGuiMetricsWindow);
        }

        if (DebugWindow._showImGuiDebugLogWindow)
        {
            ImGui.ShowDebugLogWindow(ref DebugWindow._showImGuiDebugLogWindow);
        }

        if (DebugWindow._showImGuiStackToolWindow)
        {
            ImGui.ShowStackToolWindow(ref DebugWindow._showImGuiStackToolWindow);
        }

        ImGui.PopStyleVar(3);

        if (FirstFrame)
        {
            ImGui.SetNextWindowFocus();
        }

        ctx = Tracy.TracyCZoneN(1, "Editor");

        foreach (EditorScreen editor in EditorList)
        {
            string[] commands = null;
            if (commandsplit != null && commandsplit[0] == editor.CommandEndpoint)
            {
                commands = commandsplit.Skip(1).ToArray();
                ImGui.SetNextWindowFocus();
            }

            if (_context.Device == null)
            {
                ImGui.PushStyleColor(ImGuiCol.WindowBg, *ImGui.GetStyleColorVec4(ImGuiCol.WindowBg));
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
            }

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));

            if (ImGui.Begin(editor.EditorName))
            {
                ImGui.PopStyleColor(1);
                ImGui.PopStyleVar(1);
                editor.OnGUI(commands);
                ImGui.End();
                FocusedEditor = editor;
                editor.Update(deltaseconds);
            }
            else
            {
                ImGui.PopStyleColor(1);
                ImGui.PopStyleVar(1);
                ImGui.End();
            }

            if (ProjectChanged)
            {
                ProjectChanged = false;
                editor.OnProjectChanged();
            }
        }

        // Global shortcut keys
        if (!FocusedEditor.InputCaptured())
        {
            // Shortcut: Open Settings Window
            if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_ConfigurationWindow))
            {
                SettingsWindow.ToggleMenuVisibility();
            }

            // Shortcut: Open Keybind Window
            if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_KeybindConfigWindow))
            {
                KeybindWindow.ToggleMenuVisibility();
            }
        }

        SettingsWindow.Display();
        KeybindWindow.Display();
        DebugWindow.Display();

        ColorPicker.DisplayColorPicker();
        ProjectCreationWindow.Display();

        ImGui.PopStyleVar(2);
        UnapplyStyle();
        Tracy.TracyCZoneEnd(ctx);

        if (!_initialLoadComplete)
        {
            if (!TaskManager.AnyActiveTasks())
            {
                _initialLoadComplete = true;
            }
        }

        if (!_firstframe)
        {
            FirstFrame = false;
        }

        _firstframe = false;

        // Empty project is preset, try to load stored project from previous session
        if(!ProjectInitialized)
        {
            ProjectInitialized = true;

            if (CFG.Current.Project_LoadPreviousProject && CFG.Current.LastProjectFile != "")
            {
                ProjectHandler.LoadProjectOnStart();
            }
            else
            {
                ProjectCreationWindow.ToggleMenuVisibility();
            }
        }
    }

    private const float DefaultDpi = 96f;
    private static float _dpi = DefaultDpi;

    public static float Dpi
    {
        get => _dpi;
        set
        {
            if (Math.Abs(_dpi - value) < 0.0001f) return; // Skip doing anything if no difference

            _dpi = value;
            if (CFG.Current.System_ScaleByDPI)
                UIScaleChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    private static unsafe void UpdateDpi()
    {
        if (SdlProvider.SDL.IsValueCreated && _context?.Window != null)
        {
            var window = _context.Window.SdlWindowHandle;
            int index = SdlProvider.SDL.Value.GetWindowDisplayIndex(window);
            float ddpi = 96f;
            float _ = 0f;
            SdlProvider.SDL.Value.GetDisplayDPI(index, ref ddpi, ref _, ref _);

            Dpi = ddpi;
        }
    }

    public static float GetUIScale()
    {
        var scale = CFG.Current.System_UI_Scale;
        if (CFG.Current.System_ScaleByDPI)
            scale = scale / DefaultDpi * Dpi;
        return scale;
    }
    private bool MayChangeProject()
    {
        if (TaskManager.AnyActiveTasks())
        {
            return false;
        }

        return true;
    }

    private void DisplayTaskStatus()
    {
        var status = "";

        if (TaskManager.AnyActiveTasks())
        {
            status = status + "Active tasks still on going.\n";
        }

        if (status != "")
        {
            UIHelper.ShowHoverTooltip(status);
        }
    }
}

