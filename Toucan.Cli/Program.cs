using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Toucan.Cli.CommandLine;
using Toucan.Compiler;
using Toucan.Runtime.CodeGen;

namespace Toucan.Cli
{

internal class Program
{
    #region Private

    private static void Main( string[] args )
    {
        Console.WriteLine( "Toucan Programming Langauge v0.1 (c) 2022\r\n" );
        CommandLineArgs commandLine = new CommandLineArgs( args );

        commandLine.Parse < Options >(
            o =>
            {
                if ( o.Modules != null )
                {
                    ToucanCompiler compiler = new ToucanCompiler();

                    ToucanProgram program = compiler.Compile( o.Modules.Select( File.ReadAllText ) );

                    program.Run();
                }
                else if ( o.Path != null )
                {
                    IEnumerable < string > files = Directory.EnumerateFiles(
                        o.Path,
                        "*.Toucan",
                        SearchOption.AllDirectories );

                    ToucanCompiler compiler = new ToucanCompiler();

                    ToucanProgram program = compiler.Compile( files.Select( File.ReadAllText ) );

                    program.Run();
                }
                else
                {
                    REPL.Start();
                }
            } );
    }

    #endregion
}

}
