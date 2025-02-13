using StudioCore.Editor;
using StudioCore.Editors.TableEditor.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StudioCore.Editors.TextEditor.Actions;

public class ChangeTextValue : EditorAction
{
    private XElement Element;
    private string OldValue;
    private string NewValue;

    public ChangeTextValue(XElement element, string oldValue, string newValue)
    {
        Element = element;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute()
    {
        Element.Value = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Element.Value = OldValue;

        return ActionEvent.NoEvent;
    }
}