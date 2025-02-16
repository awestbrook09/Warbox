using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using static Assimp.Metadata;
using StudioCore.Editors.TableEditor.Framework;
using StudioCore.Core.Data;

namespace StudioCore.Utilities;

public static class XmlUtils
{
    public static SortedDictionary<ResourceDescriptor, XDocument> ReadXmlFromZip(string zipPath)
    {
        var xmlFiles = new SortedDictionary<ResourceDescriptor, XDocument>();

        try
        {
            using (FileStream zipStream = new FileStream(zipPath, FileMode.Open, FileAccess.Read))
            using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        using (Stream entryStream = entry.Open())
                        {
                            try
                            {
                                string xmlContent;
                                Encoding encoding = DetectEncoding(entryStream, out xmlContent);

                                using (StringReader stringReader = new StringReader(xmlContent))
                                {
                                    XDocument xmlDoc = XDocument.Load(stringReader);

                                    var resDescriptor = new ResourceDescriptor(entry.FullName);

                                    xmlFiles[resDescriptor] = xmlDoc;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error reading {entry.FullName}: {ex.Message}");
                            }
                        }
                    }
                }
            }
        }
        catch { }

        return xmlFiles;
    }

    public static SortedDictionary<ResourceDescriptor, XDocument> ReadXmlFromDirectory(string directoryPath)
    {
        var xmlFiles = new SortedDictionary<ResourceDescriptor, XDocument>();

        try
        {
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine($"Directory '{directoryPath}' does not exist.");
                return xmlFiles;
            }

            foreach (string filePath in Directory.GetFiles(directoryPath, "*.xml", SearchOption.AllDirectories))
            {
                try
                {
                    string xmlContent;
                    Encoding encoding = DetectEncoding(filePath, out xmlContent);

                    using (StringReader stringReader = new StringReader(xmlContent))
                    {
                        XDocument xmlDoc = XDocument.Load(stringReader);

                        var resDescriptor = new ResourceDescriptor(filePath);
                        xmlFiles[resDescriptor] = xmlDoc;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading {filePath}: {ex.Message}");
                }
            }
        }
        catch { }

        return xmlFiles;
    }

    private static Encoding DetectEncoding(Stream stream, out string xmlContent)
    {
        using (StreamReader reader = new StreamReader(stream, Encoding.Default, detectEncodingFromByteOrderMarks: true))
        {
            xmlContent = reader.ReadToEnd();

            var match = System.Text.RegularExpressions.Regex.Match(xmlContent, @"<\?xml\s+.*?encoding=['""](.+?)['""]", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success)
            {
                try
                {
                    return Encoding.GetEncoding(match.Groups[1].Value);
                }
                catch (ArgumentException)
                {
                    Console.WriteLine($"Warning: Unsupported encoding '{match.Groups[1].Value}', defaulting to UTF-8.");
                }
            }

            return Encoding.UTF8;
        }
    }

    private static Encoding DetectEncoding(string filePath, out string xmlContent)
    {
        using (StreamReader reader = new StreamReader(filePath, Encoding.Default, detectEncodingFromByteOrderMarks: true))
        {
            xmlContent = reader.ReadToEnd();

            // Check for encoding declaration inside XML
            var match = System.Text.RegularExpressions.Regex.Match(xmlContent, @"<\?xml\s+.*?encoding=['""](.+?)['""]", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success)
            {
                try
                {
                    return Encoding.GetEncoding(match.Groups[1].Value);
                }
                catch (ArgumentException)
                {
                    Console.WriteLine($"Warning: Unsupported encoding '{match.Groups[1].Value}' in {filePath}, defaulting to UTF-8.");
                }
            }

            return Encoding.UTF8; // Default to UTF-8 if encoding is not specified
        }
    }

    public static void ZipXmlFiles(string sourceDirectory, string zipFilePath)
    {
        if (!Directory.Exists(sourceDirectory))
            throw new DirectoryNotFoundException($"Source directory not found: {sourceDirectory}");

        string[] xmlFiles = Directory.GetFiles(sourceDirectory, "*.xml", SearchOption.AllDirectories);

        if (xmlFiles.Length == 0)
            throw new Exception("No XML files found in the directory.");

        using (FileStream zipToCreate = new FileStream(zipFilePath, FileMode.Create))
        using (ZipArchive archive = new ZipArchive(zipToCreate, ZipArchiveMode.Create))
        {
            foreach (string file in xmlFiles)
            {
                string relativePath = Path.GetRelativePath(sourceDirectory, file);

                archive.CreateEntryFromFile(file, relativePath);
            }
        }
    }
    public static void ZipXmlDocuments(Dictionary<string, XDocument> xmlDocuments, string zipFilePath)
    {
        if (xmlDocuments == null || xmlDocuments.Count == 0)
            throw new ArgumentException("No XML documents provided to zip.", nameof(xmlDocuments));

        using (FileStream zipToCreate = new FileStream(zipFilePath, FileMode.Create))
        using (ZipArchive archive = new ZipArchive(zipToCreate, ZipArchiveMode.Create))
        {
            foreach (var kvp in xmlDocuments)
            {
                string fileName = kvp.Key;
                XDocument doc = kvp.Value;

                if (string.IsNullOrWhiteSpace(fileName))
                    throw new ArgumentException("File name cannot be null or empty.");

                ZipArchiveEntry entry = archive.CreateEntry(fileName, CompressionLevel.Optimal);

                using (Stream entryStream = entry.Open())
                using (StreamWriter writer = new StreamWriter(entryStream))
                {
                    doc.Save(writer);
                }
            }
        }
    }

    public static void ZipDirectory(string directoryPath, string zipFilePath)
    {
        // Create the ZIP archive and set the CompressionLevel
        using (FileStream zipToCreate = new FileStream(zipFilePath, FileMode.Create))
        using (ZipArchive archive = new ZipArchive(zipToCreate, ZipArchiveMode.Create))
        {
            // Get all XML files (including subdirectories)
            string[] xmlFiles = Directory.GetFiles(directoryPath, "*.xml", SearchOption.AllDirectories);

            foreach (var file in xmlFiles)
            {
                // Get the relative file path (preserve directory structure)
                string relativePath = Path.GetRelativePath(directoryPath, file);

                // Add the file to the archive
                ZipArchiveEntry entry = archive.CreateEntry(relativePath);

                // Set the time format to avoid high precision timestamps
                entry.LastWriteTime = DateTime.Now;

                using (Stream entryStream = entry.Open())
                using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    fileStream.CopyTo(entryStream);
                }
            }
        }
    }
}
