using ImGuiNET;
using Octokit;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StudioCore.Editors.TableEditor.Framework;

public static class TableMetaHandler
{
    public static List<XElement> TableMetaDefinition = new List<XElement>();

    public static List<string> TableCategories = new List<string>();

    public static Dictionary<string, XDocument> TableMetaData = new();

    public static Dictionary<string, string> TableSubTypeMappings = new();

    public static void Setup()
    {
        TableMetaDefinition = new();
        TableCategories = new();
        TableMetaData = new();
        TableSubTypeMappings = new();

        var dataDir = $"{AppContext.BaseDirectory}\\Assets\\Data\\Tables\\";

        // Table Definitions
        XDocument tableDefDoc = XDocument.Load($"{dataDir}\\Definitions.xml");
        TableMetaDefinition = tableDefDoc.Descendants("tables").Elements("entry").ToList();

        // Table Categories
        var categories = tableDefDoc.Descendants("categories").Elements("entry").ToList();
        foreach (var category in categories)
        {
            TableCategories.Add(category.Attribute("Name").Value);
        }

        // Table Meta
        string[] xmlFiles = Directory.GetFiles($"{dataDir}\\Meta\\", "*.xml", SearchOption.AllDirectories);
        foreach (string file in xmlFiles)
        {
            var name = Path.GetFileNameWithoutExtension(file);
            XDocument doc = XDocument.Load(file);

            TableMetaData.Add(name, doc);
        }

        // Document Mappings
        TableSubTypeMappings = ReadXmlToDictionary($"{dataDir}\\SubTypeMappings.xml");
    }

    private static Dictionary<string, string> ReadXmlToDictionary(string filePath)
    {
        Dictionary<string, string> result = new Dictionary<string, string>();

        XDocument xmlDoc = XDocument.Load(filePath);

        foreach (XElement element in xmlDoc.Root.Elements())
        {
            string key = element.Name.LocalName;
            string value = element.Attribute("MapTo")?.Value;

            if (value != null)
            {
                result[key] = value;
            }
        }

        return result;
    }

    public static XDocument GetMetaDocument(string name, bool useFullName = false)
    {
        var fileName = name;

        if (!useFullName)
        {
            if (fileName.Contains("__"))
            {
                fileName = fileName.Split("__")[0];
            }
        }

        if (TableMetaData.ContainsKey(fileName))
        {
            return TableMetaData[fileName];
        }

        return null;
    }
    public static List<XElement> GetEnumOptions(XDocument doc, string enumName)
    {
        return doc.Descendants("enums")
                  .Descendants("enum")
                  .Where(e => (string)e.Attribute("Name") == enumName)
                  .Descendants("Option")
                  .ToList();
    }

    public static string GetDocumentName(string elementName)
    {
        var documentName = elementName;

        if (TableSubTypeMappings.ContainsKey(documentName))
        {
            documentName = TableSubTypeMappings[documentName];
        }

        return documentName;
    }

    public static string GetFileTitle(string metaField, string elementName)
    {
        var displayedString = elementName;
        var documentName = GetDocumentName(elementName);

        var metaDoc = GetMetaDocument(documentName);
        if (metaDoc != null)
        {
            List<XElement> elements = metaDoc.Descendants(elementName).ToList();

            foreach (var entry in elements)
            {
                if (entry.Attribute(metaField) != null)
                {
                    return entry.Attribute(metaField).Value;
                }
            }
        }

        return displayedString;
    }

    public static bool CheckMetaToggle(string metaField, string elementName)
    {
        var isValid = false;
        var documentName = GetDocumentName(elementName);

        var metaDoc = GetMetaDocument(documentName);
        if (metaDoc != null)
        {
            var metaElement = metaDoc.Descendants(metaField);
            if (metaElement != null)
            {
                isValid = true;
            }
        }

        return isValid;
    }

    /// <summary>
    /// Returns the header pretty name and description
    /// </summary>
    public static string GetElementNameValue(string metaField, string parentElementName, string elementName, bool useFullName = false)
    {
        var displayedString = elementName;
        var documentName = GetDocumentName(parentElementName);

        var metaDoc = GetMetaDocument(documentName, useFullName);
        if (metaDoc != null)
        {
            List<XElement> elements = metaDoc.Descendants($"{elementName}").ToList();

            foreach (var entry in elements)
            {
                if (entry.Attribute(metaField) != null)
                {
                    return entry.Attribute(metaField).Value;
                }
            }
        }

        return displayedString;
    }

    /// <summary>
    /// Returns the attribute pretty name and description
    /// </summary>
    public static string GetAttributeNameValue(string metaField, string elementName, string attributeName)
    {
        var displayedString = attributeName;
        var documentName = GetDocumentName(elementName);

        var metaDoc = GetMetaDocument(documentName);
        if (metaDoc != null)
        {
            List<XElement> elements = metaDoc.Descendants($"{attributeName}").ToList();

            foreach (var entry in elements)
            {
                if (entry.Attribute(metaField) != null)
                {
                    return entry.Attribute(metaField).Value;
                }
            }
        }

        return displayedString;
    }

    /// <summary>
    /// Returns true if the attribute is marked as bool
    /// </summary>
    public static bool IsBoolAttribute(string metaField, string elementName, string attributeName)
    {
        var documentName = GetDocumentName(elementName);

        var metaDoc = GetMetaDocument(documentName);
        if (metaDoc != null)
        {
            List<XElement> elements = metaDoc.Descendants($"{attributeName}").ToList();

            foreach (var entry in elements)
            {
                if (entry.Attribute(metaField) != null)
                {
                    return true;
                }
            }
        }

        return false;
    }


    public static List<XElement> GetAttributeList(XElement entry)
    {
        var elementName = entry.Name.ToString();
        var documentName = GetDocumentName(elementName);

        var metaDoc = GetMetaDocument(documentName);
        if (metaDoc != null)
        {
            return metaDoc.Descendants($"entries").Descendants().ToList();

        }

        return new List<XElement>();
    }
}

