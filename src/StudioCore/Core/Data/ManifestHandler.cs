using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Core.Data;

public static class ManifestHandler
{
    public static void CreateManisfestIfMissing()
    {
        if(!HasManifest())
        {
            CreateManifest();
        }
    }

    public static bool HasManifest()
    {
        var path = $"{Warbox.ProjectDataRoot}\\mod.manifest";

        if(File.Exists(path))
        {
            return true;
        }

        return false;
    }

    public static void CreateManifest()
    {
        var path = $"{Warbox.ProjectDataRoot}\\mod.manifest";
        var modName = SanitizeModName(Warbox.ProjectHandler.CurrentProject.Config.ProjectName);

        var xmlString = $"<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
            $"<kcd_mod>\r\n  " +
            $"<info>\r\n    " +
            $"<name>{Warbox.ProjectHandler.CurrentProject.Config.ProjectName}</name>\r\n    " +
            $"<modid>{modName}</modid>\r\n    " +
            $"<description></description>\r\n    " +
            $"<author></author>\r\n    " +
            $"<version>1.0</version>\r\n    " +
            $"<created_on></created_on>\r\n    " +
            $"<modifies_level>false</modifies_level>\r\n    " +
            $"<dependencies>\r\n    " +
            $"</dependencies>\r\n  " +
            $"</info>\r\n" +
            $"</kcd_mod>";

        File.WriteAllText(path, xmlString);
    }

    public static string SanitizeModName(string input)
    {
        if (input == null)
            return string.Empty;

        string result = input.Replace(' ', '_');

        result = Regex.Replace(result, @"[^a-zA-Z0-9_]", "");

        return result.ToLower();
    }
}
