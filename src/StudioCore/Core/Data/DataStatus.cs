using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StudioCore.Core.Data;

public class DataStatus : IComparable<DataStatus>
{
    /// <summary>
    /// Name of the file
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Relative path of the file within the PAK
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// Denotes if the data has been modified, and to allow it to be saved to the project.
    /// </summary>
    public bool Modified { get; set; }

    /// <summary>
    /// Denotes if the data is dervied from project data
    /// </summary>
    public bool IsProjectData { get; set; }

    public DataStatus(string name, string path)
    {
        Name = name;
        Path = path;
        Modified = false;
        IsProjectData = false;
    }

    public DataStatus(DataStatus existing)
    {
        Name = existing.Name;
        Path = existing.Path;
        Modified = existing.Modified;
        IsProjectData = existing.IsProjectData;
    }

    public int CompareTo(DataStatus other)
    {
        return Name.CompareTo(other.Name);
    }
}
