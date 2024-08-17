﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Toucan.Runtime.Memory;

namespace Toucan.Runtime.Functions.ForeignInterface
{

public class EventWrapper < V, T, K >
{
    private readonly Dictionary < string, EventInfo > m_EventInfos = new Dictionary < string, EventInfo >();

    public object EventHolder { get; set; }

    #region Public

    public EventWrapper( object obj )
    {
        EventHolder = obj;
        EventInfo[] eventInfos = obj.GetType().GetEvents();

        foreach ( EventInfo eventInfo in eventInfos )
        {
            m_EventInfos.Add( eventInfo.Name, eventInfo );
        }
    }

    public bool TryAddEventHandler( string name, ToucanChunkWrapper eventHandlerFunction, ToucanVm ToucanVm )
    {
        if ( m_EventInfos.TryGetValue( name, out EventInfo eventInfo ) )
        {
            MethodInfo methodInfo = eventInfo.EventHandlerType.GetMethod( "Invoke" );
            ParameterInfo[] parameterInfos = methodInfo.GetParameters();

            if ( parameterInfos.Length == 0 )
            {
                if ( methodInfo.ReturnParameter != null )
                {
                    Func < object > action = delegate
                    {
                        DynamicToucanVariable[] dynamicToucanVariables = new DynamicToucanVariable[0];

                        ToucanFunctionCall ToucanFunctionCall = new ToucanFunctionCall(
                            eventHandlerFunction,
                            dynamicToucanVariables );

                        ToucanVm.CallToucanFunction( ToucanFunctionCall );

                        return null;
                    };

                    Delegate del = action;
                    eventInfo.AddEventHandler( EventHolder, DelegateUtility.Cast( del, typeof( V ) ) );
                }
                else
                {
                    Action action = delegate
                    {
                        DynamicToucanVariable[] dynamicToucanVariables = new DynamicToucanVariable[0];

                        ToucanFunctionCall ToucanFunctionCall = new ToucanFunctionCall(
                            eventHandlerFunction,
                            dynamicToucanVariables );

                        ToucanVm.CallToucanFunction( ToucanFunctionCall );
                    };

                    Delegate del = action;
                    eventInfo.AddEventHandler( EventHolder, DelegateUtility.Cast( del, typeof( V ) ) );
                }
            }
            else if ( parameterInfos.Length == 1 )
            {
                if ( methodInfo.ReturnParameter != null )
                {
                    Func < T, object > action = delegate( T arg1 )
                    {
                        DynamicToucanVariable[] dynamicToucanVariables = new DynamicToucanVariable[1];

                        dynamicToucanVariables[0] = DynamicVariableExtension.ToDynamicVariable( arg1 );

                        ToucanFunctionCall ToucanFunctionCall = new ToucanFunctionCall(
                            eventHandlerFunction,
                            dynamicToucanVariables );

                        ToucanVm.CallToucanFunction( ToucanFunctionCall );

                        return null;
                    };

                    Delegate del = action;
                    eventInfo.AddEventHandler( EventHolder, DelegateUtility.Cast( del, typeof( V ) ) );
                }
                else
                {
                    Action < T > action = delegate( T arg1 )
                    {
                        DynamicToucanVariable[] dynamicToucanVariables = new DynamicToucanVariable[1];

                        dynamicToucanVariables[0] = DynamicVariableExtension.ToDynamicVariable( arg1 );

                        ToucanFunctionCall ToucanFunctionCall = new ToucanFunctionCall(
                            eventHandlerFunction,
                            dynamicToucanVariables );

                        ToucanVm.CallToucanFunction( ToucanFunctionCall );
                    };

                    Delegate del = action;
                    eventInfo.AddEventHandler( EventHolder, DelegateUtility.Cast( del, typeof( V ) ) );
                }
            }
            else if ( parameterInfos.Length == 2 )
            {
                if ( methodInfo.ReturnParameter != null )
                {
                    Func < T, K, object > action = delegate( T arg1, K k )
                    {
                        DynamicToucanVariable[] dynamicToucanVariables = new DynamicToucanVariable[2];

                        dynamicToucanVariables[0] = DynamicVariableExtension.ToDynamicVariable( arg1 );
                        dynamicToucanVariables[1] = DynamicVariableExtension.ToDynamicVariable( k );

                        ToucanFunctionCall ToucanFunctionCall = new ToucanFunctionCall(
                            eventHandlerFunction,
                            dynamicToucanVariables );

                        ToucanVm.CallToucanFunction( ToucanFunctionCall );

                        return null;
                    };

                    Delegate del = action;
                    eventInfo.AddEventHandler( EventHolder, DelegateUtility.Cast( del, typeof( V ) ) );
                }
                else
                {
                    Action < T, K > action = delegate( T arg1, K k )
                    {
                        DynamicToucanVariable[] dynamicToucanVariables = new DynamicToucanVariable[2];

                        dynamicToucanVariables[0] = DynamicVariableExtension.ToDynamicVariable( arg1 );
                        dynamicToucanVariables[1] = DynamicVariableExtension.ToDynamicVariable( k );

                        ToucanFunctionCall ToucanFunctionCall = new ToucanFunctionCall(
                            eventHandlerFunction,
                            dynamicToucanVariables );

                        ToucanVm.CallToucanFunction( ToucanFunctionCall );
                    };

                    Delegate del = action;
                    eventInfo.AddEventHandler( EventHolder, DelegateUtility.Cast( del, typeof( V ) ) );
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        return false;
    }

    public bool TryGetEventInfo( string name, out EventInfo eventInfo )
    {
        return m_EventInfos.TryGetValue( name, out eventInfo );
    }

    public void Invoke( string name, DynamicToucanVariable[] functionArguments )
    {
         if ( m_EventInfos.TryGetValue( name, out EventInfo eventInfo ) )
        {
            MethodInfo methodInfo = eventInfo.EventHandlerType.GetMethod( "Invoke" );
            ParameterInfo[] parameterInfos = methodInfo.GetParameters();
            
            if ( functionArguments.Length != parameterInfos.Length )
            {
                return;
            }
            else
            {
                object[] eventArgs = new object[functionArguments.Length];
                
                for ( int i = 0; i < functionArguments.Length; i++ )
                {
                    object arg = functionArguments[i].ToObject();

                    if ( arg is FastClassMemorySpace )
                    {
                        eventArgs[i] = arg;
                    }
                    else
                    {
                        eventArgs[i] = Convert.ChangeType( arg, parameterInfos[i].ParameterType );
                    }
                   
                }

                RaiseEventViaReflection( EventHolder, name, eventArgs );
                return;
            }
        }
    }
        
    private void RaiseEventViaReflection(object source, string eventName, object[] eventArgs)
    {
        if ( source != null )
        {
            ((Delegate)source
                       .GetType()
                       .GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic)
                       .GetValue(source))
                .DynamicInvoke(eventArgs);
        }
    }
    private void RaiseEventViaReflection(object source, string eventName)
    {
        ((Delegate)source
                   .GetType()
                   .GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic)
                   .GetValue(source))
            .DynamicInvoke(source, EventArgs.Empty);
    }
    #endregion
}

}
