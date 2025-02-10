using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StudioCore.Editors.TableEditor.Framework;

public static class TableDefinition
{
    public static List<XElement> Definitions = new List<XElement>();

    public static List<string> Categories = new List<string>();

    public static void Setup()
    {
        var tableDefPath = $"{AppContext.BaseDirectory}\\Assets\\Data\\TableDefinitions.xml";

        // Table Definitions
        XDocument doc = XDocument.Load(tableDefPath);
        Definitions = doc.Descendants("tables").Elements("entry").ToList();

        // Table Categories
        var categories = doc.Descendants("categories").Elements("entry").ToList();
        foreach (var category in categories)
        {
            Categories.Add(category.Attribute("Name").Value);
        }
    }
}
