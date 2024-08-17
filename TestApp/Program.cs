using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Toucan.Compiler;
using Toucan.Modules.Callables;
using Toucan.Runtime;
using Toucan.Runtime.CodeGen;
using Toucan.Runtime.Functions;
using Toucan.Runtime.Functions.ForeignInterface;
using Toucan.Runtime.Memory;

namespace TestApp
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
public class SampleEventArgs
{
    public string Text { get; set; } // readonly

    #region Public

    public SampleEventArgs( string text )
    {
        Text = text;
    }

    #endregion
}

public class DelegateTest
{
    public delegate object TestDelegate( object sender, SampleEventArgs sampleEventArgs );

    public event TestDelegate OnSampleEvent;

    #region Public

    public void InvokeEvent( object sender, SampleEventArgs sampleEventArgs )
    {
        OnSampleEvent?.Invoke( sender, sampleEventArgs );
    }

    #endregion
}

public class Program
{
    #region Public

    public static void Main( string[] args )
    {
        IEnumerable < string > files = Directory.EnumerateFiles(
            ".\\TestProgram",
            "FibonacciExample.Toucan",
            SearchOption.AllDirectories );

        ToucanCompiler compiler = new ToucanCompiler();

        ToucanVm ToucanVm = new ToucanVm();

        ToucanVm.InitVm();

        DelegateTest delegateTest = new DelegateTest();

        ICSharpEvent cSharpEvent =
            new CSharpEvent < DelegateTest.TestDelegate, object, SampleEventArgs >( delegateTest );

        //delegateTest.OnSampleEvent += Test;
        foreach ( string file in files )
        {
            Console.WriteLine( $"File: {file}" );
            List < string > ToucanProg = new List < string >();
            ToucanProg.Add( File.ReadAllText( file ) );
            ToucanProgram ToucanProgram = compiler.Compile( ToucanProg );

            ToucanProgram.TypeRegistry.RegisterType < SampleEventArgs >();
            ToucanProgram.TypeRegistry.RegisterType < TestClassCSharp >();
            ToucanProgram.TypeRegistry.RegisterType < Bitmap >();
            ToucanProgram.TypeRegistry.RegisterType < ImageFormat >();
            ToucanProgram.TypeRegistry.RegisterType < Color >();
            // ToucanProgram.TypeRegistry.RegisterType < FSharpTest.Line >();
            ToucanProgram.TypeRegistry.RegisterType( typeof( Console ), "Console" );
            ToucanProgram.TypeRegistry.RegisterType (typeof(Math), "Math");
            ToucanVm.RegisterSystemModuleCallables( ToucanProgram.TypeRegistry );
            ToucanVm.RegisterCallable( "GetWhiteColorByIntensity", new WhiteColorByIntensityVm() );
            ToucanVm.SynchronizationContext = new SynchronizationContext();
            ToucanVm.RegisterExternalGlobalObject( "EventObject", cSharpEvent );

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            ToucanVm.Interpret( ToucanProgram );
            stopwatch.Stop();

            Console.WriteLine( $"--- Elapsed Time Interpreting in Milliseconds: {stopwatch.ElapsedMilliseconds}ms --- " );
        }

        Console.ReadLine();
    }

    public static object Test( object sender, SampleEventArgs sampleEventArgs )
    {
        Console.WriteLine( sampleEventArgs.Text );

        return sender;
    }

    #endregion
}

}
