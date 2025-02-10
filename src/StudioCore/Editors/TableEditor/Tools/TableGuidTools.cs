using DotNext;
using StudioCore.Core.Data;
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

        foreach (var view in Warbox.EditorHandler.TableEditor.TableDataView.GetTableViews())
        {
            var viewName = view.Key;
            var curView = view.Value;

            var results = FindAttributesByValue(curView.ViewDocument, value);
            foreach (var res in results)
            {
                var guidResult = new GuidSearchResult(curView.ViewStatus.Name, res.Item1, res.Item2, res.Item3, res.Item4);

                GuidFinderResults.Add(guidResult);
            }
        }
    }

    public static List<(int, XElement, XElement, XAttribute)> FindAttributesByValue(XDocument doc, string value)
    {
        if (doc == null)
            throw new ArgumentNullException(nameof(doc));

        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Value cannot be null or empty.", nameof(value));

        var result = new List<(int, XElement, XElement, XAttribute)>();
        int index = 0;

        foreach (var element in doc.Root.Elements())
        {
            index = 0;

            foreach (var secondElement in element.Elements())
            {
                AttributeScan(result, element, secondElement, value, index);

                foreach (var thirdElement in secondElement.Elements())
                {
                    AttributeScan(result, secondElement, thirdElement, value, index);

                    foreach (var fourthElement in thirdElement.Elements())
                    {
                        AttributeScan(result, thirdElement, fourthElement, value, index);

                        foreach (var fifthElement in fourthElement.Elements())
                        {
                            AttributeScan(result, fourthElement, fifthElement, value, index);
                        }
                    }
                }

                index++;
            }
        }

        return result;
    }

    private static void AttributeScan(List<(int, XElement, XElement, XAttribute)> result, XElement srcElement, XElement curElement, string value, int index)
    {
        foreach (var attribute in curElement.Attributes())
        {
            if (attribute.Value == value)
            {
                result.Add((index, srcElement, curElement, attribute));
            }
        }
    }
}

public class GuidSearchResult
{
    public string File { get; set; }

    public int RowIndex { get; set; }

    public XElement Element { get; set; }

    public XElement Descendant { get; set; }

    public XAttribute Attribute { get; set; }

    public GuidSearchResult(string file, int rowIndex, XElement element, XElement descendant, XAttribute attribute)
    {
        File = file;
        RowIndex = rowIndex;
        Element = element;
        Descendant = descendant;
        Attribute = attribute;  
    }
}