using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Toucan.Modules.Callables;
using Toucan.Runtime.Bytecode;
using Toucan.Runtime.Functions.ForeignInterface;
using Toucan.Symbols;

namespace Toucan.Runtime.CodeGen
{

/// <summary>
/// </summary>
public class ToucanProgram
{
    public TypeRegistry TypeRegistry { get; } = new TypeRegistry();

    public SymbolTable SymbolTable { get; private set; }

    internal BinaryChunk CompiledMainChunk { get; private set; }

    internal Dictionary < string, BinaryChunk > CompiledChunks { get; private set; }

    #region Public

    internal ToucanProgram()
    {
    }

    internal static ToucanProgram Create(
        SymbolTable symbolTable,
        BinaryChunk compiledMainChunk,
        Dictionary < string, BinaryChunk > compiledChunks )
    {
        return new ToucanProgram
        {
            SymbolTable = symbolTable, CompiledMainChunk = compiledMainChunk, CompiledChunks = compiledChunks
        };
    }

    /// <summary>
    ///     Creates a new <see cref="ToucanVm" /> and executes the current <see cref="ToucanProgram" />
    /// </summary>
    /// <returns></returns>
    public ToucanResult Run( Dictionary < string, object > externalObjects = null )
    {
        ToucanVm ToucanVm = new ToucanVm();
        ToucanVm.InitVm();
        ToucanVm.RegisterSystemModuleCallables( TypeRegistry );
        ToucanVm.RegisterExternalGlobalObjects( externalObjects );

        ToucanVmInterpretResult result = ToucanVm.Interpret( this, CancellationToken.None );

        return new ToucanResult { InterpretResult = result, ReturnValue = ToucanVm.ReturnValue };
    }

    /// <summary>
    ///     Creates a new <see cref="ToucanVm" /> and executes the current <see cref="ToucanProgram" />
    /// </summary>
    /// <returns></returns>
    public ToucanResult Run( CancellationToken cancellationToken, Dictionary < string, object > externalObjects = null )
    {
        ToucanVm ToucanVm = new ToucanVm();
        ToucanVm.InitVm();
        ToucanVm.RegisterSystemModuleCallables( TypeRegistry );
        ToucanVm.RegisterExternalGlobalObjects( externalObjects );

        ToucanVmInterpretResult result = ToucanVm.Interpret( this, cancellationToken );

        return new ToucanResult { InterpretResult = result, ReturnValue = ToucanVm.ReturnValue };
    }

    /// <summary>
    ///     Creates a new <see cref="ToucanVm" /> and executes the current <see cref="ToucanProgram" />
    /// </summary>
    /// <returns></returns>
    public async Task < ToucanResult > RunAsync(
        CancellationToken cancellationToken,
        Dictionary < string, object > externalObjects = null )
    {
        ToucanVm ToucanVm = new ToucanVm();
        ToucanVm.InitVm();
        ToucanVm.RegisterSystemModuleCallables( TypeRegistry );
        ToucanVm.RegisterExternalGlobalObjects( externalObjects );

        ToucanVmInterpretResult result = await ToucanVm.InterpretAsync( this, cancellationToken );

        return new ToucanResult { InterpretResult = result, ReturnValue = ToucanVm.ReturnValue };
    }

    #endregion
}

}
