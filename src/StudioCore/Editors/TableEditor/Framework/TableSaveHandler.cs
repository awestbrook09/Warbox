using DotNext.Collections.Generic;
using Microsoft.VisualBasic;
using StudioCore.Core.Data;
using StudioCore.Editors.TextEditor.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Assimp.Metadata;

namespace StudioCore.Editors.TableEditor.Framework;
public static class TableSaveHandler
{
    /// <summary>
    /// Export the current Table document as its own XML file (if changes are present)
    /// </summary>
    public static void Export()
    {
        var status = Warbox.EditorHandler.TableEditor.FileSelectionView.GetSelectedDocumentStatus();

        foreach (var entry in DataHandler.Tables)
        {
            var vanillaEntry = DataHandler.Vanilla_Tables.Where(e => e.Key.Name == entry.Key.Name).FirstOrDefault();

            // Only process the current table
            if (entry.Key.Name == status.Name)
            {
                SaveTable(entry, vanillaEntry, "Data");
            }
        }
    }

    /// <summary>
    /// Export the all Table documents as their own XML file (if changes are present)
    /// </summary>
    public static void ExportAll()
    {
        foreach (var entry in DataHandler.Tables)
        {
            var vanillaEntry = DataHandler.Vanilla_Tables.Where(e => e.Key.Name == entry.Key.Name).FirstOrDefault();

            SaveTable(entry, vanillaEntry, "Data");
        }
    }

    /// <summary>
    /// Export the all Table documents as their own XML file  (if changes are present),
    /// only including the explicit entries that have been changed.
    /// </summary>
    public static void ExportPTF()
    {
        foreach (var entry in DataHandler.Tables)
        {
            var vanillaEntry = DataHandler.Vanilla_Tables.Where(e => e.Key.Name == entry.Key.Name).FirstOrDefault();

            SaveTable(entry, vanillaEntry, "PTF", true);
        }
    }

    private static (XDocument, int) GetUniqueEntries(XDocument primaryDoc, XDocument vanillaDoc)
    {
        // Database -> List -> Entries
        var vanillaElements = new HashSet<string>(vanillaDoc.Elements().Elements().Elements().Select(NormalizeElement));
        var primaryElements = primaryDoc.Elements().Elements().Elements().Where(e => !vanillaElements.Contains(NormalizeElement(e)));
        var primaryElementCount = primaryElements.Count();

        var primaryElementHeader = primaryDoc.Elements().Elements().FirstOrDefault();
        if(primaryElementHeader != null)
        {
            primaryElementHeader.RemoveAll();
            foreach(var entry in primaryElements)
            {
                primaryElementHeader.Add(entry);
            }
        }

        var newDocument = new XDocument(
            new XDeclaration("1.0", "us-ascii", null),
            new XElement("database",
                new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
                new XAttribute("name", "barbora"),
                new XAttribute(XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance") + "noNamespaceSchemaLocation", "../database.xsd"),
                primaryElementHeader
            )
        );

        return (newDocument, primaryElementCount);
    }

    private static string NormalizeElement(XElement element)
    {
        // Sort attributes by name to ensure consistent representation
        string attributes = string.Join(" ", element.Attributes()
            .OrderBy(a => a.Name.ToString())
            .Select(a => $"{a.Name}='{a.Value}'"));

        // Create a normalized string representation
        return $"<{element.Name} {attributes}>{element.Value.Trim()}</{element.Name}>";
    }

    public static string SanitizeModName(string input)
    {
        if (input == null) 
            return string.Empty;

        string result = input.Replace(' ', '_');

        result = Regex.Replace(result, @"[^a-zA-Z0-9_]", "");

        return result;
    }


    /// <summary>
    /// Handles the comaprison between the current table and its vanilla equal.
    /// </summary>
    private static void SaveTable(KeyValuePair<DataStatus, XDocument> currentEntry, KeyValuePair<DataStatus, XDocument> vanillaEntry, string exportDir, bool saveAsPTF = false)
    {
        var modName = SanitizeModName(Warbox.ProjectHandler.CurrentProject.Config.ProjectName);

        var curStatus = currentEntry.Key;
        var curDocument = currentEntry.Value;

        var vanillaStatus = vanillaEntry.Key;
        var vanillaDocument = vanillaEntry.Value;

        // This contains the new 'edits' only document for writing when saving as a PTF
        var result = GetUniqueEntries(curDocument, vanillaDocument);
        var compareDocument = result.Item1;
        var compareDifferenceCount = result.Item2;

        if (compareDifferenceCount > 0)
        {
            // Save
            var outputDir = $"{Warbox.ProjectDataRoot}\\{exportDir}";

            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            var newStatus = new DataStatus(curStatus);
            newStatus.Name = $"{newStatus.Name}__testmod";

            var writePath = newStatus.Path.Replace($"{Warbox.ProjectDataRoot}\\Data", "");
            var fileDir = $"{outputDir}\\{writePath}";

            // If it is a project-specific file, use the status Path as it is a full path
            if (writePath.Contains(outputDir))
            {
                fileDir = $"{writePath}";
            }

            var fileOutputDir = Path.GetDirectoryName(fileDir);

            if (!Directory.Exists(fileOutputDir))
                Directory.CreateDirectory(fileOutputDir);

            // If saving as PTF, only include the changed lines
            if (saveAsPTF)
            {
                compareDocument.Save(fileDir);
            }
            // Otherwise, save the entire table.
            else
            {
                curDocument.Save(fileDir);
            }
            TaskLogs.AddLog($"{fileDir} saved.");
        }
    }

}
