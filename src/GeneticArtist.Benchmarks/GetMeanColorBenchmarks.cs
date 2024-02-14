using BenchmarkDotNet.Attributes;
using SkiaSharp;

namespace GeneticArtist.Benchmarks;

public class GetMeanColorBenchmarks
{
    SKBitmap _imageA = null!;
    SKBitmap _stroke = null!;
    SKPoint _point;
    
    [GlobalSetup]
    public void Setup()
    {
        _imageA = ImageLoader.Load(@"TestData\monaliza.jpg");
        _stroke = ImageLoader.Load(@"TestData\stroke_01.png");
        _point = new SKPoint(0, 0);
    }
    
    [Benchmark(Baseline = true)]
    public SKColor GetMeanColor() => _imageA.GetMeanColor(_stroke, _point);
    
    [Benchmark]
    public SKColor GetMeanColorNew() => _imageA.GetMeanColor(_stroke, _point);
}