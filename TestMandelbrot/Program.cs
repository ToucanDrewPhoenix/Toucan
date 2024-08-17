using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Toucan.Compiler;
using Toucan.Modules.Callables;
using Toucan.Runtime;
using Toucan.Runtime.Bytecode;
using Toucan.Runtime.CodeGen;
using Toucan.Runtime.Functions;
using Toucan.Runtime.Memory;

namespace TestMandelbrot
{

class ChangColorIntensity {
    public static Color GetWhiteColorByIntensity( float correctionFactor)
    {
        float red = 255 * correctionFactor;
        float green = 255 * correctionFactor;
        float blue = 255 * correctionFactor;
        
        return Color.FromArgb((int)255, (int)red, (int)green, (int)blue);
    }
}
public class WhiteColorByIntensityVm : IToucanVmCallable
{

    public object Call( DynamicToucanVariable[] arguments )
    {
        if ( arguments.Length == 1 )
        {
            return ChangColorIntensity.GetWhiteColorByIntensity(
                (float) arguments[0].NumberData );
        }

        return null;
    }
}
internal class Program
{
    public static void Main( string[] args )
    {
        IEnumerable < string > files = Directory.EnumerateFiles(
            ".\\",
            "Mandelbrot.Toucan",
            SearchOption.AllDirectories );

        ToucanCompiler compiler = new ToucanCompiler();

        ToucanVm ToucanVmReciever = new ToucanVm();

        ToucanVmReciever.InitVm();
        
        ToucanProgram programReciever = null;

        foreach ( string file in files )
        {
            Console.WriteLine( $"File: {file}" );
            List < string > ToucanProg = new List < string >();
            ToucanProg.Add( File.ReadAllText( file ) );
            programReciever = compiler.Compile( ToucanProg );

            programReciever.TypeRegistry.RegisterType < Bitmap >();
            programReciever.TypeRegistry.RegisterType < ImageFormat >();
            programReciever.TypeRegistry.RegisterType < Color >();
            programReciever.TypeRegistry.RegisterType (typeof(Math), "Math");
            ToucanVmReciever.RegisterCallable( "GetWhiteColorByIntensity", new WhiteColorByIntensityVm() );
            ToucanVmReciever.RegisterSystemModuleCallables( programReciever.TypeRegistry );
            ToucanVmReciever.SynchronizationContext = new SynchronizationContext();
            ToucanVmReciever.TypeRegistry = programReciever.TypeRegistry;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            ToucanVmReciever.Interpret( programReciever );
            stopwatch.Stop();

            Console.WriteLine(
                $"--- Elapsed Time Interpreting in Milliseconds: {stopwatch.ElapsedMilliseconds}ms --- " );
                         
            IOrderedEnumerable < KeyValuePair < string, long > > sortedDict =
                from entry in ChunkDebugHelper.InstructionCounter orderby entry.Value descending select entry;

            long totalInstructions = 0;

            foreach ( KeyValuePair < string, long > keyValuePair in sortedDict )
            {
                totalInstructions += keyValuePair.Value;
            }

            foreach ( KeyValuePair < string, long > keyValuePair in sortedDict )
            {
                Console.WriteLine(
                    "--Instruction Count for Instruction {0}: {2}     {1}%",
                    keyValuePair.Key,
                    ( 100.0 / totalInstructions * keyValuePair.Value ).ToString( "00.0" ),
                    keyValuePair.Value );
            }
        }
        
       

        Console.ReadLine();
    }
}

}
