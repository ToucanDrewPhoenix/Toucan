using System;
using Toucan.Runtime.Functions.ForeignInterface;
using Toucan.Runtime.Memory;

namespace Toucan.Runtime.Functions.Interop
{

public class InteropGetStaticClass : InteropBase, IToucanVmCallable
{
    public InteropGetStaticClass()
    {
    }

    public InteropGetStaticClass( TypeRegistry typeRegistry ) : base( typeRegistry )
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

        StaticWrapper wrapper = new StaticWrapper( type );

        return wrapper;
    }
}

}