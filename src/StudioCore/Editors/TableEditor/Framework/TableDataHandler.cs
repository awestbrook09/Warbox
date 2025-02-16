using DotNext.Collections.Generic;
using Microsoft.VisualBasic;
using StudioCore.Core.Data;
using StudioCore.Editors.TextEditor.Views;
using StudioCore.Platform;
using StudioCore.Utilities;
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
public static class TableDataHandler
{
    public static SortedDictionary<ResourceDescriptor, XDocument> Tables = new();
    public static SortedDictionary<ResourceDescriptor, XDocument> Vanilla_Tables = new();

    public static void Reset()
    {
        Tables = new();
        Vanilla_Tables = new();
    }

    public static void Setup()
    {
        Tables = new();
        Vanilla_Tables = new();

        if (Warbox.Project.IsValid())
        {
            Tables = ReadTables("Data", "Tables");
            Vanilla_Tables = ReadTables("Data", "Tables", true);

            TableMetaHandler.Setup();
            Warbox.TableEditor.TableDataView.SetupTableViews();
        }
    }

    public static SortedDictionary<ResourceDescriptor, XDocument> ReadTables(string folderName, string pakName, bool ignoreProject = false)
    {
        var dataDir = $"{Warbox.Project.GameDirectory}\\{folderName}\\{pakName}.pak";
        var projectDir = $"{Warbox.Project.ProjectDirectory}\\Source\\{folderName}\\";

        var baseData = XmlUtils.ReadXmlFromZip(dataDir);
        var projectData = XmlUtils.ReadXmlFromDirectory(projectDir);
        var finalData = new SortedDictionary<ResourceDescriptor, XDocument>();

        // Replace entries with project data if present
        if (projectData.Count > 0 && !ignoreProject)
        {
            foreach (var bEntry in baseData)
            {
                var bDataStatus = bEntry.Key;
                var hasProjectVersion = false;

                foreach (var pEntry in projectData)
                {
                    var pDataStatus = pEntry.Key;

                    // Is match for existing file, override
                    if (bDataStatus.Name == pDataStatus.Name)
                    {
                        hasProjectVersion = true;

                        pEntry.Key.IsProjectData = true;
                        if (finalData.ContainsKey(pEntry.Key))
                        {
                            finalData[pEntry.Key] = pEntry.Value;
                        }
                    }
                    // Is unique to project, new file
                    else
                    {
                        if (!finalData.ContainsKey(pEntry.Key))
                        {
                            finalData.Add(pDataStatus, pEntry.Value);
                        }
                    }
                }

                // Is not affected by project, vanilla
                if (!hasProjectVersion)
                {
                    finalData.Add(bDataStatus, bEntry.Value);
                }
            }
        }
        else
        {
            finalData = baseData;
        }

        return finalData;
    }


    public static void Export()
    {
        var status = Warbox.TableEditor.FileSelectionView.GetSelectedDocumentStatus();

        foreach (var entry in Tables)
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
        foreach (var entry in Tables)
        {
            SaveTable("Source\\Data", entry.Key, entry.Value, entry.Value);
        }
    }

    public static void PackageAll()
    {
        if (!Directory.Exists($"{Warbox.Project.ProjectDirectory}\\Source\\Data\\"))
        {
            TaskLogs.AddLog($"No Data folder exists yet.");
            return;
        }

        if (!Directory.Exists($"{Warbox.Project.ProjectDirectory}\\Data\\"))
        {
            Directory.CreateDirectory($"{Warbox.Project.ProjectDirectory}\\Data\\");
        }

        // Output it in the normal Data folder so it can be read by the game
        // (assuming we are in the Game/Mods/<mod name>/ structure
        var outputPath = $"{Warbox.Project.ProjectDirectory}\\Data\\{Warbox.Project.ProjectID}.pak";
        XmlUtils.ZipDirectory($"{Warbox.Project.ProjectDirectory}\\Source\\Data\\", outputPath);

        TaskLogs.AddLog($"Created PAK file from Data files: {outputPath}");

        ManifestHandler.CreateManisfestIfMissing();
    }

    private static List<string> ExcludedTables = new List<string>();

    public static void ExportPTF()
    {
        var status = Warbox.TableEditor.FileSelectionView.GetSelectedDocumentStatus();

        if(ExcludedTables.Contains(status.Name))
        {
            PlatformUtils.Instance.MessageBox("This table does not support patching.", "Warning", MessageBoxButtons.OK);
            return;
        }

        foreach (var entry in Tables)
        {
            var vanillaEntry = Vanilla_Tables.Where(e => e.Key.Name == entry.Key.Name).FirstOrDefault();

            if (entry.Key.Name == status.Name)
            {
                (bool, string, XDocument) result = BuildOutputDocument(entry.Key, entry.Value, vanillaEntry.Value);
                //(bool, string, XDocument) result = RemoveVanillaEntries(entry.Key, entry.Value, vanillaEntry.Value);

                if(result.Item1 && result.Item3 != null)
                {
                    SaveTable("PTF\\Data", entry.Key, entry.Value, result.Item3, $"__{Warbox.Project.ProjectID}");
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
        var writeDir = $"{Warbox.Project.ProjectDirectory}\\{saveDir}\\{resDesc.RelativeDirectory}";
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

    private static (bool, string, XDocument) BuildOutputDocument(ResourceDescriptor resDesc, XDocument baseDoc, XDocument vanillaDoc)
    {
        var tableDef = TableMetaHandler.TableMetaDefinition.Where(e => e.Attribute("Name").Value == resDesc.BaseName).FirstOrDefault();

        var tempDoc = new XDocument(baseDoc);
        var outputDoc = new XDocument(baseDoc);

        if (tableDef == null)
        {
            return (true, "Table is not defined in TableDefinitions.xml.", tempDoc);
        }

        // Remove all the existing entries in the output doc
        XElement container = outputDoc.Elements().Elements().FirstOrDefault();
        if(container == null)
        {
            return (true, "Failed to find XML container element", tempDoc);
        }

        container.Elements().Remove();

        // This is used to link X entry (base) with Y entry (vanilla) so the attributes can then be compared
        var primaryKeyAttribute = tableDef.Attribute("RowNameKey");

        // If the primary key is not defined at all, return.
        if (primaryKeyAttribute == null)
        {
            return (true, "RowNameKey is not set in TableDefinitions.xml for this table.", tempDoc);
        }

        // If the primary key is blank, this table cannot be supported.
        if(primaryKeyAttribute.Value == "")
        {
            return (true, "Patching is not supported for this table.", tempDoc);
        }

        // Find entries that should be added to output doc 
        var baseEntries = tempDoc.Elements().Elements().Elements().ToList();
        var vanillaEntries = vanillaDoc.Elements().Elements().Elements().ToList();

        foreach (var entry in baseEntries)
        {
            var addEntry = false;

            var keyAtttribute = entry.Attribute(primaryKeyAttribute.Value);

            if (keyAtttribute == null)
                continue;

            var primaryKey = keyAtttribute.Value;

            // Get the vanilla entry based on the primary key
            var vanillaEntry = vanillaEntries.Where(
                e => e.Attribute(primaryKeyAttribute.Value) != null &&
                e.Attribute(primaryKeyAttribute.Value).Value == primaryKey).FirstOrDefault();

            // If no vanilla entry exists, we can assume that the base entry is unique, and thus should be added
            if (vanillaEntry == null)
            {
                addEntry = true;
            }
            else
            {
                // Add if the value of the element is different
                if (entry.Value != vanillaEntry.Value)
                {
                    addEntry = true;
                }

                // Attribute Value check
                var baseAttributes = entry.Attributes().ToList();
                var vanillaAttributes = vanillaEntry.Attributes().ToList();

                foreach (var bAttribute in baseAttributes)
                {
                    var attributeName = bAttribute.Name;
                    var vanillaEqual = vanillaAttributes.Where(e => e.Name == attributeName).FirstOrDefault();

                    // If no vanilla entry attribute exists, we can assume that an attribute has been added, and so this entry should be added
                    if (vanillaEqual == null)
                    {
                        addEntry = true;
                    }
                    else
                    {
                        // Add if the value of the attribute is different
                        if (bAttribute.Value != vanillaEqual.Value)
                        {
                            addEntry = true;
                        }
                    }
                }
            }

            // Add the entry to the output doc if there is a difference found
            if (addEntry)
            {
                container.Add(entry);
            }
        }

        return (true, "", outputDoc);
    }
    
    public static void PackagePTF()
    {
        if(!Directory.Exists($"{Warbox.Project.ProjectDirectory}\\PTF\\Data"))
        {
            TaskLogs.AddLog($"No PTF folder exists yet.");
            return;
        }

        if(!Directory.Exists($"{Warbox.Project.ProjectDirectory}\\Data\\"))
        {
            Directory.CreateDirectory($"{Warbox.Project.ProjectDirectory}\\Data\\");
        }

        // Output it in the normal Data folder so it can be read by the game
        // (assuming we are in the Game/Mods/<mod name>/ structure
        var outputPath = $"{Warbox.Project.ProjectDirectory}\\Data\\{Warbox.Project.ProjectID}.pak";
        XmlUtils.ZipDirectory($"{Warbox.Project.ProjectDirectory}\\PTF\\Data", outputPath);

        TaskLogs.AddLog($"Created PAK file from PTF files: {outputPath}");

        ManifestHandler.CreateManisfestIfMissing();
    }
}

    