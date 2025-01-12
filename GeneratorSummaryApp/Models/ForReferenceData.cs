using System.Xml.Serialization;

namespace GeneratorSummaryApp.Models;

#pragma warning disable CS8618

// Classes to hold the reference data
[XmlRoot("ReferenceData")]
public class ReferenceData
{
    public Factors Factors { get; set; }
}

public class Factors
{
    public ValueFactor ValueFactor { get; set; }
    public EmissionsFactor EmissionsFactor { get; set; }
}

public class ValueFactor
{
    public double High { get; set; }
    public double Medium { get; set; }
    public double Low { get; set; }
}

public class EmissionsFactor
{
    public double High { get; set; }
    public double Medium { get; set; }
    public double Low { get; set; }
}

#pragma warning restore CS8618