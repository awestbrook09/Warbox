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

namespace StudioCore.Editors.TableEditor;

public static class TableMeta
{
    public static Dictionary<string, XDocument> Meta = new();

    public static void Setup()
    {
        // Get table meta
        var metaDir = $"{AppContext.BaseDirectory}\\Assets\\Data\\Meta\\";

        string[] xmlFiles = Directory.GetFiles(metaDir, "*.xml", SearchOption.AllDirectories);
        foreach (string file in xmlFiles)
        {
            var name = Path.GetFileNameWithoutExtension(file);
            XDocument doc = XDocument.Load(file);

            Meta.Add(name, doc);
        }
    }

    public static XDocument GetMetaDocument(TableEditorState editorState, string name, bool useFullName = false)
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

    /// <summary>
    /// Returns the header pretty name and description
    /// </summary>
    public static string GetElementNameValue(TableEditorState editorState, string metaField, string elementName, bool useFullName = false)
    {
        var displayedString = elementName;

        var metaDoc = GetMetaDocument(editorState, elementName, useFullName);
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
    public static string GetAttributeNameValue(TableEditorState editorState, string metaField, string elementName, string attributeName)
    {
        var displayedString = attributeName;

        var metaDoc = GetMetaDocument(editorState, elementName);
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
    public static bool IsBoolAttribute(TableEditorState editorState, string metaField, string elementName, string attributeName)
    {
        var metaDoc = GetMetaDocument(editorState, elementName);
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
    public static bool HasMetaData(TableEditorState editorState, XDocument document, XElement entry, XAttribute attribute, int index, string imguiElementName)
    {
        var elementName = entry.Name.ToString();
        var attributeName = attribute.Name.ToString();

        var metaDoc = GetMetaDocument(editorState, elementName);
        if (metaDoc != null)
        {
            List<XElement> elements = metaDoc.Descendants($"{attributeName}").ToList();

            foreach(var element in elements)
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

    public static List<XElement> GetAttributeList(TableEditorState editorState, XDocument document, XElement entry)
    {
        var elementName = entry.Name.ToString();

        var metaDoc = GetMetaDocument(editorState, elementName);
        if (metaDoc != null)
        {
            return metaDoc.Descendants($"entries").Descendants().ToList();

        }

        return new List<XElement>();
    }
}

