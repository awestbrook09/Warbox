using StudioCore.Editor;
using StudioCore.Editors.TableEditor.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StudioCore.Editors.TableEditor.Actions;

public class RemoveTableRow : EditorAction
{
    private XElement SourceRow;
    private XElement PreviousRow;
    private XElement NextRow;
    private XElement OldRow;
    private XElement ParentContainer;
    private GenericTableView CurrentView;

    public RemoveTableRow(GenericTableView curView)
    {
        CurrentView = curView;
        SourceRow = curView.GetCurrentRow();

        if (SourceRow == null)
        {
            return; 
        }

        PreviousRow = curView.GetPreviousRow();
        NextRow = curView.GetNextRow();
        OldRow = new XElement(SourceRow);

        ParentContainer = SourceRow.Parent;
    }

    public override ActionEvent Execute()
    {
        if (SourceRow?.Parent != null)
        {
            SourceRow.Remove();
        }

        CurrentView.Refresh();
        return ActionEvent.NoEvent;
    }

    // TODO: this fails to remove the row if when the original was a duplicate that was undone
    public override ActionEvent Undo()
    {
        if (ParentContainer == null || OldRow == null)
        {
            return ActionEvent.NoEvent;
        }

        bool prevExists = PreviousRow?.Parent != null;
        bool nextExists = NextRow?.Parent != null;

        if (nextExists)
        {
            NextRow.AddBeforeSelf(OldRow);
        }
        else if (prevExists)
        {
            PreviousRow.AddAfterSelf(OldRow);
        }
        else
        {
            ParentContainer.Add(OldRow);
        }

        CurrentView.Refresh();
        return ActionEvent.NoEvent;
    }
}