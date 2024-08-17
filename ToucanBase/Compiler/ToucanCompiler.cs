using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using AntlrToucanParser;
using Toucan.Ast;
using Toucan.Modules;
using Toucan.Runtime;
using Toucan.Runtime.CodeGen;
using Toucan.Symbols;
using MemoizeSharp;

namespace Toucan.Compiler
{

public class ToucanCompiler
{
    private static string m_SystemModule;

    #region Public

    /// <summary>
    ///     Compiles a set of modules
    /// </summary>
    /// <param name="modules"></param>
    /// <returns></returns>
    public ToucanProgram Compile( IEnumerable < string > modules )
    {
        ProgramBaseNode programBase = ParseModules( modules );

        SymbolTable symbolTable = new SymbolTable();

        return CompileProgram( symbolTable, programBase );
    }

    /// <summary>
    ///     Compiles a set of <see cref="Module" /> objects. You only need to use <see cref="Module" /> objects if you want to
    ///     keep a reference
    ///     to module names and imports in your own code without needing to parse a module string
    /// </summary>
    /// <param name="modules"></param>
    /// <returns></returns>
    public ToucanProgram Compile( IReadOnlyCollection < Module > modules )
    {
        List < string > moduleStrings = new List < string >();

        foreach ( Module module in modules )
        {
            StringBuilder moduleBuilder = new StringBuilder();
            moduleBuilder.AppendLine( $"module {module.Name};\r\n" );

            foreach ( string import in module.Imports )
            {
                moduleBuilder.AppendLine( $"import {import};" );
                moduleBuilder.AppendLine( $"using {import};" );
            }

            moduleBuilder.AppendLine();
            moduleBuilder.AppendLine( module.Code );

            moduleStrings.Add( moduleBuilder.ToString() );
        }

        ProgramBaseNode programBase = ParseModules( moduleStrings );

        SymbolTable symbolTable = new SymbolTable();

        return CompileProgram( symbolTable, programBase );
    }

    /// <summary>
    ///     Compiles a single expression. Used for unit tests.
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    public ToucanProgram CompileExpression( string expression )
    {
        ExpressionBaseNode expressionBaseNode = ParseExpression( expression );

        ModuleBaseNode moduleBase = new ModuleBaseNode
        {
            ModuleIdent = new ModuleIdentifier( "MainModule" ),
            Statements = new List < StatementBaseNode >
            {
                new ExpressionStatementBaseNode { ExpressionBase = expressionBaseNode }
            }
        };

        SymbolTable symbolTable = new SymbolTable();

        return CompileModule( symbolTable, moduleBase );
    }

    /// <summary>
    ///     Compiles a set of statements, with the option to reuse an existing <see cref="SymbolTable" />.
    ///     Primarily used for REPL, where the previous program context should be retained between compilations of new
    ///     statements
    /// </summary>
    /// <param name="statements"></param>
    /// <param name="symbolTable"></param>
    /// <returns></returns>
    public ToucanProgram CompileStatements( string statements, SymbolTable symbolTable = null )
    {
        IReadOnlyCollection < StatementBaseNode > statementNodes = ParseStatements( statements );

        ModuleBaseNode moduleBase = new ModuleBaseNode
        {
            ModuleIdent = new ModuleIdentifier( "MainModule" ), Statements = statementNodes
        };

        if ( symbolTable == null )
        {
            symbolTable = new SymbolTable();
        }

        return CompileModule( symbolTable, moduleBase );
    }

    #endregion

    #region Private

    private ToucanProgram CompileModule( SymbolTable symbolTable, ModuleBaseNode moduleBase )
    {
        SymbolTableBuilder symbolTableBuilder = new SymbolTableBuilder( symbolTable );

        symbolTableBuilder.BuildModuleSymbolTable( moduleBase );

        ToucanCompilationContext context = new ToucanCompilationContext( symbolTable );

        CodeGenerator generator = new CodeGenerator( context );

        generator.Compile( moduleBase );

        context.Build();

        return ToucanProgram.Create( symbolTable, context.CompiledMainChunk, context.CompiledChunks );
    }

    private ToucanProgram CompileProgram( SymbolTable symbolTable, ProgramBaseNode programBase )
    {
        SymbolTableBuilder symbolTableBuilder = new SymbolTableBuilder( symbolTable );

        symbolTableBuilder.BuildProgramSymbolTable( programBase );

        ToucanCompilationContext context = new ToucanCompilationContext( symbolTable );

        CodeGenerator generator = new CodeGenerator( context );

        generator.Compile( programBase );

        context.Build();

        return ToucanProgram.Create( symbolTable, context.CompiledMainChunk, context.CompiledChunks );
    }

    private ToucanParser CreateToucanParser( string input, out ToucanCompilerSyntaxErrorListener errorListener )
    {
        AntlrInputStream stream = new AntlrInputStream( input );
        ToucanLexer lexer = new ToucanLexer( stream );
        CommonTokenStream tokens = new CommonTokenStream( lexer );
        ToucanParser ToucanParser = new ToucanParser( tokens );
        errorListener = new ToucanCompilerSyntaxErrorListener();
        ToucanParser.AddErrorListener( errorListener );
        ToucanParser.RemoveErrorListener( ConsoleErrorListener < IToken >.Instance );

        return ToucanParser;
    }

    private string GetSystemModule()
    {
        // Memoize system module so we don't load it from the assembly resource every time we compile
        if ( m_SystemModule == null )
        {
            m_SystemModule = ModuleLoader.LoadModule( "System" );
        }

        return m_SystemModule;
    }

    private string GetToucanModule( string moduleName )
    {
        // Memoize system module so we don't load it from the assembly resource every time we compile
        return ModuleLoader.LoadModule( moduleName );
    }

        private ExpressionBaseNode ParseExpression( string expression )
    {
        ToucanParser ToucanParser = CreateToucanParser( expression, out ToucanCompilerSyntaxErrorListener errorListener );

        ToucanParser.ExpressionContext tree = ToucanParser.expression();

        if ( errorListener.Errors.Count > 0 )
        {
            throw new ToucanCompilerException(
                $"Error occured while parsing expression.\r\nError Count: {errorListener.Errors.Count}",
                errorListener.Errors );
        }

        ToucanAstGenerator gen = new ToucanAstGenerator();

        return ( ExpressionBaseNode ) gen.VisitExpression( tree );
    }

    private ModuleBaseNode ParseModule( string module )
    {
        ToucanAstGenerator gen = new ToucanAstGenerator();

        ToucanParser ToucanParser = CreateToucanParser( module, out ToucanCompilerSyntaxErrorListener errorListener );

        ToucanParser.ModuleContext tree = ToucanParser.program().module( 0 );

        if ( errorListener.Errors.Count > 0 )
        {
            module = module.Trim();
            int start = module.IndexOf( "module" ) + "module ".Length;
            string moduleName = module.Substring( start, module.IndexOf( ';' ) - start );

            throw new ToucanCompilerException(
                $"Error occured while parsing module '{moduleName}' .\r\nError Count: {errorListener.Errors.Count}",
                errorListener.Errors );
        }

        return ( ModuleBaseNode ) gen.VisitModule( tree );
    }

    private ProgramBaseNode ParseModules( IEnumerable < string > modules )
    {
        ProgramBaseNode programBase = new ProgramBaseNode();

        ModuleBaseNode systemModuleBase = ParseModule( GetSystemModule() );
        programBase.AddModule( systemModuleBase );


        ModuleBaseNode interopModuleBase = ParseModule( GetToucanModule( "Interop" ) );
        programBase.AddModule( interopModuleBase );

            foreach ( string ToucanModule in modules )
        {
            ModuleBaseNode moduleBase = ParseModule( ToucanModule );
            programBase.AddModule( moduleBase );
        }

        return programBase;
    }

    private IReadOnlyCollection < StatementBaseNode > ParseStatements( string statements )
    {
        ToucanParser ToucanParser = CreateToucanParser( statements, out ToucanCompilerSyntaxErrorListener errorListener );

        ToucanParser.StatementsContext tree = ToucanParser.statements();

        if ( errorListener.Errors.Count > 0 )
        {
            throw new ToucanCompilerException(
                $"Error occured while parsing statement.\r\nError Count: {errorListener.Errors.Count}",
                errorListener.Errors );
        }

        ToucanAstGenerator gen = new ToucanAstGenerator();

        DeclarationsBaseNode declarationsBase = ( DeclarationsBaseNode ) gen.VisitStatements( tree );

        return declarationsBase.Statements;
    }

    #endregion
}

}
