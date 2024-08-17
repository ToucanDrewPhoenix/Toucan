using System.Collections.Generic;
using System.Reflection;
using Toucan.Runtime.Memory;

namespace Toucan.Runtime.Functions.ForeignInterface
{

public class CSharpEvent < V, T, K > : ICSharpEvent
{
    private readonly EventWrapper < V, T, K > m_EventWrapper;

    #region Public

    public CSharpEvent( object obj )
    {
        m_EventWrapper = new EventWrapper < V, T, K >( obj );
    }

    public bool TryGetEventInfo( string name, out EventInfo eventInfo )
    {
        return m_EventWrapper.TryGetEventInfo( name, out eventInfo );
    }

    public object GetEventHolder()
    {
        return m_EventWrapper.EventHolder;
    }

    public void Invoke( string name, DynamicToucanVariable[] m_FunctionArguments )
    {
        m_EventWrapper.Invoke( name, m_FunctionArguments );
    }

    public bool TryAddEventHandler( string name, ToucanChunkWrapper eventHandlerFunction, ToucanVm ToucanVm )
    {
        return m_EventWrapper.TryAddEventHandler( name, eventHandlerFunction, ToucanVm );
    }
    
   

    #endregion
}

}
