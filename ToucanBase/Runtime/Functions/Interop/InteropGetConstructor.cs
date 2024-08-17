using System;
using System.Reflection;
using Toucan.Runtime.Functions.ForeignInterface;
using Toucan.Runtime.Memory;

namespace Toucan.Runtime.Functions.Interop
{

public class InteropGetConstructor : InteropBase, IToucanVmCallable
    {

    #region Public

    public InteropGetConstructor()
    {
    }

    public InteropGetConstructor( TypeRegistry typeRegistry ) : base( typeRegistry )
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

        Type[] constructorArgTypes = new Type[arguments.Length - 1];

        int counter = 0;

        for ( int i = 1; i < arguments.Length; i++ )
        {
            if ( arguments[i].DynamicType == DynamicVariableType.String )
            {
                Type argType = ResolveType( arguments[i].StringData );

                if ( argType == null )
                {
                    throw new ToucanVmRuntimeException(
                        $"Runtime Error: Type: {arguments[i].StringData} not registered as a type!" );
                }

                constructorArgTypes[counter] = argType;

            }
            else
            {
                throw new ToucanVmRuntimeException( "Expected string" );
            }

            counter++;
        }

        ConstructorInfo constructorInfo = m_TypeRegistry.GetConstructor( type, constructorArgTypes );

        return new ConstructorInvoker( constructorInfo );
    }
    

    #endregion
}

}