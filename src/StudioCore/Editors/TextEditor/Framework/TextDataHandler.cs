using StudioCore.Core.Data;
using StudioCore.Editors.TableEditor.Framework;
using StudioCore.Editors.TextEditor.Views;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StudioCore.Editors.TextEditor.Framework;

public static class TextDataHandler
{
    public static Dictionary<string, SortedDictionary<ResourceDescriptor, XDocument>> Localization = new();

    public static Dictionary<string, SortedDictionary<ResourceDescriptor, XDocument>> Vanilla_Localization = new();

    public static void Reset()
    {
        Localization = new();
        Vanilla_Localization = new();
    }

    public static void Setup()
    {
        Localization = new();
        Vanilla_Localization = new();

        if (Warbox.Project.IsValid())
        {
            Localization.Add("English", ReadLocalization("Localization", "English_xml"));

            if (CFG.Current.TextEditor_EnableLanguage_ChineseSimplified)
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

            // Vanilla Localization
            Vanilla_Localization.Add("English", ReadLocalization("Localization", "English_xml", true));

            if (CFG.Current.TextEditor_EnableLanguage_ChineseSimplified)
                Vanilla_Localization.Add("Chinese (Simplified)", ReadLocalization("Localization", "Chineses_xml", true));

            if (CFG.Current.TextEditor_EnableLanguage_ChineseTraditional)
                Vanilla_Localization.Add("Chinese (Traditional)", ReadLocalization("Localization", "Chineset_xml", true));

            if (CFG.Current.TextEditor_EnableLanguage_Czech)
                Vanilla_Localization.Add("Czech", ReadLocalization("Localization", "Czech_xml", true));

            if (CFG.Current.TextEditor_EnableLanguage_French)
                Vanilla_Localization.Add("French", ReadLocalization("Localization", "French_xml", true));

            if (CFG.Current.TextEditor_EnableLanguage_German)
                Vanilla_Localization.Add("German", ReadLocalization("Localization", "German_xml", true));

            if (CFG.Current.TextEditor_EnableLanguage_Italian)
                Vanilla_Localization.Add("Italian", ReadLocalization("Localization", "Italian_xml", true));

            if (CFG.Current.TextEditor_EnableLanguage_Japanese)
                Vanilla_Localization.Add("Japanese", ReadLocalization("Localization", "Japanese_xml", true));

            if (CFG.Current.TextEditor_EnableLanguage_Korean)
                Vanilla_Localization.Add("Korean", ReadLocalization("Localization", "Korean_xml", true));

            if (CFG.Current.TextEditor_EnableLanguage_Polish)
                Vanilla_Localization.Add("Polish", ReadLocalization("Localization", "Polish_xml", true));

            if (CFG.Current.TextEditor_EnableLanguage_Portuguese)
                Vanilla_Localization.Add("Portuguese", ReadLocalization("Localization", "Portuguese_xml", true));

            if (CFG.Current.TextEditor_EnableLanguage_Russian)
                Vanilla_Localization.Add("Russian", ReadLocalization("Localization", "Russian_xml", true));

            if (CFG.Current.TextEditor_EnableLanguage_Spanish)
                Vanilla_Localization.Add("Spanish", ReadLocalization("Localization", "Spanish_xml", true));

            if (CFG.Current.TextEditor_EnableLanguage_Turkish)
                Vanilla_Localization.Add("Turkish", ReadLocalization("Localization", "Turkish_xml", true));

            if (CFG.Current.TextEditor_EnableLanguage_Ukrainian)
                Vanilla_Localization.Add("Ukrainian", ReadLocalization("Localization", "Ukrainian_xml", true));

            TaskLogs.AddLog("Loaded the localization files.");
        }
    }

    public static SortedDictionary<ResourceDescriptor, XDocument> ReadLocalization(string folderName, string pakName, bool ignoreProject = false)
    {
        var dataDir = $"{Warbox.Project.GameDirectory}\\{folderName}\\{pakName}.pak";
        var projectDir = $"{Warbox.Project.ProjectDirectory}\\Source\\{folderName}\\{CFG.Current.TextEditor_CurrentLanguage}";

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

    public static string GetLanguagePakName()
    {
        switch (CFG.Current.TextEditor_CurrentLanguage)
        {
            case "English": return "English_xml";
            case "Chinese (Simplified)": return "Chineses_xml";
            case "Chinese (Traditional)": return "Chineset_xml";
            case "Czech": return "Czech_xml";
            case "French": return "French_xml";
            case "German": return "German_xml";
            case "Italian": return "Italian_xml";
            case "Japanese": return "Japanese_xml";
            case "Korean": return "Korean_xml";
            case "Polish": return "Polish_xml";
            case "Portuguese": return "Portuguese_xml";
            case "Russian": return "Russian_xml";
            case "Spanish": return "Spanish_xml";
            case "Turkish": return "Turkish_xml";
            case "Ukrainian": return "Ukrainian_xml";
        }

        return "UNKNOWN";
    }

    public static SortedDictionary<ResourceDescriptor, XDocument> GetCurrentLocalization()
    {
        if (Localization.Count < 1)
        {
            return null;
        }

        return Localization[CFG.Current.TextEditor_CurrentLanguage];
    }

    public static SortedDictionary<ResourceDescriptor, XDocument> GetCurrentVanillaLocalization()
    {
        if (Vanilla_Localization.Count < 1)
        {
            return null;
        }

        return Vanilla_Localization[CFG.Current.TextEditor_CurrentLanguage];
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

    public static void Save()
    {
        var curLocalization = GetCurrentLocalization();

        if (curLocalization == null)
            return;

        var resDesc = TextSelection.FileSelectionDescriptor;
        var document = curLocalization[resDesc];

        var writeDir = $"{Warbox.Project.ProjectDirectory}\\Source\\Localization\\{CFG.Current.TextEditor_CurrentLanguage}\\";
        var writePath = $"{Warbox.Project.ProjectDirectory}\\Source\\Localization\\{CFG.Current.TextEditor_CurrentLanguage}\\{resDesc.Name}{resDesc.Extension}";

        if (!Directory.Exists(writeDir))
            Directory.CreateDirectory(writeDir);

        document.Save(writePath);

        var fileName = Path.GetFileName(writePath);
        TaskLogs.AddLog($"Saved file at: {writePath}.");
        TaskLogs.AddLog($"{fileName} saved.");
    }

    public static void SavePTF()
    {
        var curLocalization = GetCurrentLocalization();
        var curVanillaLocalization = GetCurrentVanillaLocalization();

        if (curLocalization == null)
            return;

        if (curVanillaLocalization == null)
            return;

        var resDesc = TextSelection.FileSelectionDescriptor;
        var document = curLocalization[resDesc];
        var vanillaDocument = curVanillaLocalization[resDesc];
        var modName = Warbox.Project.ProjectID;

        // Get a document with only the differences
        (var valid, var patchDocument) = BuildPatchLocalization(vanillaDocument, document);

        var writeDir = $"{Warbox.Project.ProjectDirectory}\\PTF\\Localization\\{CFG.Current.TextEditor_CurrentLanguage}\\";
        var writePath = $"{Warbox.Project.ProjectDirectory}\\PTF\\Localization\\{CFG.Current.TextEditor_CurrentLanguage}\\{resDesc.Name}__{modName}{resDesc.Extension}";

        if (!Directory.Exists(writeDir))
            Directory.CreateDirectory(writeDir);

        if (valid)
        {
            patchDocument.Save(writePath);

            var fileName = Path.GetFileName(writePath);
            TaskLogs.AddLog($"Saved file at: {writePath}.");
            TaskLogs.AddLog($"{fileName} saved.");
        }
        else
        {
            TaskLogs.AddLog($"Failed to contruct patch localization.");
        }
    }

    public static void Package()
    {
        Save();

        var sourceDir = $"{Warbox.Project.ProjectDirectory}\\Source\\Localization\\{CFG.Current.TextEditor_CurrentLanguage}";
        var writeDir = $"{Warbox.Project.ProjectDirectory}\\Localization\\";

        if (!Directory.Exists(writeDir))
        {
            Directory.CreateDirectory(writeDir);
        }

        var modName = Warbox.Project.ProjectID;

        var curLoc = GetCurrentLocalization();
        var vanillaLoc = GetCurrentVanillaLocalization();

        var pakName = GetLanguagePakName();

        Dictionary<string, XDocument> xmlDocuments = new();

        foreach (var entry in vanillaLoc)
        {
            var vanillaEntry = entry;
            var modEntry = curLoc.Where(e => e.Key.Name == entry.Key.Name).FirstOrDefault();

            if (!modEntry.IsDefault())
            {
                // Add mod XDocument
                xmlDocuments.Add($"{entry.Key.Name}.xml", modEntry.Value);
            }
            else
            {
                // Add vanilla XDocument
                xmlDocuments.Add($"{entry.Key.Name}.xml", entry.Value);
            }
        }

        if (xmlDocuments.Count > 0)
        {
            XmlUtils.ZipXmlDocuments(xmlDocuments, $"{writeDir}\\{pakName}.pak");
            TaskLogs.AddLog($"Packaged {xmlDocuments.Count} localization files at {writeDir}\\{pakName}.pak");
        }

        ManifestHandler.CreateManisfestIfMissing();
    }

    public static void PackagePTF()
    {
        Save();

        var sourceDir = $"{Warbox.Project.ProjectDirectory}\\PTF\\Localization\\{CFG.Current.TextEditor_CurrentLanguage}";
        var writeDir = $"{Warbox.Project.ProjectDirectory}\\Localization\\";

        if (!Directory.Exists(writeDir))
        {
            Directory.CreateDirectory(writeDir);
        }

        var pakName = GetLanguagePakName();

        XmlUtils.ZipDirectory(sourceDir, $"{writeDir}\\{pakName}.pak");
        TaskLogs.AddLog($"Packaged patch localization files at {writeDir}\\{pakName}.pak");

        ManifestHandler.CreateManisfestIfMissing();
    }

    private static (bool, XDocument) BuildPatchLocalization(XDocument vanillaDoc, XDocument modDoc)
    {
        var isEdited = false;

        var newFileString = "<?xml version=\"1.0\" encoding=\"utf-8\"?><Table><Row></Row></Table>";
        XDocument patchDoc = XDocument.Parse(newFileString);

        var tables = modDoc.Elements();
        var rows = modDoc.Elements().Elements();
        var vanillaRows = vanillaDoc.Elements().Elements();
        var modRowTop = patchDoc.Elements().Elements().FirstOrDefault();

        // Rows
        foreach (var entry in rows)
        {
            var vanillaMatch = vanillaRows.Where(e => e.ToString() == entry.ToString()).FirstOrDefault();

            // Skip if the entry is the exact same as vanilla
            if (vanillaMatch != null)
                continue;

            isEdited = true;
            modRowTop.Add(entry);
        }

        return (isEdited, patchDoc);
    }

}
