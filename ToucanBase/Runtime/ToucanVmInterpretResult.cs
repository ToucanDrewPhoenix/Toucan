namespace Toucan.Runtime
{

public enum ToucanVmInterpretResult
{
    InterpretOk,
    InterpretCompileError,
    InterpretRuntimeError,
    Continue,
    Cancelled
}

}
