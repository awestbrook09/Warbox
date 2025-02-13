using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StudioCore.Core.Data;

public class ResourceDescriptor : IComparable<ResourceDescriptor>
{
    /// <summary>
    /// The name of the file
    /// i.e. item__buff
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The postfix used with the file
    /// i.e. buff
    /// </summary>
    public string Postfix { get; set; }

    /// <summary>
    /// Basic name of the file
    /// i.e. item
    /// </summary>
    public string BaseName { get; set; }

    /// <summary>
    /// The extension of the file
    /// i.e. xml
    /// </summary>
    public string Extension { get; set; }

    /// <summary>
    /// Relative directory of the file
    /// i.e. Libs/Tables/rpg
    /// </summary>
    public string RelativeDirectory { get; set; }

    /// <summary>
    /// Denotes if the data has been modified, and to allow it to be saved to the project.
    /// </summary>
    public bool Modified { get; set; }

    /// <summary>
    /// Denotes if the data is dervied from project data
    /// </summary>
    public bool IsProjectData { get; set; }

    public ResourceDescriptor(string path)
    {
        Modified = false;
        IsProjectData = false;

        Name = Path.GetFileNameWithoutExtension(path);
        Extension = Path.GetExtension(path);

        BaseName = "";
        Postfix = "";

        if (Name.Contains("__"))
        {
            BaseName = Name.Split("__")[0];
            Postfix = Name.Split("__")[1];
        }

        var directory = $"{path}".Replace(Name, "");
        directory = directory.Replace(Extension, "");

        // If reading a project-specific file, strip the project data root from the path
        if(directory.Contains(Warbox.ProjectDataRoot))
        {
            directory = directory.Replace(Warbox.ProjectDataRoot, "");
        }
        if (directory.Contains("Source\\Data"))
        {
            directory = directory.Replace("Source\\Data", "");
        }
        if (directory.Contains("Source\\Localization"))
        {
            directory = directory.Replace("Source\\Localization", "");
        }

        RelativeDirectory = $"{directory}";
    }

    public int CompareTo(ResourceDescriptor other)
    {
        return Name.CompareTo(other.Name);
    }
}
