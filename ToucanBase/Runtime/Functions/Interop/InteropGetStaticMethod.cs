using System;
using System.Reflection;
using Toucan.Runtime.Functions.ForeignInterface;
using Toucan.Runtime.Memory;

namespace Toucan.Runtime.Functions.Interop
{

public class InteropGetStaticMethod : InteropBase, IToucanVmCallable
{
    public InteropGetStaticMethod()
    {
    }

    public InteropGetStaticMethod( TypeRegistry typeRegistry ) : base( typeRegistry )
    {
    }

    public object Call( DynamicToucanVariable[] arguments )
    {
        Type type = ResolveType( arguments[0].StringData );

        if ( type == null )
        {
            throw new ToucanVmRuntimeException(
                $"Runtime Error: Type: {arguments[0].StringData} not registered as a type!" );
        }

        Type[] methodArgTypes = new Type[arguments.Length - 2];

        int counter = 0;

        for (int i = 2; i < arguments.Length; i++)
        {
            if (arguments[i].DynamicType == DynamicVariableType.String)
            {
                Type argType = ResolveType( arguments[i].StringData );

                if (argType == null)
                {
                    throw new ToucanVmRuntimeException(
                        $"Runtime Error: Type: {arguments[i].StringData} not registered as a type!" );
                }

                methodArgTypes[counter] = argType;

            }
            else
            {
                throw new ToucanVmRuntimeException( "Expected string" );
            }

            counter++;
        }

        MethodInfo methodInfo = m_TypeRegistry.GetMethod( type, arguments[1].StringData, methodArgTypes );

        StaticMethodInvoker staticMethodInvoker = new StaticMethodInvoker( methodInfo );
        
        return staticMethodInvoker;
    }
}

}
