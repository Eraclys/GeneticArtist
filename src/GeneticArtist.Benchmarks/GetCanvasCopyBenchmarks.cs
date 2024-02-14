using BenchmarkDotNet.Attributes;
using SkiaSharp;

namespace GeneticArtist.Benchmarks;

public class GetCanvasCopyBenchmarks
{
    SKBitmap _imageA = null!;
    ThreadLocal<(SKBitmap, SKCanvas)> _copyProvider = null!;
    
    [GlobalSetup]
    public void Setup()
    {
        _imageA = ImageLoader.Load(@"TestData\monaliza.jpg");
        _copyProvider = new ThreadLocal<(SKBitmap, SKCanvas)>(() =>
        {
            var bitmap = new SKBitmap(_imageA.Width, _imageA.Height);
            var canvas = new SKCanvas(bitmap);
            return (bitmap, canvas);
        });
    }
    
    [Benchmark(Baseline = true)]
    public void CopyBaseline()
    {
        using var copy = _imageA.Copy();
    }
    
    [Benchmark]    
    public void ThreadLocalRedrawOnTop()
    {
        var (_, skCanvas) = _copyProvider.Value;
        skCanvas.DrawBitmap(_imageA, new SKPoint(0, 0));
    }
}
