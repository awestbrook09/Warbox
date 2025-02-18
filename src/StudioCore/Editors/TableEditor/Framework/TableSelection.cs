using StudioCore.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StudioCore.Editors.TableEditor.Framework;

public static class TableSelection
{
    // File
    public static ResourceDescriptor FileSelectionDescriptor;
    public static XDocument FileSelectionDocument;
    public static int FileSelectionIndex = -1;

    public static bool FileArrowSelect = false;
    public static bool FocusFileSelection = false;

    public static List<string> PinnedFiles = new();

    // Row
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

    public static string GetCurrentFileName()
    {
        return FileSelectionDescriptor == null ? "" : FileSelectionDescriptor.Name;
    }
}
