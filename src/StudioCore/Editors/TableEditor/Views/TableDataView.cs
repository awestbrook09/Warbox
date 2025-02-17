using ImGuiNET;
using StudioCore.Core.Data;
using StudioCore.Editors.TableEditor.Framework;
using StudioCore.Editors.TextEditor;
using StudioCore.Interface;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StudioCore.Editors.TableEditor.Views;

public class TableDataView
{
    private TableEditorScreen Screen;

    public SortedDictionary<string, GenericTableView> TableViews = new();

    public TableDataView(TableEditorScreen screen)
    {
        Screen = screen;
    }

    public void SetupTableViews()
    {
        TableViews = new();

        foreach (var entry in TableMetaHandler.TableMetaDefinition)
        {
            var defName = entry.Attribute("Name").Value;
            var aliasKey = entry.Attribute("AliasNameKey").Value;
            var rowKey = entry.Attribute("RowNameKey").Value;

            // This is set for XMLs that don't have a clear primary key,
            // instead they will be displayed as Entry 1, 2, etc in the selection list
            var noPrimaryKey = false;
            var npk = entry.Attribute("NoPrimaryKey");
            if (npk != null)
            {
                noPrimaryKey = true;
            }

            foreach (var tbl in TableDataHandler.Tables)
            {
                var docFullName = tbl.Key.Name;
                var docName = tbl.Key.Name;

                if (tbl.Key.Name.Contains("__"))
                {
                    docName = tbl.Key.Name.Split("__")[0];
                }

                if (docName == defName)
                {
                    var newView = new GenericTableView(Screen, docFullName, aliasKey, noPrimaryKey, tbl.Key, tbl.Value);

                    TableViews.Add(docFullName, newView);
                }
            }
        }
    }

    public SortedDictionary<string, GenericTableView> GetTableViews()
    {
        return TableViews;
    }

    public void RefreshTableViews()
    {
        foreach (var entry in TableViews)
        {
            entry.Value.Refresh();
        }
    }

    public GenericTableView GetSpecificTableView(string name)
    {
        if (TableViews.ContainsKey(name))
        {
            return TableViews[name];
        }
        else
        {
            return null;
        }
    }

    public GenericTableView GetSelectedTableView()
    {
        var selectedDocumentName = TableSelection.GetCurrentFileName();

        if (TableViews.ContainsKey(selectedDocumentName))
        {
            return TableViews[selectedDocumentName];
        }
        else
        {
            return null;
        }
    }

    public void Display()
    {
        if (!Warbox.Project.IsValid())
            return;

        var selectedDocumentName = TableSelection.GetCurrentFileName();

        if (ImGui.Begin("Rows##tableRowView"))
        {
            foreach (var entry in TableViews)
            {
                if (entry.Value.Name == selectedDocumentName)
                {
                    entry.Value.DisplayEntries();
                }
            }

            ImGui.End();
        }


        if (ImGui.Begin("Properties##tablePropertyView"))
        {
            foreach (var entry in TableViews)
            {
                if (entry.Value.Name == selectedDocumentName)
                {
                    entry.Value.DisplayProperties();
                }
            }

            ImGui.End();
        }
    }

    public void Shortcuts()
    {
        var selectedDocumentName = TableSelection.GetCurrentFileName();

        foreach (var entry in TableViews)
        {
            if (entry.Value.Name == selectedDocumentName)
            {
                entry.Value.Shortcuts();
            }
        }
    }
}
