using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using StudioCore.Core.Data;
using StudioCore.UserProject;

namespace StudioCore.Core.Project;

/// <summary>
/// Core class representing a loaded project.
/// </summary>
public class Project
{
    public string ProjectName { get; set; }
    public string ProjectID { get; set; }

    /// <summary>
    /// The game interroot where all the game assets are
    /// </summary>
    public string GameDirectory { get; set; }

    /// <summary>
    /// An optional override mod directory where modded files are stored
    /// </summary>
    public string ProjectDirectory { get; set; }

    /// <summary>
    /// Where warbox files local to the project are stored.
    /// </summary>
    public string ProjectWarboxDirectory { get; set; }

    /// Holds the configuration parameters from the project.json
    /// </summary>
    public ProjectConfiguration Config;

    /// <summary>
    /// Current project.json path.
    /// </summary>
    public string ProjectJsonPath;

    public Project()
    {
        ProjectName = "";
        ProjectID = "";
        GameDirectory = "";
        ProjectDirectory = "";
        ProjectWarboxDirectory = "";

        Config = new ProjectConfiguration();
    }

    /// <summary>
    /// Fill the project state with the correct values.
    /// </summary>
    public void Setup()
    {
        // Project
        ProjectName = Config.ProjectName;
        ProjectID = SanitizeModName(ProjectName);
        GameDirectory = Config.GameDirectory;
        ProjectDirectory = Config.ProjectDirectory;
        ProjectWarboxDirectory = $"{Config.ProjectDirectory}/.warbox";

        // Data
        DataHandler.SetupTableEditor();
        DataHandler.SetupTextEditor();
    }

    /// <summary>
    /// Returns false if the project is not setup correctly.
    /// </summary>
    public bool IsValid()
    {
        if(GameDirectory == "" || ProjectDirectory == "")
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Converts the project name into a form suitable for mod ID
    /// </summary>
    private string SanitizeModName(string input)
    {
        if (input == null)
            return string.Empty;

        string result = input.Replace(' ', '_');

        result = Regex.Replace(result, @"[^a-zA-Z0-9_]", "");

        return result.ToLower();
    }

    /// <summary>
    /// Write out the project.json
    /// </summary>
    public void UpdateProjectJSON()
    {
        if (ProjectDirectory != "")
        {
            string jsonString = JsonSerializer.Serialize(Config, typeof(ProjectConfiguration), ProjectConfigurationSerializationContext.Default);

            try
            {
                var fs = new FileStream($"{ProjectDirectory}/project.json", FileMode.Create);
                var data = Encoding.ASCII.GetBytes(jsonString);
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Dispose();
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"{ex}");
            }
        }
    }
}

