using GeneticSharp;
using SkiaSharp;

namespace GeneticArtist.Chromosomes;

public sealed class PolygonAutoColorChromosome : ChromosomeBase, IChromosomePainter
{
    static readonly IRandomization Randomization = RandomizationProvider.Current;
    static readonly ThreadLocal<SKPaint> PaintProvider = new(() => new SKPaint
    {
        Style = SKPaintStyle.StrokeAndFill,
        IsAntialias = true,
        StrokeWidth = 1
    });
    
    static readonly ThreadLocal<SKPath> PathProvider = new(() => new SKPath());
    
    readonly SKBitmap _target;

    public PolygonAutoColorChromosome(SKBitmap target) 
        : base(7)
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
        6 => // alpha
            new Gene(Randomization.GetInt(0, 255)),
        _ => throw new ArgumentOutOfRangeException(nameof(geneIndex), $"Unsupported gene index: {geneIndex}")
    };

    public override IChromosome CreateNew() => new PolygonAutoColorChromosome(_target);
    
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
        
        var paint = PaintProvider.Value!;
        paint.Color = SKColors.Black;

        var path = PathProvider.Value!;
        path.Reset();
        path.MoveTo(position1);
        path.LineTo(position2);
        path.LineTo(position3);
        path.Close();
        
        using var tempBitmap = new SKBitmap(_target.Width, _target.Height);
        using var tempCanvas = new SKCanvas(tempBitmap);
        tempCanvas.DrawPath(path, paint);
        paint.Color = _target.GetMeanColor(tempBitmap, SKPoint.Empty);
        
        canvas.DrawPath(path, paint);
    }
}