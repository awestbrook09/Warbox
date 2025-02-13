using StudioCore.Editor;
using StudioCore.Editors.TableEditor.Views;
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
    private List<XElement> Elements;
    private XElement Row;

    public RemoveTextRow(List<XElement> elements, int rowIndex)
    {
        Elements = elements;
        RowIndex = rowIndex;
        Row = new XElement(Elements.ElementAt(rowIndex));
    }

    public override ActionEvent Execute()
    {
        Elements.RemoveAt(RowIndex);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Elements.Insert(RowIndex, Row);

        return ActionEvent.NoEvent;
    }
}