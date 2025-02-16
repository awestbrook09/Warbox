using StudioCore.Editor;
using StudioCore.Editors.TableEditor.Views;
using StudioCore.KCD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static StudioCore.KCD.KCDText;

namespace StudioCore.Editors.TableEditor.Actions;

public class AddTableRow : EditorAction
{
    private XElement SourceRow;
    private XElement NextRow;
    private XElement NewRow;
    private GenericTableView CurrentView;

    public AddTableRow(GenericTableView curView)
    {
        CurrentView = curView;
        SourceRow = curView.GetCurrentRow();
        NextRow = curView.GetNextRow();

        if (SourceRow != null)
        {
            NewRow = new XElement(SourceRow);
        }
    }

    public override ActionEvent Execute()
    {
        var container = CurrentView.GetContainer();

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

        CurrentView.Refresh();
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (NewRow?.Parent != null)
        {
            NewRow.Remove();
            CurrentView.Refresh();
        }

        return ActionEvent.NoEvent;
    }
}