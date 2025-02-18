using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StudioCore.Core.Manifest;

public static class ManifestHandler
{
    public static XDocument ManifestDocument { get; set; }

    public static void LoadManifest()
    {
        var readPath = $"{Warbox.Project.ProjectDirectory}/mod.manifest";
        if (File.Exists(readPath))
        {
            ManifestDocument = XDocument.Load(readPath);
        }
        else
        {
            TaskLogs.AddLog($"Could not find mod.manifest: {readPath}");
        }
    }

    public static void WriteManifest()
    {
        var writePath = $"{Warbox.Project.ProjectDirectory}/mod.manifest";
        ManifestDocument.Save(writePath);
    }

    public static void CreateManisfestIfMissing()
    {
        if (!HasManifest())
        {
            CreateManifest();
        }
    }

    public static bool HasManifest()
    {
        var path = $"{Warbox.Project.ProjectDirectory}\\mod.manifest";

        if (File.Exists(path))
        {
            return true;
        }

        return false;
    }

    public static void CreateManifest()
    {
        var path = $"{Warbox.Project.ProjectDirectory}\\mod.manifest";

        var xmlString = $"<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
            $"<kcd_mod>\r\n  " +
            $"<info>\r\n    " +
            $"<name>{Warbox.Project.ProjectName}</name>\r\n    " +
            $"<modid>{Warbox.Project.ProjectID}</modid>\r\n    " +
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
}
