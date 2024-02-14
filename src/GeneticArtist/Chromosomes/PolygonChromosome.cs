using GeneticSharp;
using SkiaSharp;

namespace GeneticArtist.Chromosomes;

public sealed class PolygonChromosome : ChromosomeBase, IChromosomePainter
{
    static readonly IRandomization Randomization = RandomizationProvider.Current;
    static readonly ThreadLocal<SKPaint> PaintProvider = new(() => new SKPaint
    {
        Style = SKPaintStyle.Fill,
        IsAntialias = true,
        StrokeWidth = 1
    });
    
    static readonly ThreadLocal<SKPath> PathProvider = new(() => new SKPath());
    
    readonly SKBitmap _target;

    public PolygonChromosome(SKBitmap target) 
        : base(10)
    {
        _target = target;

        CreateGenes();
    }

    public override Gene GenerateGene(int geneIndex) => geneIndex switch
    {
        0 => // xPos 1
            new Gene(Randomization.GetInt(0, _target.Width)),
        1 => // yPos 1
            new Gene(Randomization.GetInt(0, _target.Height)),
        2 => // xPos 2
            new Gene(Randomization.GetInt(0, _target.Width)),
        3 => // yPos 2
            new Gene(Randomization.GetInt(0, _target.Height)),
        4 => // xPos 3
            new Gene(Randomization.GetInt(0, _target.Width)),
        5 => // yPos 3
            new Gene(Randomization.GetInt(0, _target.Height)),
        6 => // Hue
            new Gene(Randomization.GetInt(0, 360)),
        7 => // Saturation
            new Gene(Randomization.GetInt(0, 100)),
        8 => // Value
            new Gene(Randomization.GetInt(0, 100)),
        9 => // alpha
            new Gene(Randomization.GetInt(0, 255)),
        _ => throw new ArgumentOutOfRangeException(nameof(geneIndex), $"Unsupported gene index: {geneIndex}")
    };

    public override IChromosome CreateNew() => new PolygonChromosome(_target);
    
    public void Paint(SKCanvas canvas, IChromosome chromosome)
    {
        var genes = chromosome.GetGenes();
        
        var position1 = new SKPoint(
            (int)genes[0].Value,
            (int)genes[1].Value);
        
        var position2 = new SKPoint(
            (int)genes[2].Value,
            (int)genes[3].Value);
        
        var position3 = new SKPoint(
            (int)genes[4].Value,
            (int)genes[5].Value);
        
        var color = SKColor.FromHsv(
            (int)genes[6].Value,
            (int)genes[7].Value,
            (int)genes[8].Value,
            (byte)(int)genes[9].Value);
        
        var paint = PaintProvider.Value!;
        paint.Color = color;

        var path = PathProvider.Value!;
        path.Reset();
        path.MoveTo(position1);
        path.LineTo(position2);
        path.LineTo(position3);
        path.Close();

        canvas.DrawPath(path, paint);
    }
}