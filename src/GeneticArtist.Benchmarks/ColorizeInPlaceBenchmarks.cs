using BenchmarkDotNet.Attributes;
using SkiaSharp;

namespace GeneticArtist.Benchmarks;

public class ColorizeInPlaceBenchmarks
{
    SKBitmap _imageA = null!;
    SKColor _color;

    [GlobalSetup]
    public void Setup()
    {
        _imageA = ImageLoader.Load(@"TestData\stroke_01.png");
        _color = SKColors.Chocolate;
    }
    
    [Benchmark(Baseline = true)]
    public SKBitmap ColorizeInPlace() => _imageA.ColorizeInPlace(_color);
    
    [Benchmark]
    public SKBitmap ColorizeInPlaceNew() => _imageA.ColorizeInPlace(_color);
}