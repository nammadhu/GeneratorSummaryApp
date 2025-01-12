using System.Xml.Serialization;

namespace GeneratorSummaryApp.Models;
#pragma warning disable CS8618
// Classes to hold the generation output data
public class GenerationOutput
{
    public List<Generator> Totals { get; set; }
    public List<Day> MaxEmissionGenerators { get; set; }
    public List<ActualHeatRate> ActualHeatRates { get; set; }
}

public class Generator
{
    public string Name { get; set; }
    public double Total { get; set; }
}

public class Day //MaxEmissionGenerator
{
    public string Name { get; set; }
    [XmlIgnore]
    public DateTime Date { get; set; }

    [XmlElement("Date")]
    public string FormattedDate
    {
        get => Date.ToString("yyyy-MM-ddTHH:mm:ss+00:00");
        set => Date = DateTime.Parse(value).ToUniversalTime();
    }
    public double Emission { get; set; }
}

public class ActualHeatRate
{
    public string Name { get; set; }
    public double HeatRate { get; set; }
}
#pragma warning restore CS8618