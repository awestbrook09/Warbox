using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StudioCore.Editors.TextEditor.Framework;

public static class TextSearchFilters
{
    public static string SearchCommandsHint = "There are three search commands you can use to perform complex filtering:\n\n" +
        "header: <header name>\n" +
        "This will filter the entry list to only entries with the specified header present.\n\n" +
        "prop: <property name>\n" +
        "This will filter the entry list to only entries with the specified property present.\n\n" +
        "propval: <property name> <operation> <value>\n" +
        "This will filter the entry list to only entries with the specified property present, where the value is equal, less than or greather than the specified value.\n\n";

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
    public static bool FilterRowList(string name, string text1, string text2, string input)
    {
        var preppedName = name.ToLower().Trim();
        var preppedText1 = text1.ToLower().Trim();
        var preppedText2 = text2.ToLower().Trim();
        var preppedInput = input.ToLower().Trim();

        if (preppedInput == "")
            return true;

        var isValid = false;

        if (preppedName.Contains(preppedInput))
        {
            isValid = true;
        }

        if (preppedText1.Contains(preppedInput))
        {
            isValid = true;
        }

        if (preppedText2.Contains(preppedInput))
        {
            isValid = true;
        }

        return isValid;
    }
}
