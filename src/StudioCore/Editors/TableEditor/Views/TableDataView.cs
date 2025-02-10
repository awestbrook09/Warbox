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

    private SortedDictionary<string, GenericTableView> TableViews = new();
    private SortedDictionary<string, string> XmlNames = new();

    public TableDataView(TableEditorScreen screen)
    {
        Screen = screen;

        foreach (var entry in TableDefinition.Definitions)
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

            foreach (var tbl in DataHandler.Tables)
            {
                var status = tbl.Key;
                var document = tbl.Value;

                var docFullName = status.Name;
                var docName = status.Name;

                if (status.Name.Contains("__"))
                {
                    docName = status.Name.Split("__")[0];
                }

                if (docName == defName)
                {
                    var newView = new GenericTableView(screen, docFullName, aliasKey, noPrimaryKey, status, document);

                    TableViews.Add(docFullName, newView);
                }
            }
        }

        TableMeta.Setup();
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

    public GenericTableView GetSelectedTableView()
    {
        var selectedDocumentName = Screen.FileSelectionView.GetSelectedDocumentName();

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
        var selectedDocumentName = Screen.FileSelectionView.GetSelectedDocumentName();

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
        var selectedDocumentName = Screen.FileSelectionView.GetSelectedDocumentName();

        foreach (var entry in TableViews)
        {
            if (entry.Value.Name == selectedDocumentName)
            {
                entry.Value.Shortcuts();
            }
        }
    }
}
