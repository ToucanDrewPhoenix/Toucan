﻿using Toucan.Cli.CommandLine;

namespace Toucan.Cli
{

public class Options
{
    [Option( 'p', "path", ".", "<path\\to\\modules>", "The path containing the modules to be loaded" )]
    public string Path { get; set; }

    [Option( 'i', "input", false, "<module1.Toucan> [module2.Toucan] ...", "A list of modules to be loaded" )]
    public string[] Modules { get; set; }
}

}
