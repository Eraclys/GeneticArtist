namespace GeneticArtist;

public sealed class GeneticConfig
{
    public int MinPopulationSize { get; set; }
    public int MinGenerations { get; set; }
    public int MaxPopulationSize { get; set; }
    public int MaxGenerations { get; set; }
    public float MutationProbability { get; set; }
}