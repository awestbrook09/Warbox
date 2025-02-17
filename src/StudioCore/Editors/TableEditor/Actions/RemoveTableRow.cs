using StudioCore.Editor;
using StudioCore.Editors.TableEditor.Views;
using StudioCore.Editors.TextEditor.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StudioCore.Editors.TableEditor.Actions;

public class RemoveTableRow : EditorAction
{
    private GenericTableView CurrentView;

    private int RowIndex;

    private XElement Container;
    private XElement StoredRow;
    private List<XElement> ExistingRowList;
    private List<XElement> WorkingRowList;

    public RemoveTableRow(GenericTableView curView)
    {
        CurrentView = curView;

        RowIndex = CurrentView.RowSelectionIndex;

        StoredRow = CurrentView.GetRowAtIndex(RowIndex);

        Container = CurrentView.GetRowContainer();

        ExistingRowList = CurrentView.GetRowList();
        WorkingRowList = CurrentView.GetRowList();
    }

    public override ActionEvent Execute()
    {
        var index = WorkingRowList.IndexOf(StoredRow);

        if (index == -1)
        {
            WorkingRowList.Remove(StoredRow);
        }
        else
        {
            WorkingRowList.RemoveAt(index);
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