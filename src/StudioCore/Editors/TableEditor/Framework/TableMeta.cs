using ImGuiNET;
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

public static class TableMeta
{
    public static Dictionary<string, XDocument> Meta = new();

    public static Dictionary<string, string> DocumentMappings = new();

    public static void Setup()
    {
        // Table Meta
        var metaDir = $"{AppContext.BaseDirectory}\\Assets\\Data\\Meta\\";

        string[] xmlFiles = Directory.GetFiles(metaDir, "*.xml", SearchOption.AllDirectories);
        foreach (string file in xmlFiles)
        {
            var name = Path.GetFileNameWithoutExtension(file);
            XDocument doc = XDocument.Load(file);

            Meta.Add(name, doc);
        }

        // Document Mappings
        var documentMappingPath = $"{AppContext.BaseDirectory}\\Assets\\Data\\DocumentMappings.xml";
        DocumentMappings = ReadXmlToDictionary(documentMappingPath);
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

        if (Meta.ContainsKey(fileName))
        {
            return Meta[fileName];
        }

        return null;
    }

    public static string GetDocumentName(string elementName)
    {
        var documentName = elementName;

        if (DocumentMappings.ContainsKey(documentName))
        {
            documentName = DocumentMappings[documentName];
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
    public static string GetElementNameValue(string metaField, string elementName, bool useFullName = false)
    {
        var displayedString = elementName;
        var documentName = GetDocumentName(elementName);

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

    /// <summary>
    /// Check if the attribute has any relevant metadata tags, 
    /// if so, we will display the secondary row to contain it.
    /// </summary>
    public static bool HasMetaData(XElement entry, XAttribute attribute)
    {
        var elementName = entry.Name.ToString();
        var attributeName = attribute.Name.ToString();
        var documentName = GetDocumentName(elementName);

        var metaDoc = GetMetaDocument(documentName);
        if (metaDoc != null)
        {
            List<XElement> elements = metaDoc.Descendants($"{attributeName}").ToList();

            foreach (var element in elements)
            {
                if (element.Attribute("FileEnum") != null)
                {
                    return true;
                }
                if (element.Attribute("TextRef") != null)
                {
                    return true;
                }
                if (element.Attribute("GuidRef") != null)
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

