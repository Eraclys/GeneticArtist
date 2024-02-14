using System.Diagnostics;
using GeneticSharp;
using SkiaSharp;

namespace GeneticArtist;

public sealed class Artist
{
    readonly Action<int, SKBitmap, TimeSpan>? _onIterationCompleted;
    readonly IChromosomePainter _painter;
    readonly FitnessCalculator _fitnessCalculator;
    readonly GeneticConfig _geneticConfig;
    
    GeneticAlgorithm _ga;
    bool _isStopRequested;
    double _previousBest;

    public Artist(
        SKBitmap targetImage,
        IChromosomePainter painter,
        GeneticConfig geneticConfig,
        SKBitmap canvas,
        Action<int, SKBitmap, TimeSpan>? onIterationCompleted = null)
    {
        EnsureValidParameters(targetImage, painter, geneticConfig, canvas);

        _onIterationCompleted = onIterationCompleted;
        
        _painter = painter;

        _fitnessCalculator = new FitnessCalculator(_painter, targetImage, canvas);
        
        PopulationSize = geneticConfig.MinPopulationSize;
        Generations = geneticConfig.MinGenerations;
        _geneticConfig = geneticConfig;

        _ga = CreateGeneticAlgorithm();
    }

    public int IterationCount { get; private set; }
    public int PopulationSize { get; private set; }
    public int Generations { get; private set; }
    public int SkipCount { get; private set; }

    public bool IsRunning => !_isStopRequested;

    public void Start(int? maxIterations = null)
    {
        while (!_isStopRequested && (!maxIterations.HasValue || IterationCount < maxIterations))
        {
            RunOne();
        }
        
        _isStopRequested = false;
    }

    public void RunOne()
    {
        var timestamp = Stopwatch.GetTimestamp();

        _ga.Start();
        IterationCount++;
        
        if (_ga.BestChromosome.Fitness > _previousBest)
        {
            using var skCanvas = new SKCanvas(_fitnessCalculator.Canvas);
            _painter.Paint(skCanvas, _ga.BestChromosome);
            _previousBest = _ga.BestChromosome.Fitness ?? 0;
        }
        else
        {
            SkipCount++;
            var skipPercentage = SkipCount / (double)IterationCount;
            
            if (skipPercentage > 0.05)
            {
                Generations = Math.Min(Generations + 1, _geneticConfig.MaxGenerations);
                PopulationSize = Math.Min(PopulationSize + 1, _geneticConfig.MaxPopulationSize);
                _ga = CreateGeneticAlgorithm();
            }
        }

        var elapsedTime = Stopwatch.GetElapsedTime(timestamp);

        _onIterationCompleted?.Invoke(IterationCount, _fitnessCalculator.Canvas, elapsedTime);
    }

    public void Stop()
    {
        _isStopRequested = true;
    }

    GeneticAlgorithm CreateGeneticAlgorithm() => new(
        new Population(PopulationSize, PopulationSize, _painter),
        _fitnessCalculator,
        new StochasticUniversalSamplingSelection(),
        new UniformCrossover(1),
        new UniformMutation(true))
    {
        Termination = new GenerationNumberTermination(Generations),
        MutationProbability = _geneticConfig.MutationProbability,
        TaskExecutor = new ParallelTaskExecutor
        {
            MinThreads = 10,
            MaxThreads = 20
        }
    };

    static void EnsureValidParameters(
        SKBitmap targetImage, 
        IChromosomePainter painter, 
        GeneticConfig geneticConfig, 
        SKBitmap canvas)
    {
        ArgumentNullException.ThrowIfNull(targetImage);
        ArgumentNullException.ThrowIfNull(painter);
        ArgumentNullException.ThrowIfNull(geneticConfig);
        ArgumentNullException.ThrowIfNull(canvas);
        
        if (geneticConfig.MinGenerations <= 0)
            throw new ArgumentOutOfRangeException(nameof(geneticConfig.MinGenerations));
        
        if (geneticConfig.MinPopulationSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(geneticConfig.MinPopulationSize));
        
        if (geneticConfig.MutationProbability < 0 || geneticConfig.MutationProbability > 1)
            throw new ArgumentOutOfRangeException(nameof(geneticConfig.MutationProbability));

        if (targetImage.Width != canvas.Width || targetImage.Height != canvas.Height)
            throw new Exception("Canvas must have the same dimensions as the target image.");
    }
}