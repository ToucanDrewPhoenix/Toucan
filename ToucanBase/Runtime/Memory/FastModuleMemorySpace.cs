using Toucan.Runtime.Bytecode;

namespace Toucan.Runtime.Memory
{

public class FastModuleMemorySpace : FastMemorySpace
{
    #region Public

    public FastModuleMemorySpace(
        string name,
        FastMemorySpace enclosingSpace,
        int stackCount,
        BinaryChunk callerChunk,
        int callerInstructionPointer,
        int callerLineNumberPointer,
        int memberCount ) : base( name, enclosingSpace, stackCount, callerChunk, callerInstructionPointer, callerLineNumberPointer, memberCount )
    {
    }

    #endregion
}

}
