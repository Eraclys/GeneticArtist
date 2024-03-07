using GeneticSharp;
using SkiaSharp;

namespace GeneticArtist.Chromosomes;

public sealed class LineChromosome : ChromosomeBase, IChromosomePainter
{
    static readonly IRandomization Randomization = RandomizationProvider.Current;
    static readonly ThreadLocal<SKPaint> PaintProvider = new(() => new SKPaint
    {
        Style = SKPaintStyle.Stroke,
        IsAntialias = true,
        StrokeWidth = 3
    });
    
    static readonly ThreadLocal<SKPath> PathProvider = new(() => new SKPath());
    
    readonly SKBitmap _target;

    public LineChromosome(SKBitmap target) 
        : base(5)
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
        4 => // size
            new Gene(Randomization.GetInt(1, 5)),
            //new Gene(Randomization.GetInt(1, Math.Max(_target.Height, _target.Width))),
        _ => throw new ArgumentOutOfRangeException(nameof(geneIndex), $"Unsupported gene index: {geneIndex}")
    };

    public override IChromosome CreateNew() => new LineChromosome(_target);
    
    public void Paint(SKCanvas canvas, IChromosome chromosome)
    {
        var genes = chromosome.GetGenes();
        
        var position1 = new SKPoint(
            (int)genes[0].Value,
            (int)genes[1].Value);
        
        var position2 = new SKPoint(
            (int)genes[2].Value,
            (int)genes[3].Value);
        
        var paint = PaintProvider.Value!;
        paint.StrokeWidth = (int)genes[4].Value;
        paint.Color = SKColors.Black;

        var path = PathProvider.Value!;
        path.Reset();
        path.MoveTo(position1);
        path.LineTo(position2);
        path.Close();
        
        using var tempBitmap = new SKBitmap(_target.Width, _target.Height);
        using var tempCanvas = new SKCanvas(tempBitmap);
        tempCanvas.DrawPath(path, paint);
        paint.Color = _target.GetMeanColor(tempBitmap, SKPoint.Empty);
        
        canvas.DrawPath(path, paint);
    }
}