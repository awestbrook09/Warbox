using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StudioCore.Editors.TextEditor.Framework;

public static class TextSearchFilters
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
