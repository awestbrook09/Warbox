using StudioCore.Core.Data;
using StudioCore.Editors.TableEditor.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TableEditor.Tools;

public static class TableDifferenceEngine
{
    public static Dictionary<GenericTableView, bool> TableDifferenceCache = new();

    public static void Refresh()
    {
        var curTableView = Warbox.TableEditor.TableDataView.GetSelectedTableView();

        var resDesc = curTableView.ViewStatus;
        var document = curTableView.ViewDocument;

        var vanillaTable = DataHandler.Vanilla_Tables.Where(e => e.Key.Name == resDesc.Name).FirstOrDefault();

        if (vanillaTable.Value == null)
            return;

        var vanillaDocument = vanillaTable.Value;

        if(document.ToString() != vanillaDocument.ToString())
        {
            if (TableDifferenceCache.ContainsKey(curTableView))
            {
                TableDifferenceCache[curTableView] = true;
            }
            else
            {
                TableDifferenceCache.Add(curTableView, true);
            }
        }
        else
        {
            if (TableDifferenceCache.ContainsKey(curTableView))
            {
                TableDifferenceCache[curTableView] = false;
            }
            else
            {
                TableDifferenceCache.Add(curTableView, false);
            }
        }
    }
}
