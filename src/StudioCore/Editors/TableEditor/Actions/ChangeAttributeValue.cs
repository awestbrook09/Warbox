using StudioCore.Editor;
using StudioCore.Editors.TableEditor.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StudioCore.Editors.TableEditor.Actions;

public class ChangeElementValue : EditorAction
{
    private XElement Element;
    private string OldValue;
    private string NewValue;
    private GenericTableView CurrentView;

    public ChangeElementValue(XElement element, string oldValue, string newValue, GenericTableView curView)
    {
        Element = element;
        OldValue = oldValue;
        NewValue = newValue;
        CurrentView = curView;
    }

    public override ActionEvent Execute()
    {
        Element.Value = NewValue;
        CurrentView.Refresh();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Element.Value = OldValue;
        CurrentView.Refresh();

        return ActionEvent.NoEvent;
    }
}

public class ChangeAttributeValue : EditorAction
{
    private XAttribute Attribute;
    private string OldValue;
    private string NewValue;
    private GenericTableView CurrentView;

    public ChangeAttributeValue(XAttribute attribute, string oldValue, string newValue, GenericTableView curView)
    {
        Attribute = attribute;
        OldValue = oldValue;
        NewValue = newValue;
        CurrentView = curView;
    }

    public override ActionEvent Execute()
    {
        Attribute.Value = NewValue;
        CurrentView.Refresh();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Attribute.Value = OldValue;
        CurrentView.Refresh();

        return ActionEvent.NoEvent;
    }
}