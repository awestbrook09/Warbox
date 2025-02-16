using StudioCore.Editor;
using StudioCore.Editors.TableEditor.Views;
using StudioCore.Interface;
using StudioCore.KCD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StudioCore.Editors.TextEditor.Actions;

public class RemoveTextRow : EditorAction
{
    private int RowIndex;
    private XElement SourceRow;
    private XElement PreviousRow;
    private XElement NextRow;
    private XElement OldRow;
    private XElement ParentContainer;

    public RemoveTextRow(int rowIndex)
    {
        SourceRow = Warbox.TextEditor.FileSelectionView.GetRowAtIndex(rowIndex);

        PreviousRow = Warbox.TextEditor.FileSelectionView.GetPreviousRow(rowIndex);
        NextRow = Warbox.TextEditor.FileSelectionView.GetNextRow(rowIndex);

        OldRow = new XElement(SourceRow);

        ParentContainer = SourceRow.Parent;
    }

    public override ActionEvent Execute()
    {
        if (SourceRow?.Parent != null)
        {
            SourceRow.Remove();
        }

        return ActionEvent.NoEvent;
    }

    // TODO: this fails to remove the row if when the original was a duplicate that was undone
    public override ActionEvent Undo()
    {
        if (ParentContainer == null || OldRow == null)
        {
            return ActionEvent.NoEvent;
        }

        // Source has been removed since this action occured
        if (SourceRow == null)
        {
            ParentContainer.Add(OldRow);
            return ActionEvent.NoEvent;
        }

        if (NextRow?.Parent != null)
        {
            NextRow.AddBeforeSelf(OldRow);
        }
        else if (PreviousRow?.Parent != null)
        {
            PreviousRow.AddAfterSelf(OldRow);
        }
        else
        {
            ParentContainer.Add(OldRow);
        }

        return ActionEvent.NoEvent;
    }
}