using StudioCore.Editor;
using StudioCore.Editors.TableEditor.Views;
using StudioCore.KCD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StudioCore.Editors.TextEditor.Actions;

public class AddTextRow : EditorAction
{
    private int RowIndex;
    private XElement SourceRow;
    private XElement NextRow;
    private XElement NewRow;

    public AddTextRow(int rowIndex)
    {
        RowIndex = rowIndex;
        SourceRow = Warbox.TextEditor.FileSelectionView.GetRowAtIndex(rowIndex);
        NextRow = Warbox.TextEditor.FileSelectionView.GetNextRow(rowIndex);

        if (SourceRow != null)
        {
            NewRow = new XElement(SourceRow);
        }
    }

    public override ActionEvent Execute()
    {
        var container = Warbox.TextEditor.FileSelectionView.GetContainer();

        if (container == null || NewRow == null)
        {
            return ActionEvent.NoEvent;
        }

        if (SourceRow != null && SourceRow.Parent != null)
        {
            SourceRow.AddAfterSelf(NewRow);
        }
        else if (NextRow != null)
        {
            NextRow.AddBeforeSelf(NewRow);
        }
        else
        {
            container.Add(NewRow);
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (NewRow?.Parent != null)
        {
            NewRow.Remove();
        }

        return ActionEvent.NoEvent;
    }
}