using System;
using System.Reflection;
using Toucan.Runtime.Functions.ForeignInterface;
using Toucan.Runtime.Memory;

namespace Toucan.Runtime.Functions.Interop
{

public class StaticMethodInvoker : IToucanVmCallable
{
    private Type[] m_ArgTypes;
    private readonly FastMethodInfo m_FastMethodInfo;

    private MethodInfo m_OriginalMethodInfo;
    public StaticMethodInvoker( MethodInfo methodInfo )
    {
        OriginalMethodInfo = methodInfo;
        m_FastMethodInfo = new FastMethodInfo( methodInfo );

        ParameterInfo[] parameterInfos = methodInfo.GetParameters();
        m_ArgTypes = new Type[parameterInfos.Length];

        for (var i = 0; i < parameterInfos.Length; i++)
        {
            m_ArgTypes[i] = parameterInfos[i].ParameterType;
        }
    }

    public MethodInfo OriginalMethodInfo
    {
        get => m_OriginalMethodInfo;
        private set => m_OriginalMethodInfo = value;
    }

    public object Call( DynamicToucanVariable[] arguments )
    {
        object[] constructorArgs = new object[arguments.Length];

        for (int i = 0; i < arguments.Length; i++)
        {
            constructorArgs[i] = Convert.ChangeType(
                arguments[i].ToObject(),
                m_ArgTypes[i] );

        }

        object value = m_FastMethodInfo.Invoke( null, constructorArgs );

        return value;
    }

}

}
