using GeneratorSummaryApp.Common;
using GeneratorSummaryApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GeneratorSummaryApp;

public partial class Program
{
    public static async Task ProcessFile(string inputFilePath)
    {
        if (string.IsNullOrEmpty(outputFolder) || string.IsNullOrEmpty(inputFolder)) return;

        try
        {
            // Load generation report with retry mechanism
            XDocument generationReportDoc = await LoadWithRetry(inputFilePath);

            // Process the generation report
            GenerationOutput output = ProcessGenerationReport(generationReportDoc);

            // Serialize the output to XML and save to output folder
            string outputFileName = Path.GetFileNameWithoutExtension(inputFilePath) + "-Result.xml";
            string outputFilePath = Path.Combine(outputFolder, outputFileName);
            SaveGenerationOutput(output, outputFilePath);

            MoveFileToFolder(inputFilePath, Path.Combine(inputFolder, "Processed"));
            processedCounter++;
            PrintSuccess($"{processedCounter}.Processed {outputFileName} at {DateTime.UtcNow}");
        }
        catch (Exception ex)
        {
            PrintError($"{processedCounter}.Failed file {inputFilePath}: {ex.Message}");
            MoveFileToFolder(inputFilePath, Path.Combine(inputFolder, "Error"));
        }
    }

    public static GenerationOutput ProcessGenerationReport(XDocument generationReportDoc)
    {
        if (referenceData == null) throw new Exception("Reference Data is Not Loaded Properly");
        GenerationOutput output = new GenerationOutput
        {
            Totals = new List<Generator>(),
            MaxEmissionGenerators = new List<Day>(),
            ActualHeatRates = new List<ActualHeatRate>()
        };

        var generators = generationReportDoc.Descendants(CONSTANTS.WindGenerator)
            .Concat(generationReportDoc.Descendants(CONSTANTS.GasGenerator))
            .Concat(generationReportDoc.Descendants(CONSTANTS.CoalGenerator));

        Dictionary<DateTime, Day> maxEmissionsByDate = new Dictionary<DateTime, Day>();

        foreach (var generator in generators)
        {
            string? generatorName = generator.Element(CONSTANTS.Name)?.Value;
            if (string.IsNullOrEmpty(generatorName)) continue;

            string generatorType = generator.Name.LocalName.Replace(CONSTANTS.Generator, "");
            double valueFactor = GetValueFactor(referenceData, generatorType == CONSTANTS.Wind ? generatorName : generatorType);
            double emissionFactor = GetEmissionFactor(referenceData, generatorType);
            double totalGenerationValue = 0;

            foreach (var day in generator.Descendants(CONSTANTS.Day))
            {
                // Convert to UTC
                DateTime date = DateTime.Parse(day.Element(CONSTANTS.Date)?.Value ?? "0").ToUniversalTime();
                double energy = double.Parse(day.Element(CONSTANTS.Energy)?.Value ?? "0");
                double price = double.Parse(day.Element(CONSTANTS.Price)?.Value ?? "0");

                double dailyGenerationValue = energy * price * valueFactor;
                totalGenerationValue += dailyGenerationValue;

                if (generator.Element(CONSTANTS.EmissionsRating) != null)
                {
                    double emissionsRating = double.Parse(generator.Element(CONSTANTS.EmissionsRating)?.Value ?? "0");
                    double dailyEmissions = energy * emissionsRating;

                    if (generatorType == CONSTANTS.Gas || generatorType == CONSTANTS.Coal)
                        dailyEmissions *= emissionFactor;

                    if (!maxEmissionsByDate.ContainsKey(date) || maxEmissionsByDate[date].Emission < dailyEmissions)
                    {
                        maxEmissionsByDate[date] = new Day
                        {
                            Name = generatorName,
                            Date = date,
                            Emission = dailyEmissions
                        };
                    }
                }
            }

            output.Totals.Add(new Generator
            {
                Name = generatorName,
                Total = totalGenerationValue
            });

            if (generatorType == CONSTANTS.Coal)
            {
                double totalHeatInput = double.Parse(generator.Element("TotalHeatInput")?.Value ?? "0");
                double actualNetGeneration = double.Parse(generator.Element("ActualNetGeneration")?.Value ?? "0");
                double actualHeatRate = totalHeatInput / actualNetGeneration;

                output.ActualHeatRates.Add(new ActualHeatRate
                {
                    Name = generatorName,
                    HeatRate = actualHeatRate
                });
            }
        }

        output.MaxEmissionGenerators = maxEmissionsByDate.Values.ToList();
        return output;
    }

    public static double GetValueFactor(ReferenceData referenceData, string generatorType)
    {
        return generatorType switch
        {
            CONSTANTS.OffshoreWind => referenceData.Factors.ValueFactor.Low,
            CONSTANTS.WindOffshore => referenceData.Factors.ValueFactor.Low,
            CONSTANTS.OnshoreWind => referenceData.Factors.ValueFactor.High,
            CONSTANTS.WindOnshore => referenceData.Factors.ValueFactor.High,
            CONSTANTS.Gas => referenceData.Factors.ValueFactor.Medium,
            CONSTANTS.Coal => referenceData.Factors.ValueFactor.Medium,
            _ => 1
        };
    }

    public static double GetEmissionFactor(ReferenceData referenceData, string generatorType)
    {
        return generatorType switch
        {
            CONSTANTS.Gas => referenceData.Factors.EmissionsFactor.Medium,
            CONSTANTS.Coal => referenceData.Factors.EmissionsFactor.High,
            _ => 0
        };
    }

    private static async Task ProcessExistingFiles()
    {
        if (string.IsNullOrEmpty(inputFolder)) return;
        var existingFiles = Directory.GetFiles(inputFolder, "*.xml");
        foreach (var file in existingFiles)
        {
            await ProcessFile(file);
        }
    }

}