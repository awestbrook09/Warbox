using StudioCore.Editor;
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
    private int RowIndex;
    private List<XElement> Elements;
    private XElement Row;
    private GenericTableView CurrentView;

    public AddTableRow(List<XElement> elements, int rowIndex, GenericTableView curView)
    {
        Elements = elements;
        RowIndex = rowIndex;
        Row = new XElement(Elements.ElementAt(rowIndex));
        CurrentView = curView;
    }

    public override ActionEvent Execute()
    {
        Elements.Insert(RowIndex, Row);
        CurrentView.Refresh();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Elements.RemoveAt(RowIndex);
        CurrentView.Refresh();

        return ActionEvent.NoEvent;
    }
}