using System;
using System.Reflection;
using Toucan.Runtime.Functions.ForeignInterface;
using Toucan.Runtime.Memory;

namespace Toucan.Runtime.Functions.Interop
{

public class InteropGetGenericMethod : InteropBase, IToucanVmCallable
{
    public InteropGetGenericMethod()
    {
    }

    public InteropGetGenericMethod( TypeRegistry typeRegistry ) : base( typeRegistry )
    {
    }

    public object Call( DynamicToucanVariable[] arguments )
    {
        if ( arguments[0].ObjectData is MethodInfo methodInvoker )
        {
            Type[] methodArgTypes = new Type[arguments.Length - 1];

            int counter = 0;

            for (int i = 1; i < arguments.Length; i++)
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

            MethodInfo genericMethod = methodInvoker.MakeGenericMethod( methodArgTypes );
            
            GenericMethodInvoker genericMethodInvoker = new GenericMethodInvoker( genericMethod );
        
            return genericMethodInvoker;
        }
        
        return null;
    }
}

}
