using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using static Assimp.Metadata;
using StudioCore.Editors.TableEditor.Framework;

namespace StudioCore.Core.Data;

public static class DataHandler
{
    public static Dictionary<string, SortedDictionary<ResourceDescriptor, XDocument>> Localization = new();

    /// <summary>
    /// Holds our working tables
    /// </summary>
    public static SortedDictionary<ResourceDescriptor, XDocument> Tables = new();

    /// <summary>
    /// Holds the base tables, used for comparison to generate PTF output
    /// </summary>
    public static SortedDictionary<ResourceDescriptor, XDocument> Vanilla_Tables = new();

    //public Dictionary<DataStatus, XDocument> Scripts = new Dictionary<DataStatus, XDocument>();

    public static void SetupTables()
    {
        if (Warbox.DataRoot != "" && Warbox.ProjectDataRoot != "")
        {
            Tables = ReadTables("Data", "Tables");
            Vanilla_Tables = ReadTables("Data", "Tables", true);
        }
    }

    public static void SetupTableViews()
    {
        Warbox.EditorHandler.TableEditor.TableDataView.SetupTableViews();
        TableMeta.Setup();
    }

    public static void SetupLocalization()
    {
        Localization = new();

        if (Warbox.DataRoot != "" && Warbox.ProjectDataRoot != "")
        {
            Localization.Add("English", ReadLocalization("Localization", "English_xml"));

            if(CFG.Current.TextEditor_EnableLanguage_ChineseSimplified)
                Localization.Add("Chinese (Simplified)", ReadLocalization("Localization", "Chineses_xml"));

            if (CFG.Current.TextEditor_EnableLanguage_ChineseTraditional)
                Localization.Add("Chinese (Traditional)", ReadLocalization("Localization", "Chineset_xml"));

            if (CFG.Current.TextEditor_EnableLanguage_Czech)
                Localization.Add("Czech", ReadLocalization("Localization", "Czech_xml"));

            if (CFG.Current.TextEditor_EnableLanguage_French)
                Localization.Add("French", ReadLocalization("Localization", "French_xml"));

            if (CFG.Current.TextEditor_EnableLanguage_German)
                Localization.Add("German", ReadLocalization("Localization", "German_xml"));

            if (CFG.Current.TextEditor_EnableLanguage_Italian)
                Localization.Add("Italian", ReadLocalization("Localization", "Italian_xml"));

            if (CFG.Current.TextEditor_EnableLanguage_Japanese)
                Localization.Add("Japanese", ReadLocalization("Localization", "Japanese_xml"));

            if (CFG.Current.TextEditor_EnableLanguage_Korean)
                Localization.Add("Korean", ReadLocalization("Localization", "Korean_xml"));

            if (CFG.Current.TextEditor_EnableLanguage_Polish)
                Localization.Add("Polish", ReadLocalization("Localization", "Polish_xml"));

            if (CFG.Current.TextEditor_EnableLanguage_Portuguese)
                Localization.Add("Portuguese", ReadLocalization("Localization", "Portuguese_xml"));

            if (CFG.Current.TextEditor_EnableLanguage_Russian)
                Localization.Add("Russian", ReadLocalization("Localization", "Russian_xml"));

            if (CFG.Current.TextEditor_EnableLanguage_Spanish)
                Localization.Add("Spanish", ReadLocalization("Localization", "Spanish_xml"));

            if (CFG.Current.TextEditor_EnableLanguage_Turkish)
                Localization.Add("Turkish", ReadLocalization("Localization", "Turkish_xml"));

            if (CFG.Current.TextEditor_EnableLanguage_Ukrainian)
                Localization.Add("Ukrainian", ReadLocalization("Localization", "Ukrainian_xml"));
        }
    }

    public static SortedDictionary<ResourceDescriptor, XDocument> GetCurrentLocalization()
    {
        if (Localization.Count < 1)
        {
            return null;
        }

        return Localization[CFG.Current.TextEditor_CurrentLanguage];
    }

    public static List<string> GetLanguageOptions()
    {
        var options = new List<string>
        {
            "English"
        };

        if (CFG.Current.TextEditor_EnableLanguage_ChineseSimplified)
            options.Add("Chinese (Simplified)");

        if (CFG.Current.TextEditor_EnableLanguage_ChineseTraditional)
            options.Add("Chinese (Traditional)");

        if (CFG.Current.TextEditor_EnableLanguage_Czech)
            options.Add("Czech");

        if (CFG.Current.TextEditor_EnableLanguage_French)
            options.Add("French");

        if (CFG.Current.TextEditor_EnableLanguage_German)
            options.Add("German");

        if (CFG.Current.TextEditor_EnableLanguage_Italian)
            options.Add("Italian");

        if (CFG.Current.TextEditor_EnableLanguage_Japanese)
            options.Add("Japanese");

        if (CFG.Current.TextEditor_EnableLanguage_Korean)
            options.Add("Korean");

        if (CFG.Current.TextEditor_EnableLanguage_Polish)
            options.Add("Polish");

        if (CFG.Current.TextEditor_EnableLanguage_Portuguese)
            options.Add("Portuguese");

        if (CFG.Current.TextEditor_EnableLanguage_Russian)
            options.Add("Russian");

        if (CFG.Current.TextEditor_EnableLanguage_Spanish)
            options.Add("Spanish");

        if (CFG.Current.TextEditor_EnableLanguage_Turkish)
            options.Add("Turkish");

        if (CFG.Current.TextEditor_EnableLanguage_Ukrainian)
            options.Add("Ukrainian");

        return options;
    }

    public static SortedDictionary<ResourceDescriptor, XDocument> ReadTables(string folderName, string pakName, bool ignoreProject = false)
    {
        if (Warbox.DataRoot == "")
            return new SortedDictionary<ResourceDescriptor, XDocument>();

        var dataDir = $"{Warbox.DataRoot}\\{folderName}\\{pakName}.pak";
        var projectDir = $"{Warbox.ProjectDataRoot}\\Source\\{folderName}\\";

        var baseData = ReadXmlFromZip(dataDir);
        var projectData = ReadXmlFromDirectory(projectDir);
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

    public static SortedDictionary<ResourceDescriptor, XDocument> ReadLocalization(string folderName, string pakName, bool ignoreProject = false)
    {
        if (Warbox.DataRoot == "")
            return new SortedDictionary<ResourceDescriptor, XDocument>();

        var dataDir = $"{Warbox.DataRoot}\\{folderName}\\{pakName}.pak";
        var projectDir = $"{Warbox.ProjectDataRoot}\\Source\\{folderName}\\{CFG.Current.TextEditor_CurrentLanguage}";

        var baseData = ReadXmlFromZip(dataDir);
        var projectData = ReadXmlFromDirectory(projectDir);
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
                        if(finalData.ContainsKey(pEntry.Key))
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
                if(!hasProjectVersion)
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

    private static SortedDictionary<ResourceDescriptor, XDocument> ReadXmlFromZip(string zipPath)
    {
        var xmlFiles = new SortedDictionary<ResourceDescriptor, XDocument>();

        try
        {
            using (FileStream zipStream = new FileStream(zipPath, FileMode.Open, FileAccess.Read))
            using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        using (Stream entryStream = entry.Open())
                        {
                            try
                            {
                                string xmlContent;
                                Encoding encoding = DetectEncoding(entryStream, out xmlContent);

                                using (StringReader stringReader = new StringReader(xmlContent))
                                {
                                    XDocument xmlDoc = XDocument.Load(stringReader);

                                    var resDescriptor = new ResourceDescriptor(entry.FullName);

                                    xmlFiles[resDescriptor] = xmlDoc;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error reading {entry.FullName}: {ex.Message}");
                            }
                        }
                    }
                }
            }
        }
        catch { }

        return xmlFiles;
    }

    public static SortedDictionary<ResourceDescriptor, XDocument> ReadXmlFromDirectory(string directoryPath)
    {
        var xmlFiles = new SortedDictionary<ResourceDescriptor, XDocument>();

        try
        {
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine($"Directory '{directoryPath}' does not exist.");
                return xmlFiles;
            }

            foreach (string filePath in Directory.GetFiles(directoryPath, "*.xml", SearchOption.AllDirectories))
            {
                try
                {
                    string xmlContent;
                    Encoding encoding = DetectEncoding(filePath, out xmlContent);

                    using (StringReader stringReader = new StringReader(xmlContent))
                    {
                        XDocument xmlDoc = XDocument.Load(stringReader);

                        var resDescriptor = new ResourceDescriptor(filePath);
                        xmlFiles[resDescriptor] = xmlDoc;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading {filePath}: {ex.Message}");
                }
            }
        }
        catch { }

        return xmlFiles;
    }

    private static Encoding DetectEncoding(Stream stream, out string xmlContent)
    {
        using (StreamReader reader = new StreamReader(stream, Encoding.Default, detectEncodingFromByteOrderMarks: true))
        {
            xmlContent = reader.ReadToEnd();

            var match = System.Text.RegularExpressions.Regex.Match(xmlContent, @"<\?xml\s+.*?encoding=['""](.+?)['""]", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success)
            {
                try
                {
                    return Encoding.GetEncoding(match.Groups[1].Value);
                }
                catch (ArgumentException)
                {
                    Console.WriteLine($"Warning: Unsupported encoding '{match.Groups[1].Value}', defaulting to UTF-8.");
                }
            }

            return Encoding.UTF8; 
        }
    }

    private static Encoding DetectEncoding(string filePath, out string xmlContent)
    {
        using (StreamReader reader = new StreamReader(filePath, Encoding.Default, detectEncodingFromByteOrderMarks: true))
        {
            xmlContent = reader.ReadToEnd();

            // Check for encoding declaration inside XML
            var match = System.Text.RegularExpressions.Regex.Match(xmlContent, @"<\?xml\s+.*?encoding=['""](.+?)['""]", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success)
            {
                try
                {
                    return Encoding.GetEncoding(match.Groups[1].Value);
                }
                catch (ArgumentException)
                {
                    Console.WriteLine($"Warning: Unsupported encoding '{match.Groups[1].Value}' in {filePath}, defaulting to UTF-8.");
                }
            }

            return Encoding.UTF8; // Default to UTF-8 if encoding is not specified
        }
    }

    public static void ZipXmlFiles(string sourceDirectory, string zipFilePath)
    {
        if (!Directory.Exists(sourceDirectory))
            throw new DirectoryNotFoundException($"Source directory not found: {sourceDirectory}");

        string[] xmlFiles = Directory.GetFiles(sourceDirectory, "*.xml", SearchOption.AllDirectories);

        if (xmlFiles.Length == 0)
            throw new Exception("No XML files found in the directory.");

        using (FileStream zipToCreate = new FileStream(zipFilePath, FileMode.Create))
        using (ZipArchive archive = new ZipArchive(zipToCreate, ZipArchiveMode.Create))
        {
            foreach (string file in xmlFiles)
            {
                string relativePath = Path.GetRelativePath(sourceDirectory, file);

                archive.CreateEntryFromFile(file, relativePath);
            }
        }
    }
}
