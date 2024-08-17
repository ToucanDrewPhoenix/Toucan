using System.Collections.Generic;
using System.Reflection;
using Toucan.Runtime.Memory;

namespace Toucan.Runtime.Functions.ForeignInterface
{

public interface ICSharpEvent
{
    bool TryGetEventInfo( string name, out EventInfo eventInfo );
    object GetEventHolder();
    void Invoke( string name, DynamicToucanVariable[] m_FunctionArguments );
    bool TryAddEventHandler( string name, ToucanChunkWrapper eventHandlerFunction, ToucanVm ToucanVm );
}

}
