using GeneratorSummaryApp.Models;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace GeneratorSummaryApp;

public partial class Program
{
    private static bool HasReadWritePermissions(string? folderPath)
    {
        try
        {
            if (string.IsNullOrEmpty(folderPath)) return false;

            // Attempt to create and delete a temporary file to check permissions
            string tempFilePath = Path.Combine(folderPath, Path.GetRandomFileName());
            File.WriteAllText(tempFilePath, "Permission check");
            File.Delete(tempFilePath);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static async Task<XDocument> LoadWithRetry(string filePath, int maxRetries = 3, int delayMilliseconds = 1000)
    {
        for (int retry = 0; retry < maxRetries; retry++)
        {
            try
            {
                return XDocument.Load(filePath);
            }
            catch (IOException)
            {
                if (retry == maxRetries - 1) throw;
                await Task.Delay(delayMilliseconds);
            }
        }
        throw new IOException($"Failed to load file {filePath} after {maxRetries} attempts.");
    }

    public static void MoveFileToFolder(string inputFilePath, string outputFolder)
    {
        // Create the output directory if it doesn't exist
        Directory.CreateDirectory(outputFolder);

        // Generate a unique filename with timestamp
        string processedFileName = Path.GetFileNameWithoutExtension(inputFilePath) +
                                   $"-{DateTime.UtcNow:yyyyMMddHHmmss}.xml";

        // Construct the full target path
        string processedFilePath = Path.Combine(outputFolder, processedFileName);

        // Move the file with retry mechanism
        MoveFileWithRetry(inputFilePath, processedFilePath);
    }

    private static void MoveFileWithRetry(string sourceFilePath, string destFilePath, int maxRetries = 3, int delayMilliseconds = 1000)
    {
        for (int retry = 0; retry < maxRetries; retry++)
        {
            try
            {
                if (File.Exists(destFilePath))
                {
                    File.Delete(destFilePath);
                }
                File.Move(sourceFilePath, destFilePath);
                return;
            }
            catch (IOException)
            {
                if (retry == maxRetries - 1) throw;
                Task.Delay(delayMilliseconds).Wait();
            }
        }
        throw new IOException($"Failed to move file {sourceFilePath} after {maxRetries} attempts.");
    }

    private static ReferenceData LoadReferenceData(string filePath)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(ReferenceData));
        using (FileStream fs = new FileStream(filePath, FileMode.Open))
        {
            return (ReferenceData)serializer.Deserialize(fs)!;
        }
    }

    public static void SaveGenerationOutput(GenerationOutput output, string filePath)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(GenerationOutput));
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            serializer.Serialize(writer, output);
        }
    }
}