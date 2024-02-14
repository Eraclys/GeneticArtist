using GeneticSharp;
using SkiaSharp;

namespace GeneticArtist;

public interface IChromosomePainter : IChromosome
{
    void Paint(SKCanvas canvas, IChromosome chromosome);
}