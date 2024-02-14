using GeneticSharp;
using SkiaSharp;

namespace GeneticArtist.Chromosomes;

public sealed class StrokeWithColorChromosome : ChromosomeBase, IChromosomePainter
{
    static readonly IRandomization Randomization = RandomizationProvider.Current;
    
    readonly SKBitmap _target;
    readonly SKBitmap[] _strokeImages;

    public StrokeWithColorChromosome(
        SKBitmap target,
        SKBitmap[] strokeImages) 
        : base(9)
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
        5 => // Hue
            new Gene(Randomization.GetInt(0, 360)),
        6 => // Saturation
            new Gene(Randomization.GetInt(0, 100)),
        7 => // Value
            new Gene(Randomization.GetInt(0, 100)),
        8 => // alpha
            new Gene(Randomization.GetInt(0, 255)),
        _ => throw new ArgumentOutOfRangeException(nameof(geneIndex), $"Unsupported gene index: {geneIndex}")
    };

    public override IChromosome CreateNew() => 
        new StrokeWithColorChromosome(
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

        var color = SKColor.FromHsv(
            (int)genes[5].Value,
            (int)genes[6].Value,
            (int)genes[7].Value,
            (byte)(int)genes[8].Value);
        
        var colorizedStroke = transformedStroke.ColorizeInPlace(color);
        
        canvas.DrawBitmap(colorizedStroke, position);
    }
}