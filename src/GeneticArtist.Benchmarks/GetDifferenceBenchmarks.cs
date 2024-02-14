using BenchmarkDotNet.Attributes;
using SkiaSharp;

namespace GeneticArtist.Benchmarks;

public class GetDifferenceBenchmarks
{
    SKBitmap _imageA = null!;
    SKBitmap _imageB = null!;

    [GlobalSetup]
    public void Setup()
    {
        _imageA = ImageLoader.Load(@"TestData\monaliza.jpg");
        _imageB = ImageLoader.Load(@"TestData\output.png");
    }
    
    [Benchmark(Baseline = true)]
    public double GetDifference() => _imageA.GetDifference(_imageB);
    
    [Benchmark]
    public double GetDifferenceNew() => _imageA.GetDifference(_imageB);
}