using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StudioCore.Editors.TableEditor.Tools;

public static class TablePropertyValueFinder
{
    public static bool LooseMatch = false;

    public static List<PropertyValueResult> PropertyValueFinderResults = new();

    public static void FindPropertyValues(string value)
    {
        PropertyValueFinderResults = new();

        foreach (var view in Warbox.TableEditor.TableDataView.GetTableViews())
        {
            var viewName = view.Key;
            var curView = view.Value;

            var results = FindAttributesByValue(curView.TableDocument, value);
            foreach (var res in results)
            {
                var guidResult = new PropertyValueResult(curView.TableDescriptor.Name, res.Item1, res.Item2, res.Item3, res.Item4);

                PropertyValueFinderResults.Add(guidResult);
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

                        // I'm fairly sure few XML files go this deep, if needed, add more.
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
            if (LooseMatch)
            {
                if (attribute.Value.Contains(value))
                {
                    result.Add((index, srcElement, curElement, attribute));
                }
            }
            else
            {
                if (attribute.Value == value)
                {
                    result.Add((index, srcElement, curElement, attribute));
                }
            }
        }
    }

    /// <summary>
    /// Scan for attributes with the specific name and value, and return a result list
    /// </summary>
    public static List<(int, XElement, XElement, XAttribute)> FindAttributebyNameAndValue(XDocument doc, string name, string value)
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
                AttributeScan(result, element, secondElement, value, index, name);

                foreach (var thirdElement in secondElement.Elements())
                {
                    AttributeScan(result, secondElement, thirdElement, value, index, name);

                    foreach (var fourthElement in thirdElement.Elements())
                    {
                        AttributeScan(result, thirdElement, fourthElement, value, index, name);

                        // I'm fairly sure few XML files go this deep, if needed, add more.
                        foreach (var fifthElement in fourthElement.Elements())
                        {
                            AttributeScan(result, fourthElement, fifthElement, value, index, name);
                        }
                    }
                }

                index++;
            }
        }

        return result;
    }

    /// <summary>
    /// Filter the scan by the attribute name
    /// </summary>
    private static void AttributeScan(List<(int, XElement, XElement, XAttribute)> result, XElement srcElement, XElement curElement, string value, int index, string name)
    {
        foreach (var attribute in curElement.Attributes())
        {
            if (attribute.Name == name)
            {
                if(LooseMatch)
                {
                    if (attribute.Value.Contains(value))
                    {
                        result.Add((index, srcElement, curElement, attribute));
                    }
                }
                else
                {
                    if (attribute.Value == value)
                    {
                        result.Add((index, srcElement, curElement, attribute));
                    }
                }
            }
        }
    }
}
public class PropertyValueResult
{
    public string File { get; set; }

    public int RowIndex { get; set; }

    public XElement Element { get; set; }

    public XElement Descendant { get; set; }

    public XAttribute Attribute { get; set; }

    public PropertyValueResult(string file, int rowIndex, XElement element, XElement descendant, XAttribute attribute)
    {
        File = file;
        RowIndex = rowIndex;
        Element = element;
        Descendant = descendant;
        Attribute = attribute;
    }
}