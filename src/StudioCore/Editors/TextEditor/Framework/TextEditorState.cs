using ImGuiNET;
using StudioCore.Core.Data;
using StudioCore.Editor;
using StudioCore.KCD;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StudioCore.Editors.TextEditor.Framework;

public class TextEditorState
{
    private TextEditorScreen Screen;

    public DataStatus SelectedStatus;
    public XDocument SelectedDocument;
    public KCDText SelectedText;

    public KCDText.Row SelectedTextRow;
    public int SelectedTextRowIndex = -1;

    public bool SelectNextText = false;
    public bool SelectNextTextRow = false;

    public ActionManager EditorActionManager = new();

    public TextEditorState(TextEditorScreen screen)
    {
        Screen = screen;
    }

    public void UpdateSelection(KeyValuePair<DataStatus, XDocument> selection)
    {
        UpdateSelectedDocument();

        SelectedStatus = selection.Key;
        SelectedDocument = selection.Value;
        SelectedText = new KCDText(selection.Value);
    }

    public void UpdateSelectedDocument()
    {
        if (SelectedText != null && SelectedStatus.Modified)
        {
            if (DataHandler.Localization.ContainsKey(SelectedStatus))
            {
                DataHandler.Localization[SelectedStatus] = SelectedText.ExportXML();
            }
        }
    }
}
