using Toucan.Runtime.Memory;

namespace Toucan.Runtime.CodeGen
{

public class ToucanResult
{
    public DynamicToucanVariable ReturnValue { get; set; }

    public ToucanVmInterpretResult InterpretResult { get; set; }
}

}
