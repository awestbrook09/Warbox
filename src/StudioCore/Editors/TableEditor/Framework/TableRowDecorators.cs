using Microsoft.VisualBasic;
using StudioCore.Core.Data;
using StudioCore.Editors.TableEditor.Views;
using StudioCore.Editors.TextEditor.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StudioCore.Editors.TableEditor.Framework;

public static class TableRowDecorators
{
    /// <summary>
    /// Fill out the alias override dictionary for the current document
    /// </summary>
    public static void ProcessAliasOverrides(GenericTableView curView)
    {
        var curLocalization = TextDataHandler.GetCurrentLocalization();

        if (curLocalization == null)
            return;

        curView.AliasOverrides = new();

        var metaDoc = TableMetaHandler.GetMetaDocument(curView.Screen.FileSelectionView.GetSelectedDocumentName());

        if (metaDoc == null)
            return;

        var targetAttribute = metaDoc.Root.Attribute("AliasOverrideAttribute");
        if (targetAttribute == null)
            return;

        var metaDocument = TableMetaHandler.GetMetaDocument(curView.ViewStatus.Name);

        if (metaDocument?.Root == null)
            return;

        var metaElementList = metaDocument.Root.Descendants("entries").Elements();
        var targetMetaEntry = metaElementList.FirstOrDefault(e => e.Name == targetAttribute.Value);

        if (targetMetaEntry == null)
            return;

        var textRef = targetMetaEntry.Attribute("TextRef")?.Value;
        if (string.IsNullOrEmpty(textRef))
            return;

        var targetFile = curLocalization.FirstOrDefault(e => e.Key.Name == textRef).Value;
        if (targetFile?.Root == null)
            return;

        // Preload rows for fast lookup
        var rowDictionary = targetFile.Root.Elements("Row")
            .Select(row => row.Elements("Cell").ToList())
            .Where(cells => cells.Count >= 3)
            .GroupBy(cells => cells[0].Value)
            .ToDictionary(group => group.Key, group => group.Last()[2].Value);

        int index = 0;
        foreach(var element in curView.GetContents())
        {
            var elementEntry = element;

            var attribute = elementEntry.Attribute(targetAttribute.Value);
            if (attribute == null)
            {
                continue;
            }

            rowDictionary.TryGetValue(attribute.Value, out string displayedName);
            curView.AliasOverrides[index] = displayedName ?? "";

            index++;
        }
    }

}
