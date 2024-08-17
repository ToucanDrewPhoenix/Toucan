using System.Collections.Generic;
using Toucan.Runtime.Memory;

namespace Toucan.Runtime.Functions
{

public interface IToucanVmCallable
{
    object Call( DynamicToucanVariable[] arguments );
}

}
