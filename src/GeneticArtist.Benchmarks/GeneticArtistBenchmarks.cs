using BenchmarkDotNet.Attributes;
using GeneticArtist.Chromosomes;
using SkiaSharp;

namespace GeneticArtist.Benchmarks;

public class GeneticArtistBenchmarks
{
    SKBitmap _canvas = null!;
    Artist _geneticAlgorithm = null!;

    [GlobalSetup]
    public void Setup()
    {        
        var geneticConfig = new GeneticConfig
        {
            MinGenerations = 24,
            MinPopulationSize = 16,
            MutationProbability = 0.25f,
        };
        
        var targetImage = ImageLoader.Load(@"TestData\monaliza.jpg");
        var strokeImages = ImageLoader.LoadStrokes("Strokes");
        _canvas = new SKBitmap(targetImage.Width, targetImage.Height);
        using var canvasImage = new SKCanvas(_canvas);
        canvasImage.Clear(SKColors.Black);
        
        _geneticAlgorithm = new Artist(
            targetImage,
            new StrokeChromosome(targetImage, strokeImages),
            geneticConfig,
            _canvas,
            OnIterationCompleted);
    }

    static void OnIterationCompleted(int arg1, SKBitmap arg2, TimeSpan arg3)
    {
    }

    [Benchmark(Baseline = true)]
    public void RunGeneticAlgorithmToCompletion()
    {
        _geneticAlgorithm.RunOne();
    }
}