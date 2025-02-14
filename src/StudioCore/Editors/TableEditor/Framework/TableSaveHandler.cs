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

    public static void PackageAll()
    {
        if (!Directory.Exists($"{Warbox.ProjectDataRoot}\\Source\\Data\\"))
        {
            TaskLogs.AddLog($"No Data folder exists yet.");
            return;
        }

        if (!Directory.Exists($"{Warbox.ProjectDataRoot}\\Data\\"))
        {
            Directory.CreateDirectory($"{Warbox.ProjectDataRoot}\\Data\\");
        }

        var modName = ManifestHandler.SanitizeModName(Warbox.ProjectHandler.CurrentProject.Config.ProjectName);

        // Output it in the normal Data folder so it can be read by the game
        // (assuming we are in the Game/Mods/<mod name>/ structure
        var outputPath = $"{Warbox.ProjectDataRoot}\\Data\\{modName}.pak";
        ZipDirectory($"{Warbox.ProjectDataRoot}\\Source\\Data\\", outputPath);

        TaskLogs.AddLog($"Created PAK file from Data files: {outputPath}");

        ManifestHandler.CreateManisfestIfMissing();
    }


    public static void ExportPTF()
    {
        var status = Warbox.EditorHandler.TableEditor.FileSelectionView.GetSelectedDocumentStatus();

        foreach (var entry in DataHandler.Tables)
        {
            var vanillaEntry = DataHandler.Vanilla_Tables.Where(e => e.Key.Name == entry.Key.Name).FirstOrDefault();

            if (entry.Key.Name == status.Name)
            {
                (bool, string, XDocument) result = RemoveVanillaEntries(entry.Key, entry.Value, vanillaEntry.Value);

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

    private static void SaveTable(string saveDir, ResourceDescriptor resDesc, XDocument originalDocument, XDocument document, string postfix = "")
    {
        var writeDir = $"{Warbox.ProjectDataRoot}\\{saveDir}\\{resDesc.RelativeDirectory}";
        var writePath = $"{writeDir}\\{resDesc.Name}{postfix}{resDesc.Extension}";

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

            var fileName = Path.GetFileName(writePath);
            TaskLogs.AddLog($"Saved file at: {writePath}.");
            TaskLogs.AddLog($"{fileName} saved.");
        }
    }

    private static Dictionary<string, List<RemovalTag>> Removals = new();

    // TODO: fix this so it works with nested elements
    private static (bool, string, XDocument) RemoveVanillaEntries(ResourceDescriptor resDesc, XDocument baseDoc, XDocument vanillaDoc)
    {
        Removals = new();

        var tempDoc = new XDocument(baseDoc);

        if(vanillaDoc == null)
        {
            return (false, "No valid vanilla counterpart document.", tempDoc);
        }

        // Check in reverse so we can remove child first
        CheckElements(tempDoc, vanillaDoc, "3"); // Sub sub list iter
        CheckElements(tempDoc, vanillaDoc, "2"); // Sub list tier
        CheckElements(tempDoc, vanillaDoc, "1"); // Top list tier

        var diffCount = 0;

        List<XElement> RetainedElements = new();

        if (Removals.ContainsKey("3"))
        {
            var removals = Removals["3"];
            foreach(var entry in removals)
            {
                if(entry.Element.Parent != null)
                {
                    RetainedElements.Add(entry.Element.Parent);
                    entry.Element.Remove();
                    diffCount++;
                }
            }
        }

        if (Removals.ContainsKey("2"))
        {
            var removals = Removals["2"];
            foreach (var entry in removals)
            {
                if (entry.Element.Parent != null && !RetainedElements.Contains(entry.Element))
                {
                    RetainedElements.Add(entry.Element.Parent);
                    entry.Element.Remove();
                    diffCount++;
                }
            }
        }

        if (Removals.ContainsKey("1"))
        {
            var removals = Removals["1"];
            foreach (var entry in removals)
            {
                if (entry.Element.Parent != null && !RetainedElements.Contains(entry.Element))
                {
                    RetainedElements.Add(entry.Element.Parent);
                    entry.Element.Remove();
                    diffCount++;
                }
            }
        }

        // Return failed export if no differences are found
        if (diffCount == 0)
        {
            return (false, $"No differences were found for {resDesc.Name}. PTF Export cancelled.", tempDoc);
        }

        return (true, "", tempDoc);
    }

    private static void CheckElements(XDocument tempDoc, XDocument vanillaDoc, string tier)
    {
        List<XElement> baseElements = new();
        List<XElement> vanillaElements = new();

        if(tier == "1")
        {
            baseElements = tempDoc.Elements().Elements().Descendants().ToList(); 
            vanillaElements = vanillaDoc.Elements().Elements().Descendants().ToList();
        }
        if (tier == "2")
        {
            baseElements = tempDoc.Elements().Elements().Elements().Descendants().ToList();
            vanillaElements = vanillaDoc.Elements().Elements().Elements().Descendants().ToList();
        }
        if (tier == "3")
        {
            baseElements = tempDoc.Elements().Elements().Elements().Elements().Descendants().ToList();
            vanillaElements = vanillaDoc.Elements().Elements().Elements().Elements().Descendants().ToList();
        }

        foreach (var baseElement in baseElements.ToList())
        {
            // Check attributes
            if (baseElement.HasAttributes)
            {
                if (vanillaElements.Any(vanillaElement => ElementAttributesMatch(baseElement, vanillaElement)))
                {
                    if (baseElement.Parent != null && baseElement != null)
                    {
                        var newRemoval = new RemovalTag(baseElement.Parent, baseElement);
                        if (Removals.ContainsKey(tier))
                        {
                            Removals[tier].Add(newRemoval);
                        }
                        else
                        {
                            Removals.Add(tier, new List<RemovalTag>() { newRemoval });
                        }
                    }
                }
            }
            // Element value check if no attributes are used
            else if (vanillaElements.Any(vanillaElement => ElementValueMatch(baseElement, vanillaElement)))
            {
                if (baseElement.Parent != null && baseElement != null)
                {
                    var newRemoval = new RemovalTag(baseElement.Parent, baseElement);
                    if (Removals.ContainsKey(tier))
                    {
                        Removals[tier].Add(newRemoval);
                    }
                    else
                    {
                        Removals.Add(tier, new List<RemovalTag>() { newRemoval });
                    }
                }
            }
        }
    }

    private static bool ElementValueMatch(XElement baseElement, XElement vanillaElement)
    {
        // Elements must have the same name
        if (baseElement.Name != vanillaElement.Name)
            return false;

        if(baseElement.Value == vanillaElement.Value)
            return true;

        return false;
    }

    private static bool ElementAttributesMatch(XElement baseElement, XElement vanillaElement)
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
 public class RemovalTag
{
    public XElement Parent;
    public XElement Element;

    public RemovalTag(XElement parent, XElement element)
    {
        Parent = parent;
        Element = element;
    }
}
    