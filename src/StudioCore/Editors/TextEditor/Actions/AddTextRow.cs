using StudioCore.Editor;
using StudioCore.Editors.TableEditor.Views;
using StudioCore.Editors.TextEditor.Framework;
using StudioCore.KCD;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StudioCore.Editors.TextEditor.Actions;

public class AddTextRow : EditorAction
{
    private int RowIndex;

    private XElement Container;
    private XElement StoredRow;
    private List<XElement> ExistingRowList;
    private List<XElement> WorkingRowList;

    public AddTextRow()
    {
        RowIndex = TextSelection.RowSelectionIndex;

        StoredRow = TextSelection.GetRowAtIndex(RowIndex);

        Container = TextSelection.GetRowContainer();

        ExistingRowList = TextSelection.GetRowList();
        WorkingRowList = TextSelection.GetRowList();
    }

    public override ActionEvent Execute()
    {
        var newRow = new XElement(StoredRow);

        var index = WorkingRowList.IndexOf(StoredRow);

        if (index == -1)
        {
            WorkingRowList.Add(newRow);
        }
        else
        {
            WorkingRowList.Insert(index, newRow);
        }
        Container.ReplaceNodes(WorkingRowList);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Container.ReplaceNodes(ExistingRowList);

        return ActionEvent.NoEvent;
    }
}