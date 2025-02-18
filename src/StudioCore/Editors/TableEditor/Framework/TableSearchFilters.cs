using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StudioCore.Editors.TableEditor.Framework;

public static class TableSearchFilters
{
    public static bool FilterFileList(string name, string input)
    {
        var preppedName = name.ToLower().Trim();
        var preppedInput = input.ToLower().Trim();

        if (input == "")
            return true;

        var isValid = false;

        if (preppedName.Contains(preppedInput))
        {
            isValid = true;
        }

        return isValid;
    }
    public static bool FilterResultEntry(string input, string filename, string elementName, string descendantName, string attributeName)
    {
        var pInput = input.ToLower().Trim();
        var pFilename = filename.ToLower().Trim();
        var pElementName = elementName.ToLower().Trim();
        var pDescendantName = descendantName.ToLower().Trim();
        var pAttributeName = attributeName.ToLower().Trim();

        if (input == "")
            return true;

        var isValid = false;

        if (pFilename.Contains(pInput))
        {
            isValid = true;
        }

        if (pElementName.Contains(pInput))
        {
            isValid = true;
        }

        if (pDescendantName.Contains(pInput))
        {
            isValid = true;
        }

        if (pAttributeName.Contains(pInput))
        {
            isValid = true;
        }

        return isValid;
    }

    public static string SearchCommandsHint = "There are three search commands you can use to perform complex filtering:\n\n" +
        "header: <header name>\n" +
        "This will filter the entry list to only entries with the specified header present.\n\n" +
        "prop: <property name>\n" +
        "This will filter the entry list to only entries with the specified property present.\n\n" +
        "propval: <property name> <operation> <value>\n" +
        "This will filter the entry list to only entries with the specified property present, where the value is equal (=), less than (<) or greater than (>) the specified value.\n\n" +
        "propstring: <property name> <comparison type> <value>\n" +
        "This will filter the entry list to only entries with the specified property present, where the value is exactly (=), or the property contains the value (~).\n\n";

    public static bool FilterTableRowEntry(XElement element, string name, string input)
    {
        var preppedName = name.ToLower().Trim();
        var preppedInput = input.ToLower().Trim();

        if (input == "")
            return true;

        var isValid = false;

        if (preppedInput.Contains("header:"))
        {
            var headerInput = preppedInput.Replace("header:", "");
            var headerName = $"{element.Name}".ToLower().Trim();

            if (headerName.Contains(headerInput))
            {
                isValid = true;
            }
        }

        if (preppedInput.Contains("prop:"))
        {
            var propInput = preppedInput.Replace("prop:", "");

            var attributes = element.Attributes().ToList();

            foreach (var attrib in attributes)
            {
                var attribName = $"{attrib.Name}".ToLower().Trim();

                if (attribName.Contains(propInput))
                {
                    isValid = true;
                }
            }
        }

        if (preppedInput.Contains("propval:"))
        {
            var propInput = preppedInput.Replace("propval:", "");
            var inputParts = propInput.Split(" ");
            if (inputParts.Length >= 3)
            {
                var propName = inputParts[0];
                var propOperation = inputParts[1];
                var propValue = inputParts[2];

                var attributes = element.Attributes().ToList();

                foreach (var attrib in attributes)
                {
                    var attribName = $"{attrib.Name}".ToLower().Trim();

                    if (attribName.Contains(propName))
                    {
                        if (propOperation == "=")
                        {
                            // Direct match
                            if (attrib.Value == propValue)
                            {
                                isValid = true;
                            }
                        }

                        if (propOperation == ">" || propOperation == "<")
                        {
                            // Only try this if the values are numeric
                            if (IsNumeric(attrib.Value) && IsNumeric(propValue))
                            {
                                double valueA = 0;
                                var resA = double.TryParse(attrib.Value, out valueA);

                                double valueB = 0;
                                var resB = double.TryParse(propValue, out valueB);

                                if (propOperation == ">")
                                {
                                    if (valueA > valueB)
                                    {
                                        isValid = true;
                                    }
                                }

                                if (propOperation == "<")
                                {
                                    if (valueA < valueB)
                                    {
                                        isValid = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        if (preppedInput.Contains("propstring:"))
        {
            var propInput = preppedInput.Replace("propstring:", "");
            var inputParts = propInput.Split(" ");
            if (inputParts.Length >= 3)
            {
                var propName = inputParts[0];
                var propOperation = inputParts[1];
                var propValue = inputParts[2];

                var attributes = element.Attributes().ToList();

                foreach (var attrib in attributes)
                {
                    var attribName = $"{attrib.Name}".ToLower().Trim();

                    if (attribName.Contains(propName))
                    {
                        if (propOperation == "=")
                        {
                            if (attrib.Value == propValue)
                            {
                                isValid = true;
                            }
                        }

                        if (propOperation == "~")
                        {
                            if (attrib.Value.Contains(propValue))
                            {
                                isValid = true;
                            }
                        }
                    }
                }
            }
        }

        if (preppedName.Contains(preppedInput))
        {
            isValid = true;
        }

        return isValid;
    }

    public static bool IsNumeric(string input)
    {
        return !string.IsNullOrEmpty(input) && input.All(char.IsDigit);
    }

    public static bool FilterTableEntry(string name, string input)
    {
        var preppedName = name.ToLower().Trim();
        var preppedInput = input.ToLower().Trim();

        if (input == "")
            return true;

        var isValid = false;

        if (preppedName.Contains(preppedInput))
        {
            isValid = true;
        }

        return isValid;
    }

}
