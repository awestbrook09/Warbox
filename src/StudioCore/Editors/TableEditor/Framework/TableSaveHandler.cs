using DotNext.Collections.Generic;
using Microsoft.VisualBasic;
using StudioCore.Core.Data;
using StudioCore.Editors.TextEditor.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
    public static void Export()
    {
        var status = Warbox.EditorHandler.TableEditor.FileSelectionView.GetSelectedDocumentStatus();

        foreach (var entry in DataHandler.Tables)
        {
            // Only process the current table
            if (entry.Key.Name == status.Name)
            {
                SaveTable("Source\\Data", entry.Key, entry.Value, entry.Value);
            }
        }
    }

    public static void ExportAll()
    {
        foreach (var entry in DataHandler.Tables)
        {
            SaveTable("Source\\Data", entry.Key, entry.Value, entry.Value);
        }
    }

    public static void ExportPTF()
    {
        var status = Warbox.EditorHandler.TableEditor.FileSelectionView.GetSelectedDocumentStatus();

        foreach (var entry in DataHandler.Tables)
        {
            var vanillaEntry = DataHandler.Vanilla_Tables.Where(e => e.Key.Name == entry.Key.Name).FirstOrDefault();

            if (entry.Key.Name == status.Name)
            {
                (bool, string, XDocument) result = RemoveVanillaEntries(entry.Key.Name, entry.Value, vanillaEntry.Value);

                var modName = ManifestHandler.SanitizeModName(Warbox.ProjectHandler.CurrentProject.Config.ProjectName);

                if(result.Item1 && result.Item3 != null)
                {
                    SaveTable("Source\\PTF", entry.Key, entry.Value, result.Item3, $"__{modName}");
                }
                else if (result.Item2 != "")
                {
                    TaskLogs.AddLog(result.Item2);
                }
            }
        }
    }

    public static void ExportAllPTF()
    {
        foreach (var entry in DataHandler.Tables)
        {
            var vanillaEntry = DataHandler.Vanilla_Tables.Where(e => e.Key.Name == entry.Key.Name).FirstOrDefault();

            (bool, string, XDocument) result = RemoveVanillaEntries(entry.Key.Name, entry.Value, vanillaEntry.Value);

            var modName = ManifestHandler.SanitizeModName(Warbox.ProjectHandler.CurrentProject.Config.ProjectName);

            if (result.Item1 && result.Item3 != null)
            {
                SaveTable("Source\\PTF", entry.Key, entry.Value, result.Item3, $"__{modName}");
            }
            else if(result.Item2 != "")
            {
                TaskLogs.AddLog(result.Item2);
            }
        }
    }

    private static void SaveTable(string saveDir, ResourceDescriptor resDesc, XDocument originalDocument, XDocument document, string postfix = "")
    {
        var writeDir = $"{Warbox.ProjectDataRoot}\\{saveDir}\\{resDesc.RelativeDirectory}\\";
        var writePath = $"{Warbox.ProjectDataRoot}\\{saveDir}\\{resDesc.RelativeDirectory}\\{resDesc.Name}{postfix}{resDesc.Extension}";

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

    // TODO: make this support nested tables properly, e.g. stuff where there are child of child lists
    private static (bool, string, XDocument) RemoveVanillaEntries(string name, XDocument baseDoc, XDocument vanillaDoc)
    {
        var tempDoc = new XDocument(baseDoc);

        if(vanillaDoc == null)
        {
            return (false, "No valid vanilla counterpart document.", tempDoc);
        }

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

        var editedElements = tempDoc.Elements().Elements().Descendants().Where(e => e.HasAttributes).ToList();

        if (editedElements.Count == 0)
        {
            return (false, $"No differences were found for {name}. PTF Export cancelled.", tempDoc);
        }

        return (true, "", tempDoc);
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

    public static void PackagePTF()
    {
        if(!Directory.Exists($"{Warbox.ProjectDataRoot}\\Source\\PTF\\"))
        {
            TaskLogs.AddLog($"No PTF folder exists yet.");
            return;
        }

        if(!Directory.Exists($"{Warbox.ProjectDataRoot}\\Data\\"))
        {
            Directory.CreateDirectory($"{Warbox.ProjectDataRoot}\\Data\\");
        }

        var modName = ManifestHandler.SanitizeModName(Warbox.ProjectHandler.CurrentProject.Config.ProjectName);

        // Output it in the normal Data folder so it can be read by the game
        // (assuming we are in the Game/Mods/<mod name>/ structure
        var outputPath = $"{Warbox.ProjectDataRoot}\\Data\\{modName}.pak";
        ZipDirectory($"{Warbox.ProjectDataRoot}\\Source\\PTF\\", outputPath);

        TaskLogs.AddLog($"Created PAK file from PTF files: {outputPath}");

        ManifestHandler.CreateManisfestIfMissing();
    }

    private static void ZipDirectory(string directoryPath, string zipFilePath)
    {
        // Create the ZIP archive and set the CompressionLevel
        using (FileStream zipToCreate = new FileStream(zipFilePath, FileMode.Create))
        using (ZipArchive archive = new ZipArchive(zipToCreate, ZipArchiveMode.Create))
        {
            // Get all XML files (including subdirectories)
            string[] xmlFiles = Directory.GetFiles(directoryPath, "*.xml", SearchOption.AllDirectories);

            foreach (var file in xmlFiles)
            {
                // Get the relative file path (preserve directory structure)
                string relativePath = Path.GetRelativePath(directoryPath, file);

                // Add the file to the archive
                ZipArchiveEntry entry = archive.CreateEntry(relativePath);

                // Set the time format to avoid high precision timestamps
                entry.LastWriteTime = DateTime.Now;

                using (Stream entryStream = entry.Open())
                using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    fileStream.CopyTo(entryStream);
                }
            }
        }
    }
}
