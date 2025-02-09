using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StudioCore.Editors.TableEditor.Tools;

public static class TableGuidTools
{
    /// <summary>
    /// Use this to generate a new GUID (pass in list of Guids to exclude).
    /// </summary>
    public static Guid GenerateGuid(HashSet<Guid> excludedGuids)
    {
        return GenerateUniqueGuidV4(excludedGuids);
    }

    public static Guid GenerateUniqueGuidV4(HashSet<Guid> existingGuids)
    {
        Guid newGuid;

        do
        {
            newGuid = GenerateGuidV4();
        } while (existingGuids.Contains(newGuid));

        return newGuid;
    }

    public static Guid GenerateGuidV4()
    {
        byte[] guidBytes = Guid.NewGuid().ToByteArray();

        guidBytes[7] = (byte)((guidBytes[7] & 0x0F) | 0x40);
        guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80);

        return new Guid(guidBytes);
    }

    public static List<GuidSearchResult> GuidFinderResults = new();

    public static void FindGuids(string value)
    {
        GuidFinderResults = new();

        foreach (var entry in Warbox.DataHandler.Tables)
        {
            var status = entry.Key;
            var document = entry.Value;

            var results = FindAttributesByValue(document, value);
            foreach (var res in results)
            {
                var attribute = res.Attribute;
                var index = res.ElementIndex;
                var guidResult = new GuidSearchResult(status.Name, index, attribute.Name.ToString());

                GuidFinderResults.Add(guidResult);
            }
        }
    }

    public static List<(XAttribute Attribute, int ElementIndex)> FindAttributesByValue(XDocument doc, string value)
    {
        if (doc == null) 
            throw new ArgumentNullException(nameof(doc));

        if (string.IsNullOrEmpty(value)) 
            throw new ArgumentException("Value cannot be null or empty.", nameof(value));

        return doc.Descendants()
                  .Select((element, index) => new { Element = element, Index = index })
                  .SelectMany(e => e.Element.Attributes()
                        .Where(attr => attr.Value == value)
                        .Select(attr => (Attribute: attr, ElementIndex: e.Index))).ToList();
    }
}

public class GuidSearchResult
{
    public string File { get; set; }
    public int RowIndex { get; set; }
    public string PropertyName { get; set; }

    public GuidSearchResult(string file, int rowIndex, string propertyName)
    {
        File = file;
        RowIndex = rowIndex;
        PropertyName = propertyName;    
    }
}