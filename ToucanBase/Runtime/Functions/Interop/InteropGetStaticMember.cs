using System;
using System.Reflection;
using Toucan.Runtime.Functions.ForeignInterface;
using Toucan.Runtime.Memory;

namespace Toucan.Runtime.Functions.Interop
{

public class InteropGetStaticMember : InteropBase, IToucanVmCallable
{
    public InteropGetStaticMember()
    {
    }

    public InteropGetStaticMember( TypeRegistry typeRegistry ) : base( typeRegistry )
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

        MemberInfo[] memberInfo =
            type.GetMember( arguments[1].StringData, BindingFlags.Public | BindingFlags.Static );

        if ( memberInfo.Length > 0 )
        {
            object obj = GetValue( memberInfo[0], null );

            return obj;
        }

        throw new ToucanVmRuntimeException(
            $"Runtime Error: member {arguments[0].StringData} not found on type {arguments[0].StringData}" );
    }

    private static object GetValue( MemberInfo memberInfo, object forObject )
    {
        switch ( memberInfo.MemberType )
        {
            case MemberTypes.Field:
                return ( ( FieldInfo ) memberInfo ).GetValue( forObject );

            case MemberTypes.Property:
                return ( ( PropertyInfo ) memberInfo ).GetValue( forObject );

            default:
                throw new NotImplementedException();
        }
    }
}

}