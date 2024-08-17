using System;
using System.Reflection;
using Toucan.Runtime.Memory;

namespace Toucan.Runtime.Functions.Interop
{

public class ConstructorInvoker : IToucanVmCallable
{
    private ConstructorInfo m_ConstructorInfo;
    private Type[] m_ArgTypes;


    public ConstructorInvoker( ConstructorInfo constructorInfo )
    {
        m_ConstructorInfo = constructorInfo;
        ParameterInfo[] parameterInfos = m_ConstructorInfo.GetParameters();
        m_ArgTypes = new Type[parameterInfos.Length];

        for ( var i = 0; i < parameterInfos.Length; i++ )
        {
            m_ArgTypes[i] = parameterInfos[i].ParameterType;
        }
    }

    public object Call( DynamicToucanVariable[] arguments )
    {
        object[] constructorArgs = new object[arguments.Length];

        for ( int i = 0; i < arguments.Length; i++ )
        {
            constructorArgs[i] = Convert.ChangeType(
                arguments[i].ToObject(),
                m_ArgTypes[i] );

        }

        object classObject = m_ConstructorInfo.Invoke( constructorArgs );

        return classObject;
    }

}

}