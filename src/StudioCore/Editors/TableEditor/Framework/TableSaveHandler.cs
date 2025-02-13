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
using static DotNext.Threading.Tasks.DynamicTaskAwaitable;

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
            // Only process the current table
            if (entry.Key.Name == status.Name)
            {
                SaveTable(entry.Key, entry.Value, entry.Value);
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
            SaveTable(entry.Key, entry.Value, entry.Value);
        }
    }

    /// <summary>
    /// Export the all Table documents as their own XML file  (if changes are present),
    /// only including the explicit entries that have been changed.
    /// </summary>
    public static void ExportPTF()
    {
        var status = Warbox.EditorHandler.TableEditor.FileSelectionView.GetSelectedDocumentStatus();

        foreach (var entry in DataHandler.Tables)
        {
            var vanillaEntry = DataHandler.Vanilla_Tables.Where(e => e.Key.Name == entry.Key.Name).FirstOrDefault();

            if (entry.Key.Name == status.Name)
            {
                XDocument result = RemoveVanillaEntries(entry.Value, vanillaEntry.Value);

                var modName = SanitizeModName(Warbox.ProjectHandler.CurrentProject.Config.ProjectName);

                if(result != null)
                {
                    SaveTable(entry.Key, entry.Value, result, $"__{modName}");
                }
            }
        }
    }

    private static void SaveTable(ResourceDescriptor resDesc, XDocument originalDocument, XDocument document, string postfix = "")
    {
        var writeDir = $"{Warbox.ProjectDataRoot}\\Data\\{resDesc.RelativeDirectory}\\";
        var writePath = $"{Warbox.ProjectDataRoot}\\Data\\{resDesc.RelativeDirectory}\\{resDesc.Name}{postfix}{resDesc.Extension}";

        if (!Directory.Exists(writeDir))
            Directory.CreateDirectory(writeDir);

        if (originalDocument.Declaration.Encoding != null)
        {
            if (originalDocument.Declaration.Encoding.ToLower() == "us-ascii")
            {
                using (var writer = new StreamWriter(writePath, false, Encoding.ASCII))
                {
                    document.Save(writer);
                }
            }
            else if (originalDocument.Declaration.Encoding.ToLower() == "windows-1252")
            {
                using (var writer = new StreamWriter(writePath, false, Encoding.ASCII))
                {
                    document.Save(writer);
                }
            }
            else if (originalDocument.Declaration.Encoding.ToLower() == "utf-8")
            {
                using (var writer = new StreamWriter(writePath, false, new UTF8Encoding(false)))
                {
                    document.Save(writer);
                }
            }
            else
            {
                TaskLogs.AddLog($"Unsupported encoding: {document.Declaration.Encoding}");
            }

            TaskLogs.AddLog($"{writePath} saved.");
        }
    }

    public static string SanitizeModName(string input)
    {
        if (input == null) 
            return string.Empty;

        string result = input.Replace(' ', '_');

        result = Regex.Replace(result, @"[^a-zA-Z0-9_]", "");

        return result.ToLower();
    }

    private static XDocument RemoveVanillaEntries(XDocument baseDoc, XDocument vanillaDoc)
    {
        var tempDoc = new XDocument(baseDoc);

        // Database -> Header -> Entries
        var baseElements = tempDoc.Elements().Elements().Descendants().Where(e => e.HasAttributes).ToList();
        var vanillaElements = vanillaDoc.Elements().Elements().Descendants().Where(e => e.HasAttributes).ToList();

        foreach (var baseElement in baseElements.ToList())
        {
            if (vanillaElements.Any(vanillaElement => ElementsMatch(baseElement, vanillaElement)))
            {
                baseElement.Remove();
            }
        }

        return tempDoc;
    }

    private static bool ElementsMatch(XElement baseElement, XElement vanillaElement)
    {
        // Elements must have the same name
        if (baseElement.Name != vanillaElement.Name)
            return false;

        var baseAttributes = baseElement.Attributes().OrderBy(a => a.Name.ToString()).ToList();
        var vanillaAttributes = vanillaElement.Attributes().OrderBy(a => a.Name.ToString()).ToList();

        // The number of attributes must be the same
        if (baseAttributes.Count != vanillaAttributes.Count)
            return false;

        // All attributes and their values must match
        return baseAttributes.Zip(vanillaAttributes, (b, v) => b.Name == v.Name && b.Value == v.Value).All(match => match);
    }
}
