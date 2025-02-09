using ImGuiNET;
using StudioCore.Editors.TextEditor;
using StudioCore.Interface;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StudioCore.Editors.TableEditor;

public class TableDataView
{
    private TableEditorScreen Screen;
    private TableEditorState EditorState;

    private SortedDictionary<string, GenericTableView> TableViews = new();
    private SortedDictionary<string, string> XmlNames = new();

    public TableDataView(TableEditorScreen screen)
    {
        Screen = screen;
        EditorState = screen.EditorState;

        foreach(var entry in TableDefinition.Definitions)
        {
            var name = entry.Attribute("Name").Value;
            var aliasKey = entry.Attribute("AliasNameKey").Value;
            var rowKey = entry.Attribute("RowNameKey").Value;

            // This is set for XMLs that don't have a clear primary key,
            // instead they will be displayed as Entry 1, 2, etc in the selection list
            var noPrimaryKey = false;
            var npk = entry.Attribute("NoPrimaryKey");
            if(npk != null)
            {
                noPrimaryKey = true;
            }

            var newView = new GenericTableView(screen, name, aliasKey, rowKey, noPrimaryKey);

            TableViews.Add(name, newView);
        }

        TableMeta.Setup();
    }

    public SortedDictionary<string, GenericTableView> GetTableViews()
    {
        return TableViews;
    }

    public GenericTableView GetSelectedTableView()
    {
        if(TableViews.ContainsKey(EditorState.SelectedStatus.Name))
        {
            return TableViews[EditorState.SelectedStatus.Name];
        }
        else
        {
            return null;
        }
    }

    public void Display()
    {
        if (ImGui.Begin("Rows##tableRowView"))
        {
            var xmlName = GetSelectedXmlName();

            foreach(var entry in TableViews)
            {
                if (entry.Value.Name == xmlName)
                {
                    entry.Value.DisplayEntries();
                }
            }

            ImGui.End();
        }


        if (ImGui.Begin("Properties##tablePropertyView"))
        {
            var xmlName = GetSelectedXmlName();

            foreach (var entry in TableViews)
            {
                if (entry.Value.Name == xmlName)
                {
                    entry.Value.DisplayProperties();
                }
            }

            ImGui.End();
        }
    }

    public void Shortcuts()
    {
        var xmlName = GetSelectedXmlName();

        foreach (var entry in TableViews)
        {
            if (entry.Value.Name == xmlName)
            {
                entry.Value.Shortcuts();
            }
        }
    }

    public string GetSelectedXmlName()
    {
        if (EditorState.SelectedStatus == null)
            return "";

        var xmlName = EditorState.SelectedStatus.Name;

        // Only assess the relevant part of the file name (i.e. ignore PTF part)
        if (xmlName.Contains("__"))
        {
            xmlName = xmlName.Split("__")[0];
        }

        return xmlName;
    }
}
