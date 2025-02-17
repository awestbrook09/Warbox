using StudioCore.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StudioCore.Editors.TextEditor.Framework;

public static class TextSelection
{
    // File
    public static ResourceDescriptor FileSelectionDescriptor;
    public static XDocument FileSelectionDocument;
    public static int FileSelectionIndex = -1;

    public static bool FileArrowSelect = false;
    public static bool FocusFileSelection = false;

    public static void SelectFile(ResourceDescriptor resDesc, XDocument doc, int index = -1)
    {
        FileSelectionDescriptor = resDesc;
        FileSelectionDocument = doc;
    }

    public static void ClearFileSelection()
    {
        FileSelectionDescriptor = null;
        FileSelectionDocument = null;
    }

    // Row
    public static int RowSelectionIndex = -1;

    public static bool RowArrowSelect = false;
    public static bool FocusRowSelection = false;

    public static void SelectRow(XElement entry, int index)
    {
        RowSelectionIndex = index;
        SelectedCells = entry.Elements().ToList();
    }
    public static void ClearRowSelection()
    {
        RowSelectionIndex = -1;
    }

    public static XElement GetRowAtIndex(int index)
    {
        return FileSelectionDocument.Elements().Elements().ElementAt(index);
    }

    public static XElement GetRowContainer()
    {
        return FileSelectionDocument.Elements().FirstOrDefault();
    }

    public static IEnumerable<XElement> GetRows()
    {
        return FileSelectionDocument.Elements().Elements();
    }

    public static List<XElement> GetRowList()
    {
        return FileSelectionDocument.Elements().Elements().ToList();
    }

    // Cells
    public static List<XElement> SelectedCells = new List<XElement>();
}
