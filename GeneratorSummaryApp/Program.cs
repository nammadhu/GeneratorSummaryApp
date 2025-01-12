using GeneratorSummaryApp.Models;
using System.Configuration;

namespace GeneratorSummaryApp
{
    //works on windows,linux and console and docker
    public partial class Program
    {
        public static ReferenceData? referenceData { get; private set; }
        public static string? inputFolder;
        public static string? outputFolder;
        private static string basePath = AppDomain.CurrentDomain.BaseDirectory;

        private static int processedCounter = 0;

        private static async Task Main(string[] args)
        {
            // Load configuration
            LoadConfiguration();

            // Process existing files
            await ProcessExistingFiles();

            // Start monitoring the input folder
            StartMonitoringFolder();

            PrintInfo("Press 'q'+Enter to quit the application.");
            while (Console.Read() != 'q') ;
        }

        private static void LoadConfiguration()
        {
            inputFolder = ConfigurationManager.AppSettings["InputFolder"]!;
            outputFolder = ConfigurationManager.AppSettings["OutputFolder"]!;
            string referenceDataFile = ConfigurationManager.AppSettings["ReferenceDataFile"] ??
                   Path.Combine(basePath, "Data", "ReferenceData.xml");
            // Load reference data
            referenceData = LoadReferenceData(referenceDataFile);

            if (string.IsNullOrEmpty(referenceDataFile) || !File.Exists(referenceDataFile))
            {
                PrintTerminate("Reference Data File Path Not Mentioned or not found");
                return;
            }
            // Validate folder permissions
            if (!ValidateFolders())
            {
                PrintTerminate("Error at Input or output folder does not have required read/write permissions.");
                return;
            }

            Console.WriteLine();
            PrintTitle("Welcome to Generator Report Processor");
            PrintInfo("Running with Configurations as below,");
            PrintColor($"Monitoring Location: {inputFolder}", ConsoleColor.White, ConsoleColor.DarkGreen);
            PrintInfo($"Output Location: {outputFolder}");
            PrintInfo($"Reference Data file: {referenceDataFile}");
            Console.WriteLine();

            PrintColor("Please Note:", ConsoleColor.DarkRed);
            PrintInfo("1.For any of Configuration Changes to reflect,app restart is required.");
            PrintInfo("2.In case of Same file name repeats then it overwrites on output.");
            PrintInfo("3.Untill then Its keep Monitoring the files and Processing to Output location");
            PrintInfo("4.Input files will be moved under InputFolder/Processed or InputFolder/Error");
            PrintColor("5.At anytime to terminate, Press 'q'+'Enter' or close the Terminal", ConsoleColor.Black, ConsoleColor.White);
            Console.WriteLine();
        }

        private static bool ValidateFolders()
        {
            return !string.IsNullOrEmpty(inputFolder) &&
                   HasReadWritePermissions(inputFolder) &&
                   HasReadWritePermissions(outputFolder);
        }

        private static void StartMonitoringFolder()
        {
            if (string.IsNullOrEmpty(inputFolder)) return;
            FileSystemWatcher watcher = new FileSystemWatcher(inputFolder, "*.xml");
            watcher.Created += async (sender, e) => await ProcessFile(e.FullPath);
            watcher.EnableRaisingEvents = true;

            PrintColor("Waiting for new files to process...", ConsoleColor.Yellow);
        }
    }
}