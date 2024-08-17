using Toucan.Runtime.Memory;

namespace Toucan.Runtime
{

public class ToucanFunctionCall
{
    public string FunctionName;
    public ToucanChunkWrapper ToucanChunkWrapper = null;
    public DynamicToucanVariable[] FunctionArguments;

    #region Public

    public ToucanFunctionCall(
        string functionName,
        DynamicToucanVariable[] functionArguments )
    {
        FunctionName = functionName;
        FunctionArguments = functionArguments;
    }

    public ToucanFunctionCall(
        ToucanChunkWrapper fWrapper,
        DynamicToucanVariable[] functionArguments )
    {
        ToucanChunkWrapper = fWrapper;
        FunctionArguments = functionArguments;
    }

    #endregion
}

}
