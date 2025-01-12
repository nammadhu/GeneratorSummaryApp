using GeneratorSummaryApp;
using GeneratorSummaryApp.Common;
using GeneratorSummaryApp.Models;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace GeneratorSummaryAppTests
{
    public class GeneratorSummaryAppTests
    {
#pragma warning disable CS8618
        private static ReferenceData referenceData;
#pragma warning restore CS8618
        private static string inputFolder = @"TestData\Input";
        private static string outputFolder = @"TestData\Output";
        private static string referenceDataFile = @"TestData\ReferenceData.xml";

        public GeneratorSummaryAppTests()
        {
            // Load reference data
            referenceData = LoadReferenceData(referenceDataFile);
            Program.inputFolder = inputFolder;
            Program.outputFolder = outputFolder;

            // Set the read-only ReferenceData property using reflection
            typeof(Program).GetProperty(nameof(Program.referenceData), BindingFlags.Public | BindingFlags.Static)?.SetValue(null, referenceData);
            // Ensure test directories exist
            Directory.CreateDirectory(inputFolder);
            Directory.CreateDirectory(outputFolder);
        }

        private static ReferenceData LoadReferenceData(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ReferenceData));
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                return (ReferenceData)serializer.Deserialize(fs)!;
            }
        }

        [Fact]
        public async Task TestProcessFile()
        {
            string testInputFile = @"TestData\Input\01-Basic.xml";
            string testOutputFile = Path.Combine(outputFolder, "01-Basic-Result.xml");

            // Ensure the test file exists in the input folder
            File.Copy(@"TestData\01-Basic.xml", testInputFile, true);

            // Process the file
            await Program.ProcessFile(testInputFile);

            // Assert the output file exists
            Assert.True(File.Exists(testOutputFile), "The output file was not created.");

            // Load the output file and assert contents
            XDocument outputDoc = XDocument.Load(testOutputFile);
            Assert.NotNull(outputDoc.Root);

            // Additional content checks can be added as needed
        }

        [Fact]
        public void TestGetValueFactor()
        {
            Assert.Equal(0.265, Program.GetValueFactor(referenceData, CONSTANTS.OffshoreWind));
            Assert.Equal(0.946, Program.GetValueFactor(referenceData, CONSTANTS.OnshoreWind));
            Assert.Equal(0.696, Program.GetValueFactor(referenceData, CONSTANTS.Gas));
            Assert.Equal(0.696, Program.GetValueFactor(referenceData, CONSTANTS.Coal));
        }

        [Fact]
        public void TestGetEmissionFactor()
        {
            Assert.Equal(0.0, Program.GetEmissionFactor(referenceData, CONSTANTS.OffshoreWind));
            Assert.Equal(0.0, Program.GetEmissionFactor(referenceData, CONSTANTS.OnshoreWind));
            Assert.Equal(0.562, Program.GetEmissionFactor(referenceData, CONSTANTS.Gas));
            Assert.Equal(0.812, Program.GetEmissionFactor(referenceData, CONSTANTS.Coal));
        }

        [Fact]
        public void TestProcessGenerationReport()
        {
            XDocument generationReportDoc = XDocument.Load(@"TestData\01-Basic.xml");
            GenerationOutput output = Program.ProcessGenerationReport(generationReportDoc);

            Assert.NotNull(output);
            Assert.Equal(4, output.Totals.Count);
            Assert.Equal(3, output.MaxEmissionGenerators.Count);
            Assert.Single(output.ActualHeatRates);
        }
    }
}