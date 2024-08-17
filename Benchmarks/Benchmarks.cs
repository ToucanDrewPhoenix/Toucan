using System.Collections.Generic;
using System.IO;
using BenchmarkDotNet.Attributes;
using Toucan.Compiler;
using Toucan.Runtime.CodeGen;

namespace Benchmarks
{

public class Benchmarks
{
    private readonly Dictionary < string, ToucanProgram > programs = new Dictionary < string, ToucanProgram >();

    #region Public

    public Benchmarks()
    {
        IEnumerable < string > files = Directory.EnumerateFiles(
            ".\\Benchmarks",
            "*.Toucan",
            SearchOption.AllDirectories );

        foreach ( string file in files )
        {
            string name = Path.GetFileNameWithoutExtension( file );
            ToucanCompiler compiler = new ToucanCompiler();
            programs.Add( name, compiler.Compile( new[] { File.ReadAllText( file ) } ) );
        }
    }

    [Benchmark]
    public void RunFibonacci()
    {
        programs["Fibonacci"].Run();
    }

    [Benchmark]
    public void RunPrime()
    {
        programs["Prime"].Run();
    }

    #endregion
}

public class Bar
{
    public int i;
    public float f;
    public double d;

    public int I { get; set; }

    public float F { get; set; }

    public double D { get; set; }
}

}
