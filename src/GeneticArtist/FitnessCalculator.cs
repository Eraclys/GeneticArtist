using GeneticSharp;
using SkiaSharp;

namespace GeneticArtist;

public sealed class FitnessCalculator : IFitness
{
    readonly IChromosomePainter _painter;
    readonly SKBitmap _target;
    readonly ThreadLocal<(SKBitmap, SKCanvas)> _copyProvider;
    
    public FitnessCalculator(
        IChromosomePainter painter,
        SKBitmap target,
        SKBitmap canvas)
    {
        _painter = painter;
        _target = target;
        Canvas = canvas;
        
        _copyProvider = new ThreadLocal<(SKBitmap, SKCanvas)>(() =>
        {
            var bitmap = new SKBitmap(target.Width, target.Height);
            var skCanvas = new SKCanvas(bitmap);
            return (bitmap, skCanvas);
        });
    }

    public SKBitmap Canvas { get; set; }
    
    public double Evaluate(IChromosome chromosome)
    {
        var (newImage, skCanvas) = _copyProvider.Value;
        
        skCanvas.DrawBitmap(Canvas, new SKPoint(0, 0));

        //using var freshCanvas = Canvas.Copy();
        _painter.Paint(skCanvas, chromosome);
        return 1 - _target.GetDifference(newImage);
    }
}