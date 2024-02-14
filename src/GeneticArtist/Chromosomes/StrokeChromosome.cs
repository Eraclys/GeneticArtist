using GeneticSharp;
using SkiaSharp;

namespace GeneticArtist.Chromosomes;

public sealed class StrokeChromosome : ChromosomeBase, IChromosomePainter
{
    static readonly IRandomization Randomization = RandomizationProvider.Current;
    
    readonly SKBitmap _target;
    readonly SKBitmap[] _strokeImages;

    public StrokeChromosome(
        SKBitmap target,
        SKBitmap[] strokeImages) 
        : base(5)
    {
        _target = target;
        _strokeImages = strokeImages;

        CreateGenes();
    }

    public override Gene GenerateGene(int geneIndex) => geneIndex switch
    {
        0 => // type
            new Gene(Randomization.GetInt(0, _strokeImages.Length)),
        1 => // xPos
            new Gene(Randomization.GetInt(0, _target.Width)),
        2 => // yPos
            new Gene(Randomization.GetInt(0, _target.Height)),
        3 => // scale (represented here as a single float, might need two genes for low and high or a different representation)
            new Gene(Randomization.GetFloat(0.01f, 1.0f)),
        4 => // angle
            new Gene(Randomization.GetFloat(0f, 360f)),
        _ => throw new ArgumentOutOfRangeException(nameof(geneIndex), $"Unsupported gene index: {geneIndex}")
    };

    public override IChromosome CreateNew() => new StrokeChromosome(
        _target,
        _strokeImages);

    public void Paint(SKCanvas canvas, IChromosome chromosome)
    {
        var genes = chromosome.GetGenes();
        
        var stroke = _strokeImages[(int)genes[0].Value];
        
        var transformedStroke = stroke
            .Scale((float)genes[3].Value)
            .Rotate((float)genes[4].Value);
        
        var position = new SKPoint(
            (int)genes[1].Value - transformedStroke.Width / 2f,
            (int)genes[2].Value - transformedStroke.Height / 2f);
        
        var color = _target.GetMeanColor(transformedStroke, position);
        var colorizedStroke = transformedStroke.ColorizeInPlace(color);
        
        canvas.DrawBitmap(colorizedStroke, position);
    }
}