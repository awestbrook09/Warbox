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

namespace StudioCore.Editors.TableEditor;

public class TableEditorState
{
    private TableEditorScreen Screen;
    public ActionManager EditorActionManager = new();

    public DataStatus SelectedStatus;
    public XDocument SelectedDocument;

    public bool SelectNextTable = false;

    public List<XElement> CurrentEntries = new List<XElement>();

    public TableEditorState(TableEditorScreen screen)
    {
        Screen = screen;
    }

    public void UpdateSelection(KeyValuePair<DataStatus, XDocument> selection)
    {
        SelectedStatus = selection.Key;
        SelectedDocument = selection.Value;
    }

    public void InvalidateState()
    {
        CurrentEntries = new List<XElement>();
        Screen.TableDataView.RefreshTableViews();
    }

    public List<XElement> GetCurrentEntries()
    {
        if(CurrentEntries.Count < 1)
        {
            var database = SelectedDocument.Elements();
            var classEntry = database.Elements();
            var entries = classEntry.Elements();

            CurrentEntries = entries.ToList();
        }

        return CurrentEntries;
    }
}
