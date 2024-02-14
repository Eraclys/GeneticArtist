using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;

namespace GeneticArtist.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkSwitcher
            .FromAssemblies(new[] { typeof(Program).Assembly })
            .Run(args, ManualConfig
                .Create(DefaultConfig.Instance)
                .AddDiagnoser(MemoryDiagnoser.Default)
                //.AddDiagnoser(new DotTraceDiagnoser())
            );
    }
}